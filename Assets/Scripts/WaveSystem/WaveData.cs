using System.Collections.Generic;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 波数据 - 用于存储波的数据结构
    /// 包含波的方向和波峰数据（位置和强度值）
    /// </summary>
    [System.Serializable]
    public class WaveData
    {
        /// <summary>
        /// 波的攻击方向
        /// true表示攻向玩家，false表示不攻向玩家
        /// </summary>
        public bool AttackDirection;

        /// <summary>
        /// 波峰数据字典
        /// key为位置，value为强度值
        /// </summary>
        public Dictionary<int, int> PeakData = new Dictionary<int, int>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="attackDirection">攻击方向</param>
        public WaveData(bool attackDirection)
        {
            AttackDirection = attackDirection;
        }

        /// <summary>
        /// 添加波峰数据
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="value">强度值</param>
        public void AddPeak(int position, int value)
        {
            PeakData[position] = value;
        }

        /// <summary>
        /// 获取波峰数量
        /// </summary>
        public int PeakCount => PeakData.Count;

        /// <summary>
        /// 检查是否为空
        /// </summary>
        public bool IsEmpty => PeakData.Count == 0;
    }
}

