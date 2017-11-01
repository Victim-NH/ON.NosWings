﻿/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class BCard : BCardDTO
    {
        #region Methods

        public void ApplyBCards(object session, object caster = null)
        {
            switch ((BCardType.CardType)Type)
            {
                case BCardType.CardType.Buff:
                    if (session.GetType() == typeof(Character))
                    {
                        if (ServerManager.Instance.RandomNumber() < FirstData)
                        {
                            Character character = session as Character;
                            character?.AddBuff(new Buff(SecondData, character.Level));
                        }
                    }
                    else if (session.GetType() == typeof(MapMonster))
                    {
                        if (ServerManager.Instance.RandomNumber() < FirstData)
                        {
                            if (session is MapMonster monster)
                            {
                                monster.AddBuff(!(caster is Character character) ? new Buff(SecondData, 1) : new Buff(SecondData, character.Level));
                            }
                        }
                    }
                    /*
                    else if (session is Mate mate)
                    {
                        if (ServerManager.Instance.RandomNumber() < FirstData)
                        {
                            mate?.AddBuff(new Buff(SecondData, (byte)(caster is Character character ? character.Level : 1)));
                        }
                    }
                    */
                    break;

                case BCardType.CardType.Move:
                    if (session.GetType() == typeof(Character))
                    {
                        if (session is Character character)
                        {
                            character.LastSpeedChange = DateTime.Now;
                            character.LoadSpeed();
                        }
                        Character o = session as Character;
                        o?.Session.SendPacket(o.GenerateCond());
                    }
                    break;

                case BCardType.CardType.Summons:
                    if (session.GetType() == typeof(Character))
                    {
                    }
                    else if (session.GetType() == typeof(MapMonster))
                    {
                        if (!(session is MapMonster monster))
                        {
                            return;
                        }
                        ConcurrentBag<MonsterToSummon> summonParameters = new ConcurrentBag<MonsterToSummon>();
                        for (int i = 0; i < FirstData; i++)
                        {
                            short x, y;
                            if (SubType == 11)
                            {
                                x = (short)(i + monster.MapX);
                                y = monster.MapY;
                            }
                            else
                            {
                                x = (short)(ServerManager.Instance.RandomNumber(-3, 3) + monster.MapX);
                                y = (short)(ServerManager.Instance.RandomNumber(-3, 3) + monster.MapY);
                            }
                            summonParameters.Add(new MonsterToSummon((short)SecondData, new MapCell { X = x, Y = y }, null, true));
                        }
                        int rnd = ServerManager.Instance.RandomNumber();
                        if (rnd <= Math.Abs(ThirdData) || ThirdData == 0)
                        {
                            switch (SubType)
                            {
                                case 31:
                                        EventHelper.Instance.RunEvent(new EventContainer(monster.MapInstance, EventActionType.SPAWNMONSTERS, summonParameters));
                                    break;
                                default:
                                    if (monster.OnDeathEvents.All(s => s?.EventActionType != EventActionType.SPAWNMONSTERS))
                                    {
                                        monster.OnDeathEvents.Add(new EventContainer(monster.MapInstance, EventActionType.SPAWNMONSTERS, summonParameters));
                                    }
                                    break;
                            }
                        }
                    }
                    else if (session.GetType() == typeof(MapNpc))
                    {
                    }
                    else if (session.GetType() == typeof(Mate))
                    {
                    }
                    break;

                case BCardType.CardType.SpecialAttack:
                    break;

                case BCardType.CardType.SpecialDefence:
                    break;

                case BCardType.CardType.AttackPower:
                    break;

                case BCardType.CardType.Target:
                    break;

                case BCardType.CardType.Critical:
                    break;

                case BCardType.CardType.SpecialCritical:
                    break;

                case BCardType.CardType.Element:
                    break;

                case BCardType.CardType.IncreaseDamage:
                    break;

                case BCardType.CardType.Defence:
                    break;

                case BCardType.CardType.DodgeAndDefencePercent:
                    break;

                case BCardType.CardType.Block:
                    break;

                case BCardType.CardType.Absorption:
                    break;

                case BCardType.CardType.ElementResistance:
                    break;

                case BCardType.CardType.EnemyElementResistance:
                    break;

                case BCardType.CardType.Damage:
                    break;

                case BCardType.CardType.GuarantedDodgeRangedAttack:
                    break;

                case BCardType.CardType.Morale:
                    break;

                case BCardType.CardType.Casting:
                    break;

                case BCardType.CardType.Reflection:
                    break;

                case BCardType.CardType.DrainAndSteal:
                    break;

                case BCardType.CardType.HealingBurningAndCasting:
                    AdditionalTypes.HealingBurningAndCasting subtype = (AdditionalTypes.HealingBurningAndCasting) SubType;
                    switch (subtype)
                    {
                        case AdditionalTypes.HealingBurningAndCasting.RestoreHP:
                        case AdditionalTypes.HealingBurningAndCasting.RestoreHPWhenCasting:
                            if (session is Character sess)
                            {
                                int heal = FirstData;
                                bool change = false;
                                if (IsLevelScaled)
                                {
                                    if (IsLevelDivided)
                                    {
                                        heal /= sess.Level;
                                    }
                                    else
                                    {
                                        heal *= sess.Level;
                                    }
                                }
                                sess.Session?.CurrentMapInstance?.Broadcast(sess.GenerateRc(heal));
                                if (sess.Hp + heal < sess.HpLoad())
                                {
                                    sess.Hp += heal;
                                    change = true;
                                }
                                else
                                {
                                    if (sess.Hp != (int)sess.HpLoad())
                                    {
                                        change = true;
                                    }
                                    sess.Hp = (int)sess.HpLoad();
                                }
                                if (change)
                                {
                                    sess.Session?.SendPacket(sess.GenerateStat());
                                }
                            }
                            break;
                    }
                    break;

                case BCardType.CardType.HPMP:
                    break;

                case BCardType.CardType.SpecialisationBuffResistance:
                    break;

                case BCardType.CardType.SpecialEffects:
                    break;

                case BCardType.CardType.Capture:
                    if (session.GetType() == typeof(MapMonster))
                    {
                        if (caster is Character)
                        {
                            MapMonster monster = session as MapMonster;
                            Character character = caster as Character;
                            if (monster != null)
                            {
                                if (monster.Monster.RaceType == 1 && (character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance || character.MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance))
                                {
                                    if (monster.Monster.Level < character.Level)
                                    {
                                        if (monster.CurrentHp < (monster.Monster.MaxHP / 2))
                                        {
                                            if (character.MaxMateCount > character.Mates.Count())
                                            {
                                                // Algo  
                                                int capturerate = 100 - (monster.CurrentHp / monster.Monster.MaxHP + 1) / 2;
                                                if (ServerManager.Instance.RandomNumber() <= capturerate)
                                                {
                                                    Mate currentmate = character.Mates?.FirstOrDefault(m => m.IsTeamMember && m.MateType == MateType.Pet);
                                                    monster.MapInstance.DespawnMonster(monster);
                                                    NpcMonster mateNpc = ServerManager.Instance.GetNpc(monster.Monster.NpcMonsterVNum);
                                                    Mate mate = new Mate(character, mateNpc, 1, MateType.Pet);
                                                    character.Mates.Add(mate);
                                                    mate.RefreshStats();
                                                    if (currentmate == null)
                                                    {
                                                        mate.IsTeamMember = true;
                                                        character.Session.SendPacket($"ctl 2 {mate.PetId} 3");
                                                        character.MapInstance.Broadcast(mate.GenerateIn());
                                                    }
                                                    else
                                                    {
                                                        MapCell pos = character.Miniland.Map.GetRandomPosition();
                                                        mate.PositionX = pos.X;
                                                        mate.PositionY = pos.Y;
                                                    }
                                                    character.Session.SendPacket(character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("YOU_GET_PET"), mate.Name), 0));
                                                    character.Session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());
                                                    character.Session.SendPackets(character.GenerateScP());
                                                    character.Session.SendPackets(character.GenerateScN());
                                                    character.Session.SendPacket(character.GeneratePinit());
                                                    character.Session.SendPackets(character.Mates.Where(s => s.IsTeamMember)
                                                        .OrderBy(s => s.MateType)
                                                        .Select(s => s.GeneratePst()));
                                                }
                                                else { character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CAPTURE_FAILED"), 0)); }
                                            }
                                            else { character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_MATES_COUNT"), 0)); }
                                        }
                                        else { character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MONSTER_MUST_BE_LOW_HP"), 0)); }
                                    }
                                    else { character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MONSTER_LVL_MUST_BE_LESS"), 0)); }
                                }
                                else { character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MONSTER_CANNOT_BE_CAPTURED"), 0)); }
                            }
                        }
                    }
                    break;

                case BCardType.CardType.SpecialDamageAndExplosions:
                    break;

                case BCardType.CardType.SpecialEffects2:
                    break;

                case BCardType.CardType.CalculatingLevel:
                    break;

                case BCardType.CardType.Recovery:
                    break;

                case BCardType.CardType.MaxHPMP:
                    break;

                case BCardType.CardType.MultAttack:
                    break;

                case BCardType.CardType.MultDefence:
                    break;

                case BCardType.CardType.TimeCircleSkills:
                    break;

                case BCardType.CardType.RecoveryAndDamagePercent:
                    break;

                case BCardType.CardType.Count:
                    break;

                case BCardType.CardType.NoDefeatAndNoDamage:
                    break;

                case BCardType.CardType.SpecialActions:
                    if (session is Character charact)
                    {
                        if (SubType.Equals((byte) AdditionalTypes.SpecialActions.Hide))
                        {
                            charact.Invisible = true;
                            charact.Mates.Where(s => s.IsTeamMember).ToList().ForEach(s => charact.Session.CurrentMapInstance?.Broadcast(s.GenerateOut()));
                            charact.Session.CurrentMapInstance?.Broadcast(charact.GenerateInvisible());
                        }
                    }
                    break;

                case BCardType.CardType.Mode:
                    break;

                case BCardType.CardType.NoCharacteristicValue:
                    break;

                case BCardType.CardType.LightAndShadow:
                    break;

                case BCardType.CardType.Item:
                    break;

                case BCardType.CardType.DebuffResistance:
                    break;

                case BCardType.CardType.SpecialBehaviour:
                    break;

                case BCardType.CardType.Quest:
                    break;

                case BCardType.CardType.SecondSPCard:
                    break;

                case BCardType.CardType.SPCardUpgrade:
                    break;

                case BCardType.CardType.HugeSnowman:
                    break;

                case BCardType.CardType.Drain:
                    break;

                case BCardType.CardType.BossMonstersSkill:
                    break;

                case BCardType.CardType.LordHatus:
                    break;

                case BCardType.CardType.LordCalvinas:
                    break;

                case BCardType.CardType.SESpecialist:
                    break;

                case BCardType.CardType.FourthGlacernonFamilyRaid:
                    break;

                case BCardType.CardType.SummonedMonsterAttack:
                    break;

                case BCardType.CardType.BearSpirit:
                    break;

                case BCardType.CardType.SummonSkill:
                    break;

                case BCardType.CardType.InflictSkill:
                    break;

                case BCardType.CardType.HideBarrelSkill:
                    break;

                case BCardType.CardType.FocusEnemyAttentionSkill:
                    break;

                case BCardType.CardType.TauntSkill:
                    break;

                case BCardType.CardType.FireCannoneerRangeBuff:
                    break;

                case BCardType.CardType.VulcanoElementBuff:
                    break;

                case BCardType.CardType.DamageConvertingSkill:
                    break;

                case BCardType.CardType.MeditationSkill:
                    if (session.GetType() == typeof(Character))
                    {
                        if (SubType.Equals((byte) AdditionalTypes.MeditationSkill.CausingChance))
                        {
                            if (ServerManager.Instance.RandomNumber() < FirstData)
                            {
                                Character character = (session as Character);
                                if (character == null)
                                {
                                    break;
                                }
                                if (SkillVNum.HasValue)
                                {
                                    character.LastSkillCombo = DateTime.Now;
                                    Skill skill = ServerManager.Instance.GetSkill(SkillVNum.Value);
                                    Skill newSkill = ServerManager.Instance.GetSkill((short) SecondData);
                                    Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(observer =>
                                    {
                                        foreach (QuicklistEntryDTO qe in character.QuicklistEntries.Where(s =>
                                            s.Pos.Equals(skill.CastId)))
                                        {
                                            character.Session.SendPacket(
                                                $"qset {qe.Q1} {qe.Q2} {qe.Type}.{qe.Slot}.{newSkill.CastId}.0");
                                        }
                                        character.Session.SendPacket($"mslot {newSkill.CastId} -1");
                                    });

                                    if (skill.CastId > 10)
                                    {
                                        // HACK this way
                                        Observable.Timer(TimeSpan.FromMilliseconds(skill.Cooldown * 100 + 500))
                                            .Subscribe(observer =>
                                            {
                                                character.Session.SendPacket($"sr {skill.CastId}");
                                            });
                                    }
                                }
                            }
                        }
                        else
                        {
                            Character character = (session as Character);
                            if (character == null)
                            {
                                break;
                            }
                            switch (SubType)
                            {
                                case 21:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.Now.AddSeconds(4);
                                    break;
                                case 31:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.Now.AddSeconds(8);
                                    break;
                                case 41:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.Now.AddSeconds(12);
                                    break;
                            }
                        }
                    }
                    break;

                case BCardType.CardType.FalconSkill:
                    break;

                case BCardType.CardType.AbsorptionAndPowerSkill:
                    break;

                case BCardType.CardType.LeonaPassiveSkill:
                    break;

                case BCardType.CardType.FearSkill:
                    break;

                case BCardType.CardType.SniperAttack:
                    break;

                case BCardType.CardType.FrozenDebuff:
                    break;

                case BCardType.CardType.JumpBackPush:
                    break;

                case BCardType.CardType.FairyXPIncrease:
                    break;

                case BCardType.CardType.SummonAndRecoverHP:
                    break;

                case BCardType.CardType.TeamArenaBuff:
                    break;

                case BCardType.CardType.ArenaCamera:
                    break;

                case BCardType.CardType.DarkCloneSummon:
                    break;

                case BCardType.CardType.AbsorbedSpirit:
                    break;

                case BCardType.CardType.AngerSkill:
                    break;

                case BCardType.CardType.MeteoriteTeleport:
                    break;

                case BCardType.CardType.StealBuff:
                    break;

                default:
                    Logger.Error(new ArgumentOutOfRangeException($"Card Type {Type} not defined!"));
                    //throw new ArgumentOutOfRangeException();
                    break;
            }
        }

        public override void Initialize()
        {
        }

        #endregion
    }
}