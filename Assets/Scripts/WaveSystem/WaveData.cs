using System.Collections.Generic;
using System.Linq;
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
        /// true表示攻向敌人，false表示攻向玩家
        /// </summary>
        public bool AttackDirection;

        /// <summary>
        /// 可序列化的波峰数据列表（用于Inspector编辑）
        /// 在Inspector中可以直接添加和编辑波峰数据
        /// </summary>
        [SerializeField] 
        [Tooltip("波峰数据列表（位置和强度值）")]
        private List<SerializablePeakData> serializedPeakData = new List<SerializablePeakData>();

        /// <summary>
        /// 波峰数据字典（运行时使用）
        /// key为位置，value为强度值
        /// </summary>
        private Dictionary<int, int> peakDataCache;

        /// <summary>
        /// 波峰数据字典（只读）
        /// </summary>
        public Dictionary<int, int> PeakData
        {
            get
            {
                if (peakDataCache == null)
                {
                    RefreshCache();
                }
                return peakDataCache;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="attackDirection">攻击方向</param>
        public WaveData(bool attackDirection)
        {
            AttackDirection = attackDirection;
            RefreshCache();
        }

        /// <summary>
        /// 刷新缓存（从序列化列表更新字典）
        /// </summary>
        private void RefreshCache()
        {
            peakDataCache = new Dictionary<int, int>();
            if (serializedPeakData != null)
            {
                foreach (var peak in serializedPeakData)
                {
                    if (peak != null)
                    {
                        peakDataCache[peak.position] = peak.value;
                    }
                }
            }
        }

        /// <summary>
        /// 添加波峰数据
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="value">强度值</param>
        public void AddPeak(int position, int value)
        {
            // 检查是否已存在
            var existing = serializedPeakData.FirstOrDefault(p => p != null && p.position == position);
            if (existing != null)
            {
                existing.value = value;
            }
            else
            {
                serializedPeakData.Add(new SerializablePeakData(position, value));
            }
            RefreshCache();
        }

        /// <summary>
        /// 获取序列化的波峰数据列表（用于Inspector编辑）
        /// </summary>
        public List<SerializablePeakData> GetSerializedPeakData()
        {
            return serializedPeakData;
        }

        /// <summary>
        /// 设置序列化的波峰数据列表（用于Inspector编辑）
        /// </summary>
        public void SetSerializedPeakData(List<SerializablePeakData> data)
        {
            serializedPeakData = data ?? new List<SerializablePeakData>();
            RefreshCache();
        }

        /// <summary>
        /// 获取波峰数量
        /// </summary>
        public int PeakCount => PeakData.Count;

        /// <summary>
        /// 检查是否为空
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                // 总是刷新缓存以确保数据是最新的
                RefreshCache();
                return PeakData.Count == 0;
            }
        }
    }
}

