using UnityEngine;
using UnityEngine.Events;

namespace CurrencySystem
{
    /// <summary>
    /// 金币系统管理器
    /// 提供金币的增减逻辑，配合 CoinData 使用
    /// 扣金币时配合检测金币是否足够的逻辑，如果不够返回给调用者处理
    /// </summary>
    public class CoinSystem : MonoBehaviour
    {
        [Header("金币数据")]
        [Tooltip("金币数据（持久化数据，必须设置）")]
        [SerializeField] private CoinData coinData;

        [Header("事件")]
        [Tooltip("金币数量变化时触发（当前金币数量）")]
        [SerializeField] private UnityEvent<int> onCoinsChanged = new UnityEvent<int>();

        [Tooltip("尝试花费金币失败时触发（需要的金币数量，当前金币数量）")]
        [SerializeField] private UnityEvent<int, int> onSpendFailed = new UnityEvent<int, int>();

        /// <summary>
        /// 金币数据
        /// </summary>
        public CoinData CoinData
        {
            get => coinData;
            set => coinData = value;
        }

        /// <summary>
        /// 当前金币数量（只读）
        /// </summary>
        public int CurrentCoins => coinData != null ? coinData.CurrentCoins : 0;

        /// <summary>
        /// 金币数量变化事件
        /// </summary>
        public UnityEvent<int> OnCoinsChanged => onCoinsChanged;

        /// <summary>
        /// 尝试花费金币失败事件
        /// </summary>
        public UnityEvent<int, int> OnSpendFailed => onSpendFailed;

        private void Awake()
        {
            if (coinData == null)
            {
                Debug.LogWarning("[CoinSystem] 金币数据未设置，请确保在 Inspector 中设置 CoinData");
            }
        }

        /// <summary>
        /// 检查金币是否足够
        /// </summary>
        /// <param name="amount">需要的金币数量</param>
        /// <returns>是否足够</returns>
        public bool HasEnoughCoins(int amount)
        {
            if (coinData == null)
            {
                Debug.LogError("[CoinSystem] 金币数据未设置，无法检查金币");
                return false;
            }

            return coinData.HasEnoughCoins(amount);
        }

        /// <summary>
        /// 增加金币
        /// </summary>
        /// <param name="amount">增加的金币数量</param>
        /// <returns>实际增加的金币数量</returns>
        public int AddCoins(int amount)
        {
            if (coinData == null)
            {
                Debug.LogError("[CoinSystem] 金币数据未设置，无法增加金币");
                return 0;
            }

            int actualAdded = coinData.AddCoins(amount);
            
            if (actualAdded > 0)
            {
                onCoinsChanged?.Invoke(coinData.CurrentCoins);
            }

            return actualAdded;
        }

        /// <summary>
        /// 减少金币（不检查是否足够，直接减少）
        /// 注意：此方法不会检查金币是否足够，如果金币不足会变成0（不会变成负数）
        /// 建议使用 TrySpendCoins 方法
        /// </summary>
        /// <param name="amount">减少的金币数量</param>
        /// <returns>实际减少的金币数量</returns>
        public int RemoveCoins(int amount)
        {
            if (coinData == null)
            {
                Debug.LogError("[CoinSystem] 金币数据未设置，无法减少金币");
                return 0;
            }

            int actualRemoved = coinData.RemoveCoins(amount);
            
            if (actualRemoved > 0)
            {
                onCoinsChanged?.Invoke(coinData.CurrentCoins);
            }

            return actualRemoved;
        }

        /// <summary>
        /// 尝试花费金币（检查是否足够）
        /// 如果金币足够，则扣除并返回 true；否则返回 false，不扣除金币，并触发失败事件
        /// </summary>
        /// <param name="amount">需要花费的金币数量</param>
        /// <returns>是否成功花费（金币足够返回 true，不足返回 false）</returns>
        public bool TrySpendCoins(int amount)
        {
            if (coinData == null)
            {
                Debug.LogError("[CoinSystem] 金币数据未设置，无法花费金币");
                return false;
            }

            bool success = coinData.TrySpendCoins(amount);
            
            if (success)
            {
                // 成功花费，触发金币变化事件
                onCoinsChanged?.Invoke(coinData.CurrentCoins);
            }
            else
            {
                // 失败，触发失败事件（让调用者处理不够之后的逻辑）
                onSpendFailed?.Invoke(amount, coinData.CurrentCoins);
            }

            return success;
        }

        /// <summary>
        /// 设置金币数量（用于初始化或重置）
        /// </summary>
        /// <param name="amount">新的金币数量</param>
        public void SetCoins(int amount)
        {
            if (coinData == null)
            {
                Debug.LogError("[CoinSystem] 金币数据未设置，无法设置金币");
                return;
            }

            coinData.SetCoins(amount);
            onCoinsChanged?.Invoke(coinData.CurrentCoins);
        }

        /// <summary>
        /// 重置金币（设置为初始值）
        /// </summary>
        /// <param name="initialAmount">初始金币数量（默认为0）</param>
        public void ResetCoins(int initialAmount = 0)
        {
            if (coinData == null)
            {
                Debug.LogError("[CoinSystem] 金币数据未设置，无法重置金币");
                return;
            }

            coinData.ResetCoins(initialAmount);
            onCoinsChanged?.Invoke(coinData.CurrentCoins);
        }
    }
}

