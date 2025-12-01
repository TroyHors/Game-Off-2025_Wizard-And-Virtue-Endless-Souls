using UnityEngine;
using TMPro;
using CurrencySystem;

namespace UI
{
    /// <summary>
    /// 金币数量显示组件
    /// 读取金币数据并显示在TextMeshPro组件中
    /// 通过挂载方式引用CoinSystem和TextMeshProUGUI
    /// </summary>
    public class CoinDisplay : MonoBehaviour
    {
        [Header("系统引用")]
        [Tooltip("金币系统（用于读取金币数据，必须设置）")]
        [SerializeField] private CoinSystem coinSystem;

        [Header("UI组件")]
        [Tooltip("金币数量文本（TextMeshProUGUI组件，用于显示金币数量，必须设置）")]
        [SerializeField] private TextMeshProUGUI coinText;

        private void Awake()
        {
            // 如果没有设置，尝试自动查找
            if (coinSystem == null)
            {
                coinSystem = FindObjectOfType<CoinSystem>();
            }

            if (coinText == null)
            {
                coinText = GetComponentInChildren<TextMeshProUGUI>();
            }

            // 验证引用
            if (coinSystem == null)
            {
                Debug.LogError("[CoinDisplay] CoinSystem未设置，无法显示金币数量");
            }

            if (coinText == null)
            {
                Debug.LogError("[CoinDisplay] TextMeshProUGUI未设置，无法显示金币数量");
            }
        }

        private void Start()
        {
            // 在Start时订阅事件，确保CoinSystem已完全初始化
            SubscribeToCoinChanges();
            
            // 初始化显示当前金币数量
            UpdateCoinDisplay();
        }

        private void OnEnable()
        {
            // 确保事件已订阅（如果Start还没执行）
            SubscribeToCoinChanges();
            
            // 更新显示（确保显示最新数据）
            UpdateCoinDisplay();
        }

        private void OnDisable()
        {
            // 取消订阅金币变化事件
            UnsubscribeFromCoinChanges();
        }

        /// <summary>
        /// 订阅金币变化事件
        /// </summary>
        private void SubscribeToCoinChanges()
        {
            if (coinSystem != null)
            {
                // 先移除监听器，避免重复订阅
                coinSystem.OnCoinsChanged.RemoveListener(OnCoinsChanged);
                // 添加监听器
                coinSystem.OnCoinsChanged.AddListener(OnCoinsChanged);
            }
        }

        /// <summary>
        /// 取消订阅金币变化事件
        /// </summary>
        private void UnsubscribeFromCoinChanges()
        {
            if (coinSystem != null)
            {
                coinSystem.OnCoinsChanged.RemoveListener(OnCoinsChanged);
            }
        }

        /// <summary>
        /// 金币数量变化事件处理
        /// </summary>
        /// <param name="newCoinAmount">新的金币数量</param>
        private void OnCoinsChanged(int newCoinAmount)
        {
            UpdateCoinDisplay();
        }

        /// <summary>
        /// 更新金币显示
        /// </summary>
        private void UpdateCoinDisplay()
        {
            if (coinText == null)
            {
                return;
            }

            if (coinSystem == null)
            {
                coinText.text = "0";
                return;
            }

            int currentCoins = coinSystem.CurrentCoins;
            coinText.text = currentCoins.ToString();
        }
    }
}

