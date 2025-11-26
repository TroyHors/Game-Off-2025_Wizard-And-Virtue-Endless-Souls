using UnityEngine;

namespace CurrencySystem
{
    /// <summary>
    /// 金币数据（持久化数据）
    /// 长期存在的金币数据，使用 ScriptableObject 持久化
    /// </summary>
    [CreateAssetMenu(fileName = "CoinData", menuName = "Currency System/Coin Data")]
    public class CoinData : ScriptableObject
    {
        [Header("金币设置")]
        [Tooltip("当前金币数量")]
        [SerializeField] private int currentCoins = 0;

        /// <summary>
        /// 当前金币数量（只读）
        /// </summary>
        public int CurrentCoins => currentCoins;

        /// <summary>
        /// 检查金币是否足够
        /// </summary>
        /// <param name="amount">需要的金币数量</param>
        /// <returns>是否足够</returns>
        public bool HasEnoughCoins(int amount)
        {
            return currentCoins >= amount;
        }

        /// <summary>
        /// 增加金币
        /// </summary>
        /// <param name="amount">增加的金币数量</param>
        /// <returns>实际增加的金币数量</returns>
        public int AddCoins(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CoinData] 增加的金币数量必须为正数，当前值：{amount}");
                return 0;
            }

            int oldCoins = currentCoins;
            currentCoins += amount;
            int actualAdded = currentCoins - oldCoins;

            Debug.Log($"[CoinData] 增加了 {actualAdded} 枚金币，当前金币：{currentCoins}");

            return actualAdded;
        }

        /// <summary>
        /// 减少金币（不检查是否足够，直接减少）
        /// 注意：此方法不会检查金币是否足够，如果金币不足会变成负数
        /// 建议使用 TrySpendCoins 方法
        /// </summary>
        /// <param name="amount">减少的金币数量</param>
        /// <returns>实际减少的金币数量</returns>
        public int RemoveCoins(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CoinData] 减少的金币数量必须为正数，当前值：{amount}");
                return 0;
            }

            int oldCoins = currentCoins;
            currentCoins = Mathf.Max(0, currentCoins - amount);
            int actualRemoved = oldCoins - currentCoins;

            if (actualRemoved > 0)
            {
                Debug.Log($"[CoinData] 减少了 {actualRemoved} 枚金币，当前金币：{currentCoins}");
            }

            return actualRemoved;
        }

        /// <summary>
        /// 尝试花费金币（检查是否足够）
        /// 如果金币足够，则扣除并返回 true；否则返回 false，不扣除金币
        /// </summary>
        /// <param name="amount">需要花费的金币数量</param>
        /// <returns>是否成功花费（金币足够返回 true，不足返回 false）</returns>
        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CoinData] 花费的金币数量必须为正数，当前值：{amount}");
                return false;
            }

            if (!HasEnoughCoins(amount))
            {
                Debug.Log($"[CoinData] 金币不足，需要 {amount} 枚，当前只有 {currentCoins} 枚");
                return false;
            }

            currentCoins -= amount;
            Debug.Log($"[CoinData] 花费了 {amount} 枚金币，当前金币：{currentCoins}");

            return true;
        }

        /// <summary>
        /// 设置金币数量（用于初始化或重置）
        /// </summary>
        /// <param name="amount">新的金币数量</param>
        public void SetCoins(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[CoinData] 金币数量不能为负数，当前值：{amount}，将设置为 0");
                amount = 0;
            }

            currentCoins = amount;
            Debug.Log($"[CoinData] 金币数量设置为 {currentCoins}");
        }

        /// <summary>
        /// 重置金币（设置为初始值）
        /// </summary>
        /// <param name="initialAmount">初始金币数量（默认为0）</param>
        public void ResetCoins(int initialAmount = 0)
        {
            SetCoins(initialAmount);
            Debug.Log($"[CoinData] 金币已重置为 {currentCoins}");
        }
    }
}

