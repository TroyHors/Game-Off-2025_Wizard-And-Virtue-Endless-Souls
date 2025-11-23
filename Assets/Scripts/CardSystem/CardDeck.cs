using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 卡组管理器（局外）
    /// 负责管理游戏外的卡组，支持添加和删除卡牌
    /// </summary>
    [System.Serializable]
    public class CardDeck
    {
        [SerializeField] private List<GameObject> cards = new List<GameObject>();

        /// <summary>
        /// 卡组中的卡牌数量
        /// </summary>
        public int Count => cards.Count;

        /// <summary>
        /// 获取卡组中的所有卡牌（只读）
        /// </summary>
        public IReadOnlyList<GameObject> Cards => cards;

        /// <summary>
        /// 添加卡牌到卡组
        /// </summary>
        /// <param name="cardPrefab">要添加的卡牌Prefab</param>
        /// <returns>是否添加成功</returns>
        public bool AddCard(GameObject cardPrefab)
        {
            if (cardPrefab == null)
            {
                Debug.LogWarning("[CardDeck] 尝试添加空卡牌到卡组");
                return false;
            }

            cards.Add(cardPrefab);
            return true;
        }

        /// <summary>
        /// 批量添加卡牌到卡组
        /// </summary>
        /// <param name="cardPrefabs">要添加的卡牌Prefab列表</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int AddCards(List<GameObject> cardPrefabs)
        {
            if (cardPrefabs == null)
            {
                Debug.LogWarning("[CardDeck] 尝试添加空的卡牌列表到卡组");
                return 0;
            }

            int addedCount = 0;
            foreach (var cardPrefab in cardPrefabs)
            {
                if (cardPrefab != null)
                {
                    cards.Add(cardPrefab);
                    addedCount++;
                }
            }

            return addedCount;
        }

        /// <summary>
        /// 批量添加卡牌到卡组
        /// </summary>
        /// <param name="cardPrefabs">要添加的卡牌Prefab数组</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int AddCards(GameObject[] cardPrefabs)
        {
            if (cardPrefabs == null)
            {
                Debug.LogWarning("[CardDeck] 尝试添加空的卡牌数组到卡组");
                return 0;
            }

            return AddCards(new List<GameObject>(cardPrefabs));
        }

        /// <summary>
        /// 从列表初始化卡组（清空现有卡组后添加）
        /// </summary>
        /// <param name="cardPrefabs">卡牌Prefab列表</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int InitializeFromList(List<GameObject> cardPrefabs)
        {
            Clear();
            return AddCards(cardPrefabs);
        }

        /// <summary>
        /// 从数组初始化卡组（清空现有卡组后添加）
        /// </summary>
        /// <param name="cardPrefabs">卡牌Prefab数组</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int InitializeFromArray(GameObject[] cardPrefabs)
        {
            Clear();
            return AddCards(cardPrefabs);
        }

        /// <summary>
        /// 从卡组中移除卡牌
        /// </summary>
        /// <param name="cardPrefab">要移除的卡牌Prefab</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveCard(GameObject cardPrefab)
        {
            if (cardPrefab == null)
            {
                Debug.LogWarning("[CardDeck] 尝试移除空卡牌");
                return false;
            }

            return cards.Remove(cardPrefab);
        }

        /// <summary>
        /// 从卡组中移除指定索引的卡牌
        /// </summary>
        /// <param name="index">卡牌索引</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveCardAt(int index)
        {
            if (index < 0 || index >= cards.Count)
            {
                Debug.LogWarning($"[CardDeck] 索引 {index} 超出范围，卡组大小为 {cards.Count}");
                return false;
            }

            cards.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 清空卡组
        /// </summary>
        public void Clear()
        {
            cards.Clear();
        }

        /// <summary>
        /// 检查卡组是否为空
        /// </summary>
        public bool IsEmpty()
        {
            return cards.Count == 0;
        }

        /// <summary>
        /// 创建卡组的深拷贝（用于局内使用）
        /// </summary>
        /// <returns>卡组的副本</returns>
        public List<GameObject> CreateCopy()
        {
            return new List<GameObject>(cards);
        }
    }
}

