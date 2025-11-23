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
            if (playerWave.IsEmpty)
            {
                if (debugLog)
                {
                    Debug.Log("[DamageSystemHelper] 玩家波为空，无需处理");
                }
                return;
            }

            // 步骤2：获取敌人波
            Wave enemyWave = enemyWaveManager.CurrentEnemyWave;
            if (enemyWave.IsEmpty)
            {
                if (debugLog)
                {
                    Debug.Log("[DamageSystemHelper] 敌人波为空，直接使用玩家波生成伤害序列");
                }
                // 如果敌人波为空，直接使用玩家波生成伤害序列
                List<PeakHit> emptyEnemyHitSequence = WaveHitSequenceGenerator.GenerateHitSequence(playerWave, targetManager);
                ProcessHitSequence(emptyEnemyHitSequence);
                return;
            }

            // 步骤3：配对两个波
            List<Wave> pairedWaves = WavePairing.PairWaves(playerWave, enemyWave);
            
            if (debugLog)
            {
                Debug.Log($"[DamageSystemHelper] 玩家波和敌人波配对完成，生成 {pairedWaves.Count} 个结果波");
                foreach (var wave in pairedWaves)
                {
                    Debug.Log($"[DamageSystemHelper] 结果波：方向={(wave.AttackDirection.HasValue ? (wave.AttackDirection.Value ? "攻向玩家" : "攻向敌人") : "空")}，波峰数={wave.PeakCount}");
                }
            }

            // 步骤4：从配对后的波生成伤害序列
            List<PeakHit> pairedHitSequence = WaveHitSequenceGenerator.GenerateHitSequenceFromPairedWaves(pairedWaves, targetManager);

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

