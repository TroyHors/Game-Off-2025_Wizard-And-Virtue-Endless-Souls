using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 波峰 - 波的最小单位
    /// 表示波在某个位置的一个数值点
    /// 注意：位置信息存储在Wave中，波峰只存储强度和方向
    /// </summary>
    [System.Serializable]
    public class WavePeak
    {
        /// <summary>
        /// 波峰的强度值（整数，可正可负）
        /// 正负只表示数值符号，不表示方向
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 攻击方向
        /// true表示攻向玩家，false表示不攻向玩家（或攻向其他方向）
        /// </summary>
        public bool AttackDirection { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value">强度值</param>
        /// <param name="attackDirection">攻击方向（true=攻向玩家）</param>
        public WavePeak(int value, bool attackDirection)
        {
            Value = value;
            AttackDirection = attackDirection;
        }

        /// <summary>
        /// 创建波峰的副本
        /// </summary>
        /// <returns>新的波峰实例</returns>
        public WavePeak Clone()
        {
            return new WavePeak(Value, AttackDirection);
        }

        public override string ToString()
        {
            return $"WavePeak(Value:{Value}, Dir:{(AttackDirection ? "Player" : "Other")})";
        }
    }
}

