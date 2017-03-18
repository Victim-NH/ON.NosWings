﻿using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Domain;
using System;
using OpenNos.Core;
using System.Xml;
using System.Linq;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject
{
    public class ScriptedInstance : ScriptedInstanceDTO
    {
        public ScriptedInstanceType Type { get; set; }
        public byte LevelMinimum { get; set; }
        public byte LevelMaximum { get; set; }
        public string Label { get; set; }
        public MapInstance FirstMap { get; set; }

        InstanceBag _instancebag = new InstanceBag();

        Dictionary<int, MapInstance> _mapinstancedictionary = new Dictionary<int, MapInstance>();

        public string GenerateRbr()
        {
            return $"rbr 0.0.0 4 15 {LevelMinimum}.{LevelMaximum} 0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 -1.0 {WinnerScore}.{(WinnerScore > 0 ? Winner : "")} 0 0 {Language.Instance.GetMessageFromKey("TS_TUTORIAL")}\n{Label}";
        }

        public void Dispose()
        {
            //TODO disposing all maps
        }

        public void LoadScript()
        {
            XmlDocument doc = new XmlDocument();
            if (Script != null)
            {
                doc.LoadXml(Script);
                XmlNode InstanceEvents = doc.SelectSingleNode("Definition");
                //CreateMaps
                foreach (XmlNode variable in InstanceEvents.SelectSingleNode("InstanceEvents").ChildNodes)
                {
                    if (variable.Name == "CreateMap")
                    {
                        MapInstance newmap = ServerManager.Instance.GenerateMapInstance(short.Parse(variable?.Attributes["VNum"].Value), MapInstanceType.TimeSpaceInstance, _instancebag);
                        newmap.MapIndexX = byte.Parse(variable?.Attributes["IndexX"].Value);
                        newmap.MapIndexY = byte.Parse(variable?.Attributes["IndexY"].Value);
                        if (!_mapinstancedictionary.ContainsKey(int.Parse(variable?.Attributes["Map"].Value)))
                        {
                            _mapinstancedictionary.Add(int.Parse(variable?.Attributes["Map"].Value), newmap);
                        }
                    }
                }

                FirstMap = _mapinstancedictionary.Values.FirstOrDefault();
                GenerateEvent(InstanceEvents, FirstMap);

            }
        }

        private List<EventContainer> GenerateEvent(XmlNode node, MapInstance parentmapinstance)
        {
            List<EventContainer> evts = new List<EventContainer>();


            foreach (XmlNode mapevent in node.ChildNodes)
            {
                if (mapevent.Name == "#comment")
                    continue;
                int mapid = -1;
                short positionX = -1;
                short positionY = -1;
                bool isHostile = true;
                bool isBonus;
                bool isTarget;
                bool isMate;
                bool isProtected;
                bool move;
                if (!int.TryParse(mapevent.Attributes["Map"]?.Value, out mapid))
                {
                    mapid = -1;
                }
                if (!short.TryParse(mapevent.Attributes["PositionX"]?.Value, out positionX))
                {
                    positionX = -1;
                }
                if (!short.TryParse(mapevent.Attributes["PositionY"]?.Value, out positionY))
                {
                    positionY = -1;
                }
                bool.TryParse(mapevent?.Attributes["IsTarget"]?.Value, out isTarget);
                bool.TryParse(mapevent?.Attributes["IsBonus"]?.Value, out isBonus);
                bool.TryParse(mapevent?.Attributes["IsProtected"]?.Value, out isProtected);
                bool.TryParse(mapevent?.Attributes["IsMate"]?.Value, out isMate);
                if (!bool.TryParse(mapevent?.Attributes["Move"]?.Value, out move))
                {
                    move = true;
                }
                if (!bool.TryParse(mapevent?.Attributes["IsHostile"]?.Value, out isHostile))
                {
                    isHostile = true;
                }
                MapInstance mapinstance = _mapinstancedictionary.FirstOrDefault(s => s.Key == mapid).Value;
                if (mapinstance == null)
                {
                    mapinstance = parentmapinstance;
                }
                MapCell cell = mapinstance?.Map?.GetRandomPosition();
                if (cell != null && (positionX == -1 || positionY == -1))
                {
                    positionX = cell.X;
                    positionY = cell.Y;
                }
                switch (mapevent.Name)
                {
                    //master events
                    case "CreateMap":
                    case "InstanceEvents":
                        GenerateEvent(mapevent, mapinstance).ForEach(e => EventHelper.Instance.RunEvent(e));
                        break;

                    //register events
                    case "OnCharacterDiscoveringMap":
                    case "OnMoveOnMap":
                    case "OnMapClean":
                        evts.Add(new EventContainer(mapinstance, EventActionType.REGISTEREVENT, new Tuple<string, List<EventContainer>>(mapevent.Name, GenerateEvent(mapevent, mapinstance))));
                        break;

                    //child events
                    case "OnDeath":
                        evts.AddRange(GenerateEvent(mapevent, mapinstance));
                        break;

                    case "SummonMonsters":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNMONSTERS, mapinstance.Map.GenerateMonsters(short.Parse(mapevent?.Attributes["VNum"].Value), short.Parse(mapevent?.Attributes["Amount"].Value), move, new List<EventContainer>(), isBonus, isHostile)));
                        break;

                    case "SummonMonster":
                        List<MonsterToSummon> lst = new List<MonsterToSummon>();
                        lst.Add(new MonsterToSummon(short.Parse(mapevent?.Attributes["VNum"].Value), new MapCell() { X = positionX, Y = positionY }, -1, move, GenerateEvent(mapevent, mapinstance), isTarget, isBonus, isHostile));
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNMONSTERS, lst.AsEnumerable()));
                        break;

                    case "SummonNps":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNNPCS, mapinstance.Map.GenerateNpcs(short.Parse(mapevent?.Attributes["VNum"].Value), short.Parse(mapevent?.Attributes["Amount"].Value), new List<EventContainer>(), isMate, isProtected)));
                        break;

                    case "SummonNpc":
                        List<NpcToSummon> lstn = new List<NpcToSummon>();
                        lstn.Add(new NpcToSummon(short.Parse(mapevent?.Attributes["VNum"].Value), new MapCell() { X = positionX, Y = positionY }, -1, GenerateEvent(mapevent, mapinstance), isMate, isProtected));
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNNPCS, lstn.AsEnumerable()));
                        break;

                    case "SpawnButton":
                        MapButton button = new MapButton(
                            int.Parse(mapevent?.Attributes["Id"].Value),
                            positionX,
                           positionY,
                             short.Parse(mapevent?.Attributes["VNumEnabled"].Value),
                              short.Parse(mapevent?.Attributes["VNumDisabled"].Value),
                              new List<EventContainer>(),
                               new List<EventContainer>(),
                                new List<EventContainer>()
                            );
                        foreach (XmlNode var in mapevent.ChildNodes)
                        {
                            switch (var.Name)
                            {
                                case "OnFirstEnable":
                                    button.FirstEnableEvents.AddRange(GenerateEvent(var, mapinstance));
                                    break;
                                case "OnEnable":
                                    button.EnableEvents.AddRange(GenerateEvent(var, mapinstance));
                                    break;
                                case "OnDisable":
                                    button.DisableEvents.AddRange(GenerateEvent(var, mapinstance));
                                    break;
                            }
                        }
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNBUTTON, button));
                        break;


                    case "StopClock":
                        evts.Add(new EventContainer(mapinstance, EventActionType.STOPCLOCK, null));
                        break;

                    case "StopMapClock":
                        evts.Add(new EventContainer(mapinstance, EventActionType.STOPMAPCLOCK, null));
                        break;

                    case "RefreshMapItems":
                        evts.Add(new EventContainer(mapinstance, EventActionType.REFRESHMAPITEMS, null));
                        break;

                    case "ChangePortalType":
                        evts.Add(new EventContainer(mapinstance, EventActionType.CHANGEPORTALTYPE, new Tuple<int, PortalType>(int.Parse(mapevent?.Attributes["IdOnMap"].Value), (PortalType)sbyte.Parse(mapevent?.Attributes["Type"].Value))));
                        break;

                    case "SendPacket":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SENDPACKET, mapevent?.Attributes["Value"].Value));
                        break;

                    case "NpcDialog":
                        evts.Add(new EventContainer(mapinstance, EventActionType.NPCDIALOG, int.Parse(mapevent?.Attributes["Value"].Value)));
                        break;

                    case "SendMessage":
                        evts.Add(new EventContainer(mapinstance, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(mapevent?.Attributes["Value"].Value, byte.Parse(mapevent?.Attributes["Type"].Value))));
                        break;

                    case "GenerateClock":
                        evts.Add(new EventContainer(mapinstance, EventActionType.CLOCK, int.Parse(mapevent?.Attributes["Value"].Value)));
                        break;

                    case "GenerateMapClock":
                        evts.Add(new EventContainer(mapinstance, EventActionType.MAPCLOCK, int.Parse(mapevent?.Attributes["Value"].Value)));
                        break;

                    case "StartClock":
                        Tuple<List<EventContainer>, List<EventContainer>> eve = new Tuple<List<EventContainer>, List<EventContainer>>(new List<EventContainer>(), new List<EventContainer>());
                        foreach (XmlNode var in mapevent.ChildNodes)
                        {
                            switch (var.Name)
                            {
                                case "OnTimeout":
                                    eve.Item1.AddRange(GenerateEvent(var, mapinstance));
                                    break;
                                case "OnStop":
                                    eve.Item2.AddRange(GenerateEvent(var, mapinstance));
                                    break;
                            }
                        }
                        evts.Add(new EventContainer(mapinstance, EventActionType.STARTCLOCK, eve));
                        break;

                    case "StartMapClock":
                        eve = new Tuple<List<EventContainer>, List<EventContainer>>(new List<EventContainer>(), new List<EventContainer>());
                        foreach (XmlNode var in mapevent.ChildNodes)
                        {
                            switch (var.Name)
                            {
                                case "OnTimeout":
                                    eve.Item1.AddRange(GenerateEvent(var, mapinstance));
                                    break;
                                case "OnStop":
                                    eve.Item2.AddRange(GenerateEvent(var, mapinstance));
                                    break;
                            }
                        }
                        evts.Add(new EventContainer(mapinstance, EventActionType.STARTMAPCLOCK, eve));
                        break;

                    case "SpawnPortal":
                        MapInstance destmap = _mapinstancedictionary.First(s => s.Key == int.Parse(mapevent?.Attributes["MapTo"].Value)).Value;
                        Portal portal = new Portal()
                        {
                            PortalId = byte.Parse(mapevent?.Attributes["IdOnMap"].Value),
                            SourceX = positionX,
                            SourceY = positionY,
                            Type = short.Parse(mapevent?.Attributes["Type"].Value),
                            DestinationX = short.Parse(mapevent?.Attributes["ToX"].Value),
                            DestinationY = short.Parse(mapevent?.Attributes["ToY"].Value),
                            SourceMapInstanceId = mapinstance.MapInstanceId,
                            DestinationMapInstanceId = destmap.MapInstanceId,
                        };
                        evts.Add(new EventContainer(mapinstance, EventActionType.SPAWNPORTAL, portal));
                        break;
                }

            }
            return evts;
        }

        public string GenerateWp()
        {
            return $"wp {PositionX} {PositionY} {ScriptedInstanceId} 0 {LevelMinimum} {LevelMaximum}"; 
        }

        public List<string> GenerateMinimap()
        {
            List<string> lst = new List<string>();
            lst.Add("rsfm 0 0 4 12");
            _mapinstancedictionary.Values.ToList().ForEach(s => lst.Add(s.GenerateRsfn(true)));
            return lst;
        }

        public void LoadGlobals()
        {
            XmlDocument doc = new XmlDocument();
            if (Script != null)
            {
                doc.LoadXml(Script);
                XmlNode def = doc.SelectSingleNode("Definition").SelectSingleNode("Globals");
                LevelMinimum = byte.Parse(def.SelectSingleNode("LevelMinimum")?.Attributes["Value"].Value);
                LevelMaximum = byte.Parse(def.SelectSingleNode("LevelMaximum")?.Attributes["Value"].Value);
                Label = def.SelectSingleNode("Label")?.Attributes["Value"].Value;
            }
        }
    }
}