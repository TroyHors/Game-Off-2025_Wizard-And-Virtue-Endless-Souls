using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 波牌数据
    /// ScriptableObject，定义单个波牌类型的配置信息
    /// </summary>
    [CreateAssetMenu(fileName = "Wave Card Data", menuName = "Wave System/Wave Card Data")]
    public class WaveCardData : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("波牌ID（唯一标识符）")]
        [SerializeField] private string cardId;

        [Tooltip("波牌名称（用于UI显示）")]
        [SerializeField] private string cardName;

        [Header("波数据")]
        [Tooltip("波数据（定义波牌的波峰信息）")]
        [SerializeField] private WaveData waveData = new WaveData(true);

        [Header("经济")]
        [Tooltip("波牌价格（购买/使用该波牌所需的金币）")]
        [SerializeField] private int cardPrice = 0;

        [Header("波牌实体（可选）")]
        [Tooltip("波牌Prefab（如果使用Prefab系统，必须包含 WaveCardComponent 组件）")]
        [SerializeField] private GameObject cardPrefab;

        /// <summary>
        /// 波牌ID（唯一标识符）
        /// </summary>
        public string CardId => cardId;

        /// <summary>
        /// 波牌名称
        /// </summary>
        public string CardName => cardName;

        /// <summary>
        /// 波数据
        /// </summary>
        public WaveData WaveData => waveData;

        /// <summary>
        /// 波牌价格
        /// </summary>
        public int CardPrice => cardPrice;

        /// <summary>
        /// 波牌Prefab（可选）
        /// </summary>
        public GameObject CardPrefab => cardPrefab;

        /// <summary>
        /// 验证数据有效性
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogError($"[WaveCardData] {name} 的波牌ID为空");
                return false;
            }

            if (waveData == null)
            {
                Debug.LogError($"[WaveCardData] {name} 的波数据为null");
                return false;
            }

            // Prefab是可选的，但如果设置了，必须包含WaveCardComponent
            if (cardPrefab != null && cardPrefab.GetComponent<WaveCardComponent>() == null)
            {
                Debug.LogWarning($"[WaveCardData] {name} 的波牌Prefab缺少 WaveCardComponent 组件（Prefab是可选的，但建议添加组件）");
            }

            return true;
        }
    }
}

