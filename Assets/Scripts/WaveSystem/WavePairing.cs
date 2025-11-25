using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 波配对工具类
    /// 处理两个波之间的配对逻辑，生成新的波
    /// </summary>
    public static class WavePairing
    {
        /// <summary>
        /// 配对两个波
        /// 相同位置的波峰会进行配对计算，生成新的波
        /// </summary>
        /// <param name="waveA">第一个波</param>
        /// <param name="waveB">第二个波</param>
        /// <returns>配对后生成的新波列表（可能包含1个或2个波）</returns>
        public static List<Wave> PairWaves(Wave waveA, Wave waveB)
        {
            if (waveA == null || waveB == null)
            {
                Debug.LogWarning("[WavePairing] 尝试配对空的波");
                return new List<Wave>();
            }

            // 获取所有需要配对的位置（两个波中至少有一个存在波峰的位置）
            HashSet<int> allPositions = new HashSet<int>();
            foreach (var pos in waveA.Positions)
            {
                allPositions.Add(pos);
            }
            foreach (var pos in waveB.Positions)
            {
                allPositions.Add(pos);
            }

            // 用于存储不同方向的波峰
            Dictionary<int, WavePeak> peaksDirectionTrue = new Dictionary<int, WavePeak>();
            Dictionary<int, WavePeak> peaksDirectionFalse = new Dictionary<int, WavePeak>();

            // 遍历所有位置，进行配对计算
            foreach (int position in allPositions)
            {
                bool hasPeakA = waveA.TryGetPeak(position, out WavePeak peakA);
                bool hasPeakB = waveB.TryGetPeak(position, out WavePeak peakB);

                WavePeak resultPeak = null;

                if (hasPeakA && hasPeakB)
                {
                    // 两个波都有波峰，进行配对
                    resultPeak = PairPeaks(peakA, peakB);
                }
                else if (hasPeakA)
                {
                    // 只有波A有波峰，直接使用
                    resultPeak = peakA.Clone();
                }
                else if (hasPeakB)
                {
                    // 只有波B有波峰，直接使用
                    resultPeak = peakB.Clone();
                }

                // 将结果波峰根据攻击方向分类存储（包括强度为0的波峰）
                if (resultPeak != null)
                {
                    if (resultPeak.AttackDirection)
                    {
                        peaksDirectionTrue[position] = resultPeak;
                    }
                    else
                    {
                        peaksDirectionFalse[position] = resultPeak;
                    }
                }
            }

            // 根据攻击方向生成新波
            List<Wave> resultWaves = new List<Wave>();

            if (peaksDirectionTrue.Count > 0)
            {
                Wave waveTrue = new Wave();
                foreach (var kvp in peaksDirectionTrue)
                {
                    waveTrue.AddPeak(kvp.Key, kvp.Value);
                }
                resultWaves.Add(waveTrue);
            }

            if (peaksDirectionFalse.Count > 0)
            {
                Wave waveFalse = new Wave();
                foreach (var kvp in peaksDirectionFalse)
                {
                    waveFalse.AddPeak(kvp.Key, kvp.Value);
                }
                resultWaves.Add(waveFalse);
            }

            return resultWaves;
        }

        /// <summary>
        /// 配对两个波峰
        /// </summary>
        /// <param name="peakA">第一个波峰</param>
        /// <param name="peakB">第二个波峰</param>
        /// <returns>配对后的新波峰</returns>
        private static WavePeak PairPeaks(WavePeak peakA, WavePeak peakB)
        {
            if (peakA == null || peakB == null)
            {
                Debug.LogWarning("[WavePairing] 尝试配对空的波峰");
                return null;
            }

            // 强度相加
            int resultValue = peakA.Value + peakB.Value;

            // 确定攻击方向
            bool resultDirection;
            if (peakA.AttackDirection == peakB.AttackDirection)
            {
                // 方向相同，直接继承
                resultDirection = peakA.AttackDirection;
            }
            else
            {
                // 方向相反，继承绝对值更大的波峰的方向
                int absA = System.Math.Abs(peakA.Value);
                int absB = System.Math.Abs(peakB.Value);

                if (absA > absB)
                {
                    resultDirection = peakA.AttackDirection;
                }
                else if (absB > absA)
                {
                    resultDirection = peakB.AttackDirection;
                }
                else
                {
                    // 绝对值相等，默认使用peakA的方向
                    resultDirection = peakA.AttackDirection;
                }
            }

            return new WavePeak(resultValue, resultDirection);
        }
    }
}

