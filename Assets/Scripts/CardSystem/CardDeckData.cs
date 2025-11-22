using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 卡牌数据条目（卡牌ID和数量）
    /// </summary>
    [Serializable]
    public class CardDeckEntry
    {
        [Tooltip("卡牌ID（用于查找对应的Prefab）")]
        public string cardId;

        [Tooltip("该卡牌在卡组中的数量")]
        public int count = 1;

        public CardDeckEntry(string cardId, int count = 1)
        {
            this.cardId = cardId;
            this.count = count;
        }
    }

    /// <summary>
    /// 卡组数据（用于运行时动态构建卡组）
    /// 存储卡牌ID和数量，而不是直接存储Prefab引用
    /// </summary>
    [CreateAssetMenu(fileName = "New Deck Data", menuName = "Card System/Deck Data")]
    public class CardDeckData : ScriptableObject
    {
        [Header("卡组数据")]
        [Tooltip("卡组名称")]
        public string deckName = "New Deck";

        [Tooltip("卡组中的卡牌列表（ID和数量）")]
        public List<CardDeckEntry> cards = new List<CardDeckEntry>();

        /// <summary>
        /// 获取卡组中的总卡牌数量
        /// </summary>
        public int TotalCardCount
        {
            get
            {
                int total = 0;
                foreach (var entry in cards)
                {
                    total += entry.count;
                }
                return total;
            }
        }

        /// <summary>
        /// 获取卡组中的卡牌种类数量
        /// </summary>
        public int UniqueCardCount => cards.Count;
    }
}

