using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 波 - 由多个波峰组成的集合
    /// 波峰可以位于任意位置，波中可以有空位
    /// 重要约束：同一个波中的所有波峰必须具有相同的攻击方向
    /// </summary>
    [System.Serializable]
    public class Wave
    {
        /// <summary>
        /// 使用Dictionary存储波峰，key为位置，value为波峰
        /// 这样可以支持空位（不存在的key表示空位）
        /// </summary>
        private Dictionary<int, WavePeak> peaks = new Dictionary<int, WavePeak>();

        /// <summary>
        /// 波的攻击方向
        /// null表示空波（没有波峰），true表示攻向敌人，false表示攻向玩家
        /// 同一个波中的所有波峰必须具有相同的攻击方向
        /// </summary>
        public bool? AttackDirection { get; private set; } = null;

        /// <summary>
        /// 获取波中波峰的数量
        /// </summary>
        public int PeakCount => peaks.Count;

        /// <summary>
        /// 检查波是否为空（没有任何波峰）
        /// </summary>
        public bool IsEmpty => peaks.Count == 0;

        /// <summary>
        /// 获取所有波峰的位置（只读）
        /// </summary>
        public IReadOnlyCollection<int> Positions => peaks.Keys;

        /// <summary>
        /// 获取所有波峰（只读）
        /// </summary>
        public IReadOnlyCollection<WavePeak> Peaks => peaks.Values;

        /// <summary>
        /// 获取所有波峰的键值对（只读），用于遍历
        /// </summary>
        public IReadOnlyDictionary<int, WavePeak> PeakDictionary => peaks;

        /// <summary>
        /// 在指定位置添加波峰
        /// 如果该位置已存在波峰，则会被替换
        /// 如果波峰的攻击方向与波的方向不一致，会报错并拒绝添加
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="peak">要添加的波峰</param>
        public void AddPeak(int position, WavePeak peak)
        {
            if (peak == null)
            {
                Debug.LogWarning("[Wave] 尝试添加空的波峰");
                return;
            }

            // 检查方向一致性
            if (AttackDirection.HasValue)
            {
                // 波已有方向，检查是否一致
                if (AttackDirection.Value != peak.AttackDirection)
                {
                    Debug.LogError($"[Wave] 尝试添加不同方向的波峰！波的方向为{(AttackDirection.Value ? "攻向敌人" : "攻向玩家")}，但波峰的方向为{(peak.AttackDirection ? "攻向敌人" : "攻向玩家")}。位置{position}的波峰已被拒绝添加。");
                    return;
                }
            }
            else
            {
                // 波为空，设置方向
                AttackDirection = peak.AttackDirection;
            }

            peaks[position] = peak;
        }

        /// <summary>
        /// 在指定位置添加波峰
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="value">强度值</param>
        /// <param name="attackDirection">攻击方向（true=攻向敌人）</param>
        public void AddPeak(int position, int value, bool attackDirection)
        {
            AddPeak(position, new WavePeak(value, attackDirection));
        }

        /// <summary>
        /// 移除指定位置的波峰
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>是否成功移除</returns>
        public bool RemovePeak(int position)
        {
            return peaks.Remove(position);
        }

        /// <summary>
        /// 获取指定位置的波峰
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="peak">输出的波峰，如果不存在则为null</param>
        /// <returns>是否存在该位置的波峰</returns>
        public bool TryGetPeak(int position, out WavePeak peak)
        {
            return peaks.TryGetValue(position, out peak);
        }

        /// <summary>
        /// 检查指定位置是否存在波峰
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>是否存在波峰</returns>
        public bool HasPeakAt(int position)
        {
            return peaks.ContainsKey(position);
        }

        /// <summary>
        /// 获取指定位置的波峰
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>波峰，如果不存在则返回null</returns>
        public WavePeak GetPeak(int position)
        {
            peaks.TryGetValue(position, out WavePeak peak);
            return peak;
        }

        /// <summary>
        /// 获取波的最小位置
        /// </summary>
        /// <returns>最小位置，如果波为空则返回0</returns>
        public int GetMinPosition()
        {
            if (peaks.Count == 0) return 0;
            return peaks.Keys.Min();
        }

        /// <summary>
        /// 获取波的最大位置
        /// </summary>
        /// <returns>最大位置，如果波为空则返回0</returns>
        public int GetMaxPosition()
        {
            if (peaks.Count == 0) return 0;
            return peaks.Keys.Max();
        }

        /// <summary>
        /// 清空所有波峰
        /// </summary>
        public void Clear()
        {
            peaks.Clear();
            AttackDirection = null;
        }

        /// <summary>
        /// 创建波的副本
        /// </summary>
        /// <returns>新的波实例</returns>
        public Wave Clone()
        {
            Wave newWave = new Wave();
            newWave.AttackDirection = AttackDirection;
            foreach (var kvp in peaks)
            {
                newWave.peaks[kvp.Key] = kvp.Value.Clone();
            }
            return newWave;
        }

        /// <summary>
        /// 从波数据创建波
        /// </summary>
        /// <param name="waveData">波数据</param>
        /// <returns>新创建的波</returns>
        public static Wave FromData(WaveData waveData)
        {
            if (waveData == null)
            {
                Debug.LogWarning("[Wave] FromData: 传入的波数据为null");
                return new Wave();
            }

            Wave wave = new Wave();
            wave.AttackDirection = waveData.AttackDirection;

            foreach (var kvp in waveData.PeakData)
            {
                int position = kvp.Key;
                int value = kvp.Value;
                wave.AddPeak(position, new WavePeak(value, waveData.AttackDirection));
            }

            return wave;
        }

        /// <summary>
        /// 获取所有波峰的列表（按位置排序）
        /// 用于UI显示等需要有序列表的场景
        /// 返回包含位置信息的元组列表
        /// </summary>
        /// <returns>按位置排序的波峰列表（位置，波峰）</returns>
        public List<(int position, WavePeak peak)> GetSortedPeaks()
        {
            return peaks.OrderBy(kvp => kvp.Key)
                       .Select(kvp => (kvp.Key, kvp.Value))
                       .ToList();
        }

        /// <summary>
        /// 获取指定范围内的所有波峰（按位置排序）
        /// </summary>
        /// <param name="minPosition">最小位置（包含）</param>
        /// <param name="maxPosition">最大位置（包含）</param>
        /// <returns>指定范围内的波峰列表（位置，波峰）</returns>
        public List<(int position, WavePeak peak)> GetPeaksInRange(int minPosition, int maxPosition)
        {
            return peaks.Where(kvp => kvp.Key >= minPosition && kvp.Key <= maxPosition)
                       .OrderBy(kvp => kvp.Key)
                       .Select(kvp => (kvp.Key, kvp.Value))
                       .ToList();
        }

        /// <summary>
        /// 生成负波 - 生成一个当前波除了波峰强度符号相反以外其他一切相同的波
        /// </summary>
        /// <returns>新的负波实例</returns>
        public Wave GenerateNegativeWave()
        {
            Wave negativeWave = new Wave();
            negativeWave.AttackDirection = AttackDirection;

            foreach (var kvp in peaks)
            {
                int position = kvp.Key;
                WavePeak originalPeak = kvp.Value;
                // 强度取反，方向保持不变
                WavePeak negativePeak = new WavePeak(-originalPeak.Value, originalPeak.AttackDirection);
                negativeWave.peaks[position] = negativePeak;
            }

            return negativeWave;
        }

        /// <summary>
        /// 设置波的攻击方向
        /// 如果波中已有波峰，会同时更新所有波峰的方向
        /// </summary>
        /// <param name="attackDirection">新的攻击方向</param>
        /// <returns>是否成功设置</returns>
        public bool SetAttackDirection(bool attackDirection)
        {
            // 如果波为空，直接设置
            if (peaks.Count == 0)
            {
                AttackDirection = attackDirection;
                return true;
            }

            // 更新所有波峰的方向
            foreach (var peak in peaks.Values)
            {
                peak.AttackDirection = attackDirection;
            }

            AttackDirection = attackDirection;
            return true;
        }

        /// <summary>
        /// 设置指定位置波峰的强度值
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="value">新的强度值</param>
        /// <returns>是否成功设置（如果位置不存在则返回false）</returns>
        public bool SetPeakValue(int position, int value)
        {
            if (!peaks.TryGetValue(position, out WavePeak peak))
            {
                Debug.LogWarning($"[Wave] 尝试设置不存在位置的波峰强度: 位置{position}");
                return false;
            }

            peak.Value = value;
            return true;
        }

        /// <summary>
        /// 移动波峰位置
        /// 将指定位置的波峰移动到新位置
        /// </summary>
        /// <param name="oldPosition">原位置</param>
        /// <param name="newPosition">新位置</param>
        /// <returns>是否成功移动</returns>
        public bool MovePeak(int oldPosition, int newPosition)
        {
            if (!peaks.TryGetValue(oldPosition, out WavePeak peak))
            {
                Debug.LogWarning($"[Wave] 尝试移动不存在位置的波峰: 位置{oldPosition}");
                return false;
            }

            if (oldPosition == newPosition)
            {
                // 位置相同，无需移动
                return true;
            }

            // 如果新位置已有波峰，会被替换
            peaks[newPosition] = peak;
            peaks.Remove(oldPosition);
            return true;
        }

        /// <summary>
        /// 批量设置波峰强度
        /// </summary>
        /// <param name="peakValues">位置到强度值的字典</param>
        /// <returns>成功设置的数量</returns>
        public int SetPeakValues(Dictionary<int, int> peakValues)
        {
            if (peakValues == null)
            {
                Debug.LogWarning("[Wave] SetPeakValues: 传入的字典为null");
                return 0;
            }

            int successCount = 0;
            foreach (var kvp in peakValues)
            {
                if (SetPeakValue(kvp.Key, kvp.Value))
                {
                    successCount++;
                }
            }

            return successCount;
        }

        public override string ToString()
        {
            if (peaks.Count == 0)
            {
                return "Wave(Empty)";
            }

            var sortedPeaks = GetSortedPeaks();
            var peakStrings = sortedPeaks.Select(p => $"Pos:{p.position}, {p.peak}");
            return $"Wave(Count:{peaks.Count}, Peaks:[{string.Join(", ", peakStrings)}])";
        }
    }
}

