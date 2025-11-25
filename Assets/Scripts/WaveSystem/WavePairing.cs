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
        /// 配对规则：相同位置波峰强度相减（waveA - waveB）
        /// 结果波始终生成两个：一个是攻向敌人的波，一个是攻向玩家的波
        /// </summary>
        /// <param name="waveA">第一个波（通常是玩家波）</param>
        /// <param name="waveB">第二个波（通常是敌人波）</param>
        /// <returns>配对后生成的新波列表（始终包含2个波，可能为空波）</returns>
        public static List<Wave> PairWaves(Wave waveA, Wave waveB)
        {
            if (waveA == null || waveB == null)
            {
                Debug.LogWarning("[WavePairing] 尝试配对空的波");
                // 返回两个空波
                return new List<Wave> { new Wave(), new Wave() };
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
            Dictionary<int, WavePeak> peaksDirectionTrue = new Dictionary<int, WavePeak>();  // 攻向敌人
            Dictionary<int, WavePeak> peaksDirectionFalse = new Dictionary<int, WavePeak>(); // 攻向玩家

            // 遍历所有位置，进行配对计算
            foreach (int position in allPositions)
            {
                bool hasPeakA = waveA.TryGetPeak(position, out WavePeak peakA);
                bool hasPeakB = waveB.TryGetPeak(position, out WavePeak peakB);

                WavePeak resultPeak = null;

                if (hasPeakA && hasPeakB)
                {
                    // 两个波都有波峰，进行配对（强度相减：waveA - waveB）
                    resultPeak = PairPeaks(peakA, peakB);
                }
                else if (hasPeakA)
                {
                    // 只有波A有波峰，直接使用
                    resultPeak = peakA.Clone();
                }
                else if (hasPeakB)
                {
                    // 只有波B有波峰，需要反转方向（因为是从waveA的角度配对）
                    // 如果waveB的波峰攻向敌人，那么从waveA的角度看，应该是攻向玩家（相反方向）
                    resultPeak = new WavePeak(-peakB.Value, !peakB.AttackDirection);
                }

                // 将结果波峰根据攻击方向分类存储（跳过强度为0的波峰）
                if (resultPeak != null && resultPeak.Value != 0)
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

            // 始终生成两个波（即使为空波）
            List<Wave> resultWaves = new List<Wave>();

            // 第一个波：攻向敌人的波
            Wave waveTrue = new Wave();
            foreach (var kvp in peaksDirectionTrue)
            {
                waveTrue.AddPeak(kvp.Key, kvp.Value);
            }
            resultWaves.Add(waveTrue);

            // 第二个波：攻向玩家的波
            Wave waveFalse = new Wave();
            foreach (var kvp in peaksDirectionFalse)
            {
                waveFalse.AddPeak(kvp.Key, kvp.Value);
            }
            resultWaves.Add(waveFalse);

            return resultWaves;
        }

        /// <summary>
        /// 配对两个波峰
        /// 配对规则：强度相减（peakA - peakB）
        /// 方向确定：
        /// - 如果结果值>0，方向继承peakA的方向
        /// - 如果结果值<0，方向与peakB相反（因为是从peakA的角度配对，peakB更大时应该反向）
        /// - 如果结果值=0，不生成波峰（在调用处会跳过）
        /// </summary>
        /// <param name="peakA">第一个波峰（通常是玩家波峰）</param>
        /// <param name="peakB">第二个波峰（通常是敌人波峰）</param>
        /// <returns>配对后的新波峰</returns>
        private static WavePeak PairPeaks(WavePeak peakA, WavePeak peakB)
        {
            if (peakA == null || peakB == null)
            {
                Debug.LogWarning("[WavePairing] 尝试配对空的波峰");
                return null;
            }

            // 强度相减（peakA - peakB）
            int resultValue = peakA.Value - peakB.Value;

            // 确定攻击方向
            bool resultDirection;
            if (resultValue == 0)
            {
                // 结果值为0，不生成波峰（在调用处会跳过）
                // 这里返回一个方向，但调用处会检查Value是否为0
                resultDirection = peakA.AttackDirection;
            }
            else if (resultValue > 0)
            {
                // 结果值>0，说明peakA更大，方向继承peakA
                resultDirection = peakA.AttackDirection;
            }
            else
            {
                // 结果值<0，说明peakB更大，方向与peakB相反（因为是从peakA的角度配对）
                // 例如：peakA攻向敌人(5)，peakB攻向玩家(8)，结果=-3，应该攻向玩家（与peakB相反）
                resultDirection = !peakB.AttackDirection;
            }

            return new WavePeak(resultValue, resultDirection);
        }
    }
}

