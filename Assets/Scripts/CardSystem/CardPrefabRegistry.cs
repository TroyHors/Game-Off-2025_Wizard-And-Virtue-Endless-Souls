using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 卡牌Prefab注册表
    /// 将卡牌ID映射到对应的Prefab
    /// </summary>
    [CreateAssetMenu(fileName = "Card Prefab Registry", menuName = "Card System/Card Prefab Registry")]
    public class CardPrefabRegistry : ScriptableObject
    {
        [System.Serializable]
        public class CardPrefabEntry
        {
            [Tooltip("卡牌ID（唯一标识符）")]
            public string cardId;

            [Tooltip("对应的卡牌Prefab")]
            public GameObject cardPrefab;
        }

        [Header("卡牌Prefab注册表")]
        [Tooltip("卡牌ID到Prefab的映射列表")]
        [SerializeField] private List<CardPrefabEntry> cardEntries = new List<CardPrefabEntry>();

        /// <summary>
        /// 卡牌Prefab字典（运行时使用）
        /// </summary>
        private Dictionary<string, GameObject> cardPrefabDict;

        /// <summary>
        /// 初始化注册表（构建字典）
        /// </summary>
        private void Initialize()
        {
            if (cardPrefabDict != null)
            {
                return; // 已经初始化
            }

            cardPrefabDict = new Dictionary<string, GameObject>();
            foreach (var entry in cardEntries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.cardId) && entry.cardPrefab != null)
                {
                    if (cardPrefabDict.ContainsKey(entry.cardId))
                    {
                        Debug.LogWarning($"[CardPrefabRegistry] 卡牌ID '{entry.cardId}' 重复，将使用最后一个Prefab");
                    }
                    cardPrefabDict[entry.cardId] = entry.cardPrefab;
                }
            }
        }

        /// <summary>
        /// 根据卡牌ID获取Prefab
        /// </summary>
        /// <param name="cardId">卡牌ID</param>
        /// <returns>卡牌Prefab，如果未找到则返回null</returns>
        public GameObject GetCardPrefab(string cardId)
        {
            Initialize();

            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogWarning("[CardPrefabRegistry] 尝试获取空ID的卡牌Prefab");
                return null;
            }

            if (cardPrefabDict.TryGetValue(cardId, out GameObject prefab))
            {
                return prefab;
            }

            Debug.LogWarning($"[CardPrefabRegistry] 未找到ID为 '{cardId}' 的卡牌Prefab");
            return null;
        }

        /// <summary>
        /// 检查是否包含指定的卡牌ID
        /// </summary>
        /// <param name="cardId">卡牌ID</param>
        /// <returns>是否包含</returns>
        public bool ContainsCardId(string cardId)
        {
            Initialize();
            return cardPrefabDict.ContainsKey(cardId);
        }

        /// <summary>
        /// 获取所有注册的卡牌ID
        /// </summary>
        /// <returns>卡牌ID列表</returns>
        public List<string> GetAllCardIds()
        {
            Initialize();
            return new List<string>(cardPrefabDict.Keys);
        }

        /// <summary>
        /// 在Inspector中验证时重新初始化
        /// </summary>
        private void OnValidate()
        {
            // 重置字典，下次访问时会重新初始化
            cardPrefabDict = null;
        }
    }
}

