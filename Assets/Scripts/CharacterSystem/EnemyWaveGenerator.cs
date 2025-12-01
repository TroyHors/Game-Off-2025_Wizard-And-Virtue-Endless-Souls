using System.Collections.Generic;
using UnityEngine;
using WaveSystem;
using DamageSystem;

namespace CharacterSystem
{
    /// <summary>
    /// 敌人波生成器
    /// 独立处理敌人随机波生成逻辑，不依赖战斗流程
    /// </summary>
    public static class EnemyWaveGenerator
    {
        /// <summary>
        /// 为所有敌人生成随机波（根据每个敌人的配置）
        /// </summary>
        /// <param name="enemies">敌人列表（HealthComponent）</param>
        /// <param name="enemyConfig">敌人配置（用于获取每个敌人的波生成配置）</param>
        /// <returns>成功生成波的敌人数量</returns>
        public static int GenerateRandomWavesForAllEnemies(List<HealthComponent> enemies, EnemyConfig enemyConfig)
        {
            Debug.Log($"[EnemyWaveGenerator] 开始为所有敌人生成随机波，敌人数量: {enemies?.Count ?? 0}");
            
            if (enemies == null || enemies.Count == 0)
            {
                Debug.LogWarning("[EnemyWaveGenerator] 敌人列表为空，无法生成随机波");
                return 0;
            }

            if (enemyConfig == null)
            {
                Debug.LogWarning("[EnemyWaveGenerator] 敌人配置未设置，无法生成随机波");
                return 0;
            }

            Debug.Log($"[EnemyWaveGenerator] 敌人配置包含 {enemyConfig.ConfigCount} 个配置数据");

            int successCount = 0;
            int skippedNoWaveManager = 0;
            int skippedNoConfig = 0;
            int skippedNoWaveGenConfig = 0;

            // 遍历所有敌人
            foreach (var enemy in enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                GameObject enemyObject = enemy.gameObject;
                Debug.Log($"[EnemyWaveGenerator] 处理敌人: {enemyObject.name}");
                
                EnemyWaveManager waveManager = enemyObject.GetComponent<EnemyWaveManager>();
                if (waveManager == null)
                {
                    Debug.LogWarning($"[EnemyWaveGenerator] 敌人 {enemyObject.name} 没有 EnemyWaveManager 组件，跳过");
                    skippedNoWaveManager++;
                    continue;
                }

                // 获取敌人对应的配置数据（通过敌人名称匹配）
                // 注意：这里假设敌人名称格式为 "EnemyName_Index"
                string enemyName = enemyObject.name;
                EnemyConfigData configData = null;

                Debug.Log($"[EnemyWaveGenerator] 尝试匹配敌人名称: {enemyName}");

                // 尝试从EnemyConfig中找到匹配的配置
                for (int i = 0; i < enemyConfig.ConfigCount; i++)
                {
                    var config = enemyConfig.GetEnemyConfig(i);
                    if (config != null)
                    {
                        Debug.Log($"[EnemyWaveGenerator] 检查配置[{i}]: enemyName={config.enemyName}, waveGenerationConfig={(config.waveGenerationConfig != null ? "存在" : "null")}");
                        if (enemyName.Contains(config.enemyName))
                        {
                            configData = config;
                            Debug.Log($"[EnemyWaveGenerator] 找到匹配的配置数据: 索引{i}, 名称={config.enemyName}");
                            break;
                        }
                    }
                }

                if (configData == null)
                {
                    Debug.LogWarning($"[EnemyWaveGenerator] 无法找到敌人 {enemyName} 对应的配置数据，跳过生成随机波");
                    skippedNoConfig++;
                    continue;
                }

                // 如果有波生成配置，生成随机波
                if (configData.waveGenerationConfig != null)
                {
                    Debug.Log($"[EnemyWaveGenerator] 开始为敌人 {enemyName} 生成随机波，使用配置: {configData.waveGenerationConfig.nodeType}");
                    WaveData randomWaveData = GenerateRandomWaveData(configData.waveGenerationConfig);
                    
                    Debug.Log($"[EnemyWaveGenerator] 生成的波数据: 波峰数={randomWaveData.PeakCount}, 是否为空={randomWaveData.IsEmpty}");
                    
                    // 更新配置数据中的波数据（存储在当前敌人实例的config里）
                    configData.waveData = randomWaveData;
                    
                    // 设置到EnemyWaveManager
                    waveManager.SetEnemyWaveData(randomWaveData);
                    
                    Debug.Log($"[EnemyWaveGenerator] 为敌人 {enemyName} 生成了随机波，波峰总数: {randomWaveData.PeakCount}");
                    successCount++;
                }
                else
                {
                    Debug.LogWarning($"[EnemyWaveGenerator] 敌人 {enemyName} 的配置数据没有波生成配置（waveGenerationConfig为null），使用现有波数据");
                    skippedNoWaveGenConfig++;
                }
            }

            Debug.Log($"[EnemyWaveGenerator] 波生成完成: 成功={successCount}, 跳过(无WaveManager)={skippedNoWaveManager}, 跳过(无配置)={skippedNoConfig}, 跳过(无波生成配置)={skippedNoWaveGenConfig}");
            return successCount;
        }

        /// <summary>
        /// 生成随机波数据（根据配置的方法和参数）
        /// </summary>
        /// <param name="typeConfig">敌人类型生成配置</param>
        /// <returns>生成的波数据</returns>
        public static WaveData GenerateRandomWaveData(EnemyTypeGenerationConfig typeConfig)
        {
            if (typeConfig == null)
            {
                Debug.LogError("[EnemyWaveGenerator] 敌人类型生成配置为空，无法生成随机波");
                return new WaveData(false);
            }

            WaveData waveData = new WaveData(false); // 敌人波方向为false（攻向玩家）

            // 随机生成波峰总值
            int totalPeakValue = Random.Range(typeConfig.totalPeakValueRange.x, typeConfig.totalPeakValueRange.y + 1);

            // 根据生成方法生成波峰
            switch (typeConfig.waveGenerationMethod)
            {
                case WaveGenerationMethod.Random:
                    GenerateRandomWavePeaks(waveData, totalPeakValue, typeConfig.maxSinglePeakValue, typeConfig.peakPositionRange);
                    break;

                case WaveGenerationMethod.Uniform:
                    GenerateUniformWavePeaks(waveData, totalPeakValue, typeConfig.maxSinglePeakValue, typeConfig.peakPositionRange);
                    break;

                case WaveGenerationMethod.Concentrated:
                    GenerateConcentratedWavePeaks(waveData, totalPeakValue, typeConfig.maxSinglePeakValue, typeConfig.peakPositionRange);
                    break;
            }

            Debug.Log($"[EnemyWaveGenerator] 生成的波数据: 波峰数={waveData.PeakCount}, 是否为空={waveData.IsEmpty}");
            return waveData;
        }

        /// <summary>
        /// 随机生成波峰（随机位置和强度）
        /// </summary>
        private static void GenerateRandomWavePeaks(WaveData waveData, int totalValue, int maxSinglePeak, Vector2Int positionRange)
        {
            int remainingValue = totalValue;
            HashSet<int> usedPositions = new HashSet<int>();

            while (remainingValue > 0)
            {
                // 随机选择位置
                int position = Random.Range(positionRange.x, positionRange.y + 1);
                
                // 如果位置已被使用，跳过（避免重复位置）
                if (usedPositions.Contains(position))
                {
                    continue;
                }

                // 随机生成波峰强度（不超过剩余值和单个波峰最大值）
                int peakValue = Random.Range(1, Mathf.Min(remainingValue, maxSinglePeak) + 1);
                
                waveData.AddPeak(position, peakValue);
                usedPositions.Add(position);
                remainingValue -= peakValue;
            }
        }

        /// <summary>
        /// 均匀生成波峰（均匀分布位置和强度）
        /// </summary>
        private static void GenerateUniformWavePeaks(WaveData waveData, int totalValue, int maxSinglePeak, Vector2Int positionRange)
        {
            int rangeSize = positionRange.y - positionRange.x + 1;
            if (rangeSize <= 0)
            {
                Debug.LogWarning("[EnemyWaveGenerator] 位置范围无效，无法生成均匀波峰");
                return;
            }

            // 计算每个波峰的平均值
            int averagePeakValue = Mathf.Min(totalValue / rangeSize, maxSinglePeak);
            if (averagePeakValue <= 0)
            {
                averagePeakValue = 1;
            }

            int remainingValue = totalValue;
            for (int pos = positionRange.x; pos <= positionRange.y && remainingValue > 0; pos++)
            {
                int peakValue = Mathf.Min(averagePeakValue, remainingValue);
                if (peakValue > 0)
                {
                    waveData.AddPeak(pos, peakValue);
                    remainingValue -= peakValue;
                }
            }

            // 如果还有剩余值，随机分配到已有波峰
            if (remainingValue > 0)
            {
                var peaks = waveData.GetSerializedPeakData();
                while (remainingValue > 0 && peaks.Count > 0)
                {
                    int randomIndex = Random.Range(0, peaks.Count);
                    var peak = peaks[randomIndex];
                    int addValue = Mathf.Min(remainingValue, maxSinglePeak - peak.value);
                    if (addValue > 0)
                    {
                        peak.value += addValue;
                        remainingValue -= addValue;
                    }
                    else
                    {
                        break; // 无法再分配
                    }
                }
            }
        }

        /// <summary>
        /// 集中生成波峰（集中在少数位置）
        /// </summary>
        private static void GenerateConcentratedWavePeaks(WaveData waveData, int totalValue, int maxSinglePeak, Vector2Int positionRange)
        {
            // 选择1-3个集中位置
            int concentrationCount = Random.Range(1, 4);
            List<int> concentrationPositions = new List<int>();
            
            for (int i = 0; i < concentrationCount; i++)
            {
                int position = Random.Range(positionRange.x, positionRange.y + 1);
                if (!concentrationPositions.Contains(position))
                {
                    concentrationPositions.Add(position);
                }
            }

            // 将总值分配到集中位置
            int remainingValue = totalValue;
            for (int i = 0; i < concentrationPositions.Count && remainingValue > 0; i++)
            {
                int position = concentrationPositions[i];
                int peakValue;
                
                if (i == concentrationPositions.Count - 1)
                {
                    // 最后一个位置，分配所有剩余值
                    peakValue = Mathf.Min(remainingValue, maxSinglePeak);
                }
                else
                {
                    // 随机分配，但不超过单个波峰最大值
                    peakValue = Random.Range(1, Mathf.Min(remainingValue / (concentrationPositions.Count - i), maxSinglePeak) + 1);
                }
                
                if (peakValue > 0)
                {
                    waveData.AddPeak(position, peakValue);
                    remainingValue -= peakValue;
                }
            }
        }
    }
}

