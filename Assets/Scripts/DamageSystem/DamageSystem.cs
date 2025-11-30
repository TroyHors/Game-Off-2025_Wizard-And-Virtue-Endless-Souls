using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WaveSystem;

namespace DamageSystem
{
    /// <summary>
    /// 伤害结算系统
    /// 处理波系统输出的有序波峰伤害列表
    /// 逐个依次命中结算，不区分玩家或敌人，只通过 target 和组件系统找到目标
    /// </summary>
    public class DamageSystem : MonoBehaviour
    {
        [Header("设置")]
        [Tooltip("是否在结算时打印调试信息")]
        [SerializeField] private bool debugLog = true;

        [Tooltip("每个伤害之间的延迟时间（秒），用于表现层播放动画")]
        [SerializeField] private float hitDelay = 0.1f;

        [Header("事件")]
        [Tooltip("开始处理伤害序列时触发（伤害序列）")]
        [SerializeField] private UnityEvent<List<PeakHit>> onHitSequenceStart = new UnityEvent<List<PeakHit>>();

        [Tooltip("单个伤害结算时触发（PeakHit，目标剩余生命值，目标剩余护盾值）")]
        [SerializeField] private UnityEvent<PeakHit, int, int> onHitProcessed = new UnityEvent<PeakHit, int, int>();

        [Tooltip("伤害序列处理完成时触发（总伤害数，死亡数）")]
        [SerializeField] private UnityEvent<int, int> onHitSequenceComplete = new UnityEvent<int, int>();

        [Tooltip("目标死亡时触发（死亡的GameObject）")]
        [SerializeField] private UnityEvent<GameObject> onTargetDeath = new UnityEvent<GameObject>();

        /// <summary>
        /// 开始处理伤害序列事件
        /// </summary>
        public UnityEvent<List<PeakHit>> OnHitSequenceStart => onHitSequenceStart;

        /// <summary>
        /// 单个伤害结算事件（供表现层播放动画）
        /// </summary>
        public UnityEvent<PeakHit, int, int> OnHitProcessed => onHitProcessed;

        /// <summary>
        /// 伤害序列处理完成事件
        /// </summary>
        public UnityEvent<int, int> OnHitSequenceComplete => onHitSequenceComplete;

        /// <summary>
        /// 目标死亡事件（供战斗流程系统订阅）
        /// </summary>
        public UnityEvent<GameObject> OnTargetDeath => onTargetDeath;

        /// <summary>
        /// 是否正在处理伤害序列
        /// </summary>
        public bool IsProcessing { get; private set; }

        /// <summary>
        /// 处理有序波峰伤害列表（无返回值包装，供UnityEvent调用）
        /// </summary>
        /// <param name="hitSequence">有序波峰伤害列表（必须已按 orderIndex 升序排序）</param>
        public void ProcessHitSequence(List<PeakHit> hitSequence)
        {
            ProcessHitSequenceWithResult(hitSequence);
        }

        /// <summary>
        /// 处理有序波峰伤害列表（同步版本）
        /// 逐个依次命中结算
        /// </summary>
        /// <param name="hitSequence">有序波峰伤害列表（必须已按 orderIndex 升序排序）</param>
        /// <returns>处理结果（总伤害数，死亡数）</returns>
        public (int totalHits, int deaths) ProcessHitSequenceWithResult(List<PeakHit> hitSequence)
        {
            if (hitSequence == null || hitSequence.Count == 0)
            {
                if (debugLog)
                {
                    Debug.Log("[DamageSystem] 伤害序列为空，无需处理");
                }
                return (0, 0);
            }

            // 验证序列是否已排序
            if (!IsSorted(hitSequence))
            {
                Debug.LogWarning("[DamageSystem] 伤害序列未按 orderIndex 排序，将自动排序");
                hitSequence = SortHitSequence(hitSequence);
            }

            IsProcessing = true;
            onHitSequenceStart?.Invoke(hitSequence);

            int totalHits = 0;
            int deaths = 0;

            // 遍历 hits，逐个依次命中结算
            foreach (PeakHit hit in hitSequence)
            {
                // 通过 target 确定对象和其 HealthComponent
                if (hit.target == null)
                {
                    Debug.LogWarning($"[DamageSystem] PeakHit 的目标为空，跳过：{hit}");
                    continue;
                }

                HealthComponent healthComponent = hit.target.GetComponent<HealthComponent>();
                if (healthComponent == null)
                {
                    Debug.LogWarning($"[DamageSystem] 目标 {hit.target.name} 缺少 HealthComponent 组件，跳过：{hit}");
                    continue;
                }

                // 若目标不存在或已经死亡，跳过
                if (healthComponent.IsDead)
                {
                    if (debugLog)
                    {
                        Debug.Log($"[DamageSystem] 目标 {hit.target.name} 已死亡，跳过伤害：{hit}");
                    }
                    continue;
                }

                // 调用目标扣血函数
                float actualDamage = healthComponent.TakeDamage(hit.damage);
                totalHits++;

                // 获取结算后的状态
                int remainingHealth = healthComponent.CurrentHealth;
                int remainingShield = healthComponent.CurrentShield;

                // 触发单个伤害结算事件（供表现层播放动画）
                onHitProcessed?.Invoke(hit, remainingHealth, remainingShield);

                // 若目标在这次伤害后死亡，需要立刻更新死亡状态
                if (healthComponent.IsDead)
                {
                    deaths++;
                    if (debugLog)
                    {
                        Debug.Log($"[DamageSystem] 目标 {hit.target.name} 在受到 {hit.damage:F2} 点伤害后死亡（实际造成 {actualDamage:F2} 点生命值伤害）");
                    }
                    // 触发目标死亡事件
                    Debug.Log($"[DamageSystem] 触发OnTargetDeath事件，目标: {hit.target.name}，监听器数量: {onTargetDeath.GetPersistentEventCount()}");
                    onTargetDeath?.Invoke(hit.target);
                }
                else if (debugLog)
                {
                    Debug.Log($"[DamageSystem] 目标 {hit.target.name} 受到 {hit.damage:F2} 点伤害（实际造成 {actualDamage:F2} 点生命值伤害），剩余生命值：{remainingHealth}，剩余护盾：{remainingShield}");
                }
            }

            IsProcessing = false;
            onHitSequenceComplete?.Invoke(totalHits, deaths);

            if (debugLog)
            {
                Debug.Log($"[DamageSystem] 伤害序列处理完成，总伤害数：{totalHits}，死亡数：{deaths}");
            }

            return (totalHits, deaths);
        }

        /// <summary>
        /// 处理有序波峰伤害列表（异步版本，支持延迟）
        /// 逐个依次命中结算，每个伤害之间有延迟，用于表现层播放动画
        /// </summary>
        /// <param name="hitSequence">有序波峰伤害列表（必须已按 orderIndex 升序排序）</param>
        /// <returns>协程</returns>
        public Coroutine ProcessHitSequenceAsync(List<PeakHit> hitSequence)
        {
            return StartCoroutine(ProcessHitSequenceCoroutine(hitSequence));
        }

        /// <summary>
        /// 处理伤害序列的协程
        /// </summary>
        private IEnumerator ProcessHitSequenceCoroutine(List<PeakHit> hitSequence)
        {
            if (hitSequence == null || hitSequence.Count == 0)
            {
                if (debugLog)
                {
                    Debug.Log("[DamageSystem] 伤害序列为空，无需处理");
                }
                yield break;
            }

            // 验证序列是否已排序
            if (!IsSorted(hitSequence))
            {
                Debug.LogWarning("[DamageSystem] 伤害序列未按 orderIndex 排序，将自动排序");
                hitSequence = SortHitSequence(hitSequence);
            }

            IsProcessing = true;
            onHitSequenceStart?.Invoke(hitSequence);

            int totalHits = 0;
            int deaths = 0;

            // 遍历 hits，逐个依次命中结算
            foreach (PeakHit hit in hitSequence)
            {
                // 通过 target 确定对象和其 HealthComponent
                if (hit.target == null)
                {
                    Debug.LogWarning($"[DamageSystem] PeakHit 的目标为空，跳过：{hit}");
                    continue;
                }

                HealthComponent healthComponent = hit.target.GetComponent<HealthComponent>();
                if (healthComponent == null)
                {
                    Debug.LogWarning($"[DamageSystem] 目标 {hit.target.name} 缺少 HealthComponent 组件，跳过：{hit}");
                    continue;
                }

                // 若目标不存在或已经死亡，跳过
                if (healthComponent.IsDead)
                {
                    if (debugLog)
                    {
                        Debug.Log($"[DamageSystem] 目标 {hit.target.name} 已死亡，跳过伤害：{hit}");
                    }
                    continue;
                }

                // 调用目标扣血函数
                float actualDamage = healthComponent.TakeDamage(hit.damage);
                totalHits++;

                // 获取结算后的状态
                int remainingHealth = healthComponent.CurrentHealth;
                int remainingShield = healthComponent.CurrentShield;

                // 触发单个伤害结算事件（供表现层播放动画）
                onHitProcessed?.Invoke(hit, remainingHealth, remainingShield);

                // 若目标在这次伤害后死亡，需要立刻更新死亡状态
                if (healthComponent.IsDead)
                {
                    deaths++;
                    if (debugLog)
                    {
                        Debug.Log($"[DamageSystem] 目标 {hit.target.name} 在受到 {hit.damage:F2} 点伤害后死亡（实际造成 {actualDamage:F2} 点生命值伤害）");
                    }
                    // 触发目标死亡事件
                    Debug.Log($"[DamageSystem] 触发OnTargetDeath事件，目标: {hit.target.name}，监听器数量: {onTargetDeath.GetPersistentEventCount()}");
                    onTargetDeath?.Invoke(hit.target);
                }
                else if (debugLog)
                {
                    Debug.Log($"[DamageSystem] 目标 {hit.target.name} 受到 {hit.damage:F2} 点伤害（实际造成 {actualDamage:F2} 点生命值伤害），剩余生命值：{remainingHealth}，剩余护盾：{remainingShield}");
                }

                // 延迟，用于表现层播放动画
                if (hitDelay > 0)
                {
                    yield return new WaitForSeconds(hitDelay);
                }
            }

            IsProcessing = false;
            onHitSequenceComplete?.Invoke(totalHits, deaths);

            if (debugLog)
            {
                Debug.Log($"[DamageSystem] 伤害序列处理完成，总伤害数：{totalHits}，死亡数：{deaths}");
            }
        }

        /// <summary>
        /// 检查伤害序列是否已按 orderIndex 排序
        /// </summary>
        private bool IsSorted(List<PeakHit> hitSequence)
        {
            if (hitSequence == null || hitSequence.Count <= 1)
            {
                return true;
            }

            for (int i = 1; i < hitSequence.Count; i++)
            {
                if (hitSequence[i - 1].orderIndex > hitSequence[i].orderIndex)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 对伤害序列按 orderIndex 升序排序
        /// </summary>
        private List<PeakHit> SortHitSequence(List<PeakHit> hitSequence)
        {
            if (hitSequence == null)
            {
                return new List<PeakHit>();
            }

            List<PeakHit> sorted = new List<PeakHit>(hitSequence);
            sorted.Sort((a, b) => a.orderIndex.CompareTo(b.orderIndex));
            return sorted;
        }
    }
}

