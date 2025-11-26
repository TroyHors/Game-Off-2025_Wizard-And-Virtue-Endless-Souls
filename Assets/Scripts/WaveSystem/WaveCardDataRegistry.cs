using System.Collections.Generic;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 波牌数据注册表
    /// 管理所有波牌数据，类似成员数据注册表
    /// </summary>
    [CreateAssetMenu(fileName = "Wave Card Data Registry", menuName = "Wave System/Wave Card Data Registry")]
    public class WaveCardDataRegistry : ScriptableObject
    {
        [System.Serializable]
        public class WaveCardDataEntry
        {
            [Tooltip("波牌数据")]
            public WaveCardData waveCardData;
        }

        [Header("波牌数据列表")]
        [Tooltip("所有波牌数据")]
        [SerializeField] private List<WaveCardDataEntry> cardEntries = new List<WaveCardDataEntry>();

        /// <summary>
        /// 波牌数据字典（运行时使用）
        /// </summary>
        private Dictionary<string, WaveCardData> cardDataDict;

        /// <summary>
        /// 初始化注册表（构建字典）
        /// </summary>
        private void Initialize()
        {
            if (cardDataDict != null)
            {
                return; // 已经初始化
            }

            cardDataDict = new Dictionary<string, WaveCardData>();
            foreach (var entry in cardEntries)
            {
                if (entry != null && entry.waveCardData != null)
                {
                    string cardId = entry.waveCardData.CardId;
                    if (string.IsNullOrEmpty(cardId))
                    {
                        Debug.LogWarning($"[WaveCardDataRegistry] 波牌数据 {entry.waveCardData.name} 的波牌ID为空，跳过");
                        continue;
                    }

                    if (cardDataDict.ContainsKey(cardId))
                    {
                        Debug.LogWarning($"[WaveCardDataRegistry] 波牌ID '{cardId}' 重复，将使用最后一个数据");
                    }
                    cardDataDict[cardId] = entry.waveCardData;
                }
            }

            Debug.Log($"[WaveCardDataRegistry] 初始化完成，注册了 {cardDataDict.Count} 个波牌数据");
        }

        /// <summary>
        /// 根据波牌ID获取波牌数据
        /// </summary>
        /// <param name="cardId">波牌ID</param>
        /// <returns>波牌数据，如果不存在则返回null</returns>
        public WaveCardData GetWaveCardData(string cardId)
        {
            Initialize();

            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogWarning("[WaveCardDataRegistry] 波牌ID为空");
                return null;
            }

            if (cardDataDict.TryGetValue(cardId, out WaveCardData cardData))
            {
                return cardData;
            }

            Debug.LogWarning($"[WaveCardDataRegistry] 未找到波牌ID '{cardId}' 对应的波牌数据");
            return null;
        }

        /// <summary>
        /// 根据索引获取波牌数据
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>波牌数据，如果索引无效则返回null</returns>
        public WaveCardData GetWaveCardDataByIndex(int index)
        {
            Initialize();

            if (index < 0 || index >= cardEntries.Count)
            {
                Debug.LogWarning($"[WaveCardDataRegistry] 索引 {index} 无效，注册表中共有 {cardEntries.Count} 个波牌数据");
                return null;
            }

            var entry = cardEntries[index];
            if (entry == null || entry.waveCardData == null)
            {
                Debug.LogWarning($"[WaveCardDataRegistry] 索引 {index} 处的波牌数据为空");
                return null;
            }

            return entry.waveCardData;
        }

        /// <summary>
        /// 获取所有波牌数据
        /// </summary>
        /// <returns>所有波牌数据的列表</returns>
        public List<WaveCardData> GetAllWaveCardData()
        {
            Initialize();
            return new List<WaveCardData>(cardDataDict.Values);
        }

        /// <summary>
        /// 检查波牌ID是否存在
        /// </summary>
        /// <param name="cardId">波牌ID</param>
        /// <returns>是否存在</returns>
        public bool HasWaveCardData(string cardId)
        {
            Initialize();
            return cardDataDict.ContainsKey(cardId);
        }

        /// <summary>
        /// 获取波牌数据数量
        /// </summary>
        public int CardCount => cardEntries.Count;

        /// <summary>
        /// 在Inspector中验证时重新初始化
        /// </summary>
        private void OnValidate()
        {
            // 重置字典，下次访问时会重新初始化
            cardDataDict = null;
        }
    }
}

