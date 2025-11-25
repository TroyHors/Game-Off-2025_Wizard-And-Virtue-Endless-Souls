using System.Collections.Generic;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 结果波计算器
    /// 独立的结果波计算逻辑，不修改原始波，只读取信息并计算
    /// </summary>
    public static class WaveResultCalculator
    {
        /// <summary>
        /// 计算两个波配对后的结果波
        /// 永远返回两个结果波：一个攻向玩家，一个攻向敌人
        /// </summary>
        /// <param name="playerWave">玩家发出的波（手牌波）</param>
        /// <param name="enemyWave">敌人波</param>
        /// <param name="minPosition">最小位置（用于初始化结果波的大小）</param>
        /// <param name="maxPosition">最大位置（用于初始化结果波的大小）</param>
        /// <returns>包含两个结果波的列表：[攻向敌人的波, 攻向玩家的波]</returns>
        public static List<Wave> CalculateResultWaves(Wave playerWave, Wave enemyWave, int minPosition, int maxPosition)
        {
            // 创建两个结果波，初始全部填充强度为0的波
            Wave waveToEnemy = CreateInitializedWave(true, minPosition, maxPosition);  // 攻向敌人
            Wave waveToPlayer = CreateInitializedWave(false, minPosition, maxPosition); // 攻向玩家

            if (playerWave == null || enemyWave == null)
            {
                Debug.LogWarning("[WaveResultCalculator] 玩家波或敌人波为null，返回空结果波");
                return new List<Wave> { waveToEnemy, waveToPlayer };
            }

            // 获取所有需要配对的位置（两个波中至少有一个存在波峰的位置）
            HashSet<int> allPositions = new HashSet<int>();
            foreach (var pos in playerWave.Positions)
            {
                allPositions.Add(pos);
            }
            foreach (var pos in enemyWave.Positions)
            {
                allPositions.Add(pos);
            }

            // 遍历所有位置，进行配对计算
            foreach (int position in allPositions)
            {
                bool hasPlayerPeak = playerWave.TryGetPeak(position, out WavePeak playerPeak);
                bool hasEnemyPeak = enemyWave.TryGetPeak(position, out WavePeak enemyPeak);

                if (hasPlayerPeak && hasEnemyPeak)
                {
                    // 两个波都有波峰，进行配对
                    ProcessPairedPeaks(playerPeak, enemyPeak, position, waveToEnemy, waveToPlayer);
                }
                else if (hasPlayerPeak)
                {
                    // 只有玩家波有波峰，直接放入对应方向的结果波
                    AddPeakToResultWave(playerPeak, position, waveToEnemy, waveToPlayer);
                }
                else if (hasEnemyPeak)
                {
                    // 只有敌人波有波峰，直接放入对应方向的结果波
                    AddPeakToResultWave(enemyPeak, position, waveToEnemy, waveToPlayer);
                }
            }

            return new List<Wave> { waveToEnemy, waveToPlayer };
        }

        /// <summary>
        /// 创建初始化的波（所有位置填充强度为0的波峰）
        /// </summary>
        /// <param name="attackDirection">攻击方向（true=攻向敌人，false=攻向玩家）</param>
        /// <param name="minPosition">最小位置</param>
        /// <param name="maxPosition">最大位置</param>
        /// <returns>初始化的波</returns>
        private static Wave CreateInitializedWave(bool attackDirection, int minPosition, int maxPosition)
        {
            Wave wave = new Wave();
            wave.SetAttackDirection(attackDirection);

            // 在所有位置填充强度为0的波峰
            for (int position = minPosition; position <= maxPosition; position++)
            {
                wave.AddPeak(position, 0, attackDirection);
            }

            return wave;
        }

        /// <summary>
        /// 处理配对的两个波峰
        /// </summary>
        /// <param name="playerPeak">玩家波峰</param>
        /// <param name="enemyPeak">敌人波峰</param>
        /// <param name="position">波峰位置</param>
        /// <param name="waveToEnemy">攻向敌人的结果波</param>
        /// <param name="waveToPlayer">攻向玩家的结果波</param>
        private static void ProcessPairedPeaks(WavePeak playerPeak, WavePeak enemyPeak, int position, 
            Wave waveToEnemy, Wave waveToPlayer)
        {
            int playerValue = playerPeak.Value;
            int enemyValue = enemyPeak.Value;

            // 判断强度是否同号
            bool sameSign = (playerValue >= 0 && enemyValue >= 0) || (playerValue < 0 && enemyValue < 0);

            if (sameSign)
            {
                // 强度同号：放入绝对值更大的方向的结果波，强度为相减绝对值
                int absPlayer = Mathf.Abs(playerValue);
                int absEnemy = Mathf.Abs(enemyValue);

                if (absPlayer > absEnemy)
                {
                    // 玩家波绝对值更大，放入玩家波的方向
                    int resultValue = absPlayer - absEnemy;
                    bool resultDirection = playerPeak.AttackDirection;
                    waveToEnemy.SetPeakValue(position, resultDirection ? resultValue : 0);
                    waveToPlayer.SetPeakValue(position, resultDirection ? 0 : resultValue);
                }
                else if (absEnemy > absPlayer)
                {
                    // 敌人波绝对值更大，放入敌人波的方向
                    int resultValue = absEnemy - absPlayer;
                    bool resultDirection = enemyPeak.AttackDirection;
                    waveToEnemy.SetPeakValue(position, resultDirection ? resultValue : 0);
                    waveToPlayer.SetPeakValue(position, resultDirection ? 0 : resultValue);
                }
                else
                {
                    // 绝对值相等，结果为0（已经在初始化时设置为0，无需修改）
                }
            }
            else
            {
                // 强度异号：同时放入两个结果波，强度为绝对值相加
                int absPlayer = Mathf.Abs(playerValue);
                int absEnemy = Mathf.Abs(enemyValue);
                int resultValue = absPlayer + absEnemy;

                // 玩家波的方向
                bool playerDirection = playerPeak.AttackDirection;
                // 敌人波的方向
                bool enemyDirection = enemyPeak.AttackDirection;

                // 同时放入两个结果波（每个波根据其方向放入对应的结果波）
                // 玩家波的方向
                if (playerDirection)
                {
                    // 玩家波攻向敌人，结果值放入攻向敌人的波
                    waveToEnemy.SetPeakValue(position, resultValue);
                }
                else
                {
                    // 玩家波攻向玩家，结果值放入攻向玩家的波
                    waveToPlayer.SetPeakValue(position, resultValue);
                }

                // 敌人波的方向
                if (enemyDirection)
                {
                    // 敌人波攻向敌人，结果值放入攻向敌人的波（如果已经放入，则累加）
                    int currentEnemyValue = waveToEnemy.GetPeak(position).Value;
                    waveToEnemy.SetPeakValue(position, currentEnemyValue + resultValue);
                }
                else
                {
                    // 敌人波攻向玩家，结果值放入攻向玩家的波（如果已经放入，则累加）
                    int currentPlayerValue = waveToPlayer.GetPeak(position).Value;
                    waveToPlayer.SetPeakValue(position, currentPlayerValue + resultValue);
                }
            }
        }

        /// <summary>
        /// 将单个波峰添加到结果波中
        /// </summary>
        /// <param name="peak">要添加的波峰</param>
        /// <param name="position">波峰位置</param>
        /// <param name="waveToEnemy">攻向敌人的结果波</param>
        /// <param name="waveToPlayer">攻向玩家的结果波</param>
        private static void AddPeakToResultWave(WavePeak peak, int position, Wave waveToEnemy, Wave waveToPlayer)
        {
            int absValue = Mathf.Abs(peak.Value);
            bool direction = peak.AttackDirection;

            if (direction)
            {
                // 攻向敌人
                waveToEnemy.SetPeakValue(position, absValue);
                waveToPlayer.SetPeakValue(position, 0);
            }
            else
            {
                // 攻向玩家
                waveToEnemy.SetPeakValue(position, 0);
                waveToPlayer.SetPeakValue(position, absValue);
            }
        }
    }
}

