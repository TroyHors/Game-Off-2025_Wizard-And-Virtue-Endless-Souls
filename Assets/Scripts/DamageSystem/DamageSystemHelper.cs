using System.Collections.Generic;
using UnityEngine;
using WaveSystem;

namespace DamageSystem
{
    /// <summary>
    /// 伤害系统辅助类
    /// 提供便捷方法，用于在 UnityEvent 中连接波系统和伤害系统
    /// </summary>
    public class DamageSystemHelper : MonoBehaviour
    {
        [Header("系统引用")]
        [Tooltip("手牌波格表管理器")]
        [SerializeField] private HandWaveGridManager handWaveGridManager;

        [Tooltip("敌人波管理器")]
        [SerializeField] private EnemyWaveManager enemyWaveManager;

        [Tooltip("目标管理器")]
        [SerializeField] private TargetManager targetManager;

        [Tooltip("伤害结算系统")]
        [SerializeField] private DamageSystem damageSystem;

        [Header("设置")]
        [Tooltip("是否使用异步处理（支持延迟，用于动画）")]
        [SerializeField] private bool useAsyncProcessing = false;

        [Tooltip("是否在配对时打印调试信息")]
        [SerializeField] private bool debugLog = true;

        /// <summary>
        /// 从发出的波生成伤害序列并处理（无返回值包装，供UnityEvent调用）
        /// 完整流程：发出玩家波 -> 获取敌人波 -> 配对 -> 生成伤害序列 -> 处理伤害序列
        /// </summary>
        public void ProcessEmittedWave()
        {
            if (handWaveGridManager == null)
            {
                Debug.LogError("[DamageSystemHelper] HandWaveGridManager 未设置");
                return;
            }

            if (enemyWaveManager == null)
            {
                Debug.LogError("[DamageSystemHelper] EnemyWaveManager 未设置");
                return;
            }

            if (targetManager == null)
            {
                Debug.LogError("[DamageSystemHelper] TargetManager 未设置");
                return;
            }

            if (damageSystem == null)
            {
                Debug.LogError("[DamageSystemHelper] DamageSystem 未设置");
                return;
            }

            // 步骤1：发出玩家波
            Wave playerWave = handWaveGridManager.EmitHandWaveWithResult();
            
            // 步骤2：获取敌人波
            Wave enemyWave = enemyWaveManager.CurrentEnemyWave;

            // 步骤3：计算结果波（使用新的结果波计算器）
            // 空波会被转换为所有位置强度为0的波，正常处理
            int minPosition = handWaveGridManager.MinGridPosition;
            int maxPosition = handWaveGridManager.MaxGridPosition;
            List<Wave> resultWaves = WaveResultCalculator.CalculateResultWaves(playerWave, enemyWave, minPosition, maxPosition);
            
            if (debugLog)
            {
                Debug.Log($"[DamageSystemHelper] ========== 发波匹配完成 ==========");
                Debug.Log($"[DamageSystemHelper] 玩家波和敌人波配对完成，生成 {resultWaves.Count} 个结果波");
                
                for (int i = 0; i < resultWaves.Count; i++)
                {
                    Wave wave = resultWaves[i];
                    string directionStr = wave.AttackDirection.HasValue 
                        ? (wave.AttackDirection.Value ? "攻向敌人" : "攻向玩家") 
                        : "空";
                    
                    Debug.Log($"[DamageSystemHelper] --- 结果波 #{i + 1} ---");
                    Debug.Log($"[DamageSystemHelper] 方向: {directionStr}");
                    Debug.Log($"[DamageSystemHelper] 波峰数: {wave.PeakCount}");
                    
                    if (wave.PeakCount > 0)
                    {
                        // 获取所有波峰（按位置排序）
                        var sortedPeaks = wave.GetSortedPeaks();
                        Debug.Log($"[DamageSystemHelper] 波峰详情:");
                        foreach (var (position, peak) in sortedPeaks)
                        {
                            // 只显示强度不为0的波峰
                            if (peak.Value != 0)
                            {
                                string peakDirectionStr = peak.AttackDirection ? "攻向敌人" : "攻向玩家";
                                Debug.Log($"[DamageSystemHelper]   位置 {position}: 强度={peak.Value}, 方向={peakDirectionStr}");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log($"[DamageSystemHelper] 波峰详情: 无波峰（空波）");
                    }
                }
                Debug.Log($"[DamageSystemHelper] ========================================");
            }

            // 步骤4：从结果波生成伤害序列
            List<PeakHit> pairedHitSequence = WaveHitSequenceGenerator.GenerateHitSequenceFromPairedWaves(resultWaves, targetManager);

            if (pairedHitSequence == null || pairedHitSequence.Count == 0)
            {
                if (debugLog)
                {
                    Debug.Log("[DamageSystemHelper] 伤害序列为空，无需处理");
                }
                return;
            }

            // 步骤5：处理伤害序列
            ProcessHitSequence(pairedHitSequence);
        }

        /// <summary>
        /// 处理伤害序列（内部方法）
        /// </summary>
        private void ProcessHitSequence(List<PeakHit> hitSequence)
        {
            if (useAsyncProcessing)
            {
                damageSystem.ProcessHitSequenceAsync(hitSequence);
            }
            else
            {
                damageSystem.ProcessHitSequence(hitSequence);
            }
        }
    }
}

