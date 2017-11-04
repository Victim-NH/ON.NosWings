﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Domain;

namespace OpenNos.GameObject
{
    public class CharacterQuest : CharacterQuestDTO
    {
        #region Members

        private Quest _quest;

        #endregion

        #region Instantiation

        public CharacterQuest(long questId, long characterId)
        {
            QuestId = questId;
            CharacterId = characterId;
        }

        public CharacterQuest(CharacterQuestDTO characterQuestDto)
        {
            Id = characterQuestDto.Id;
            FirstObjective = characterQuestDto.FirstObjective ?? 0;
            SecondObjective = characterQuestDto.SecondObjective ?? 0;
            ThirdObjective = characterQuestDto.ThirdObjective ?? 0;
            QuestId = characterQuestDto.QuestId;
            CharacterId = characterQuestDto.CharacterId;
        }

        #endregion

        #region Properties

        public Quest Quest
        {
            get { return _quest ?? (_quest = ServerManager.Instance.GetQuest(QuestId)); }
        }

        public bool RewardInWaiting { get; set; }

        public List<QuestRewardDTO> QuestRewards { get; set; }

        public short QuestNumber { get; set; }

        #endregion

        #region Methods

        #endregion
    }
}
