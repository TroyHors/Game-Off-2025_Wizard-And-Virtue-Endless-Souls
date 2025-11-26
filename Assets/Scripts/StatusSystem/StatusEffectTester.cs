using UnityEngine;
using StatusSystem;
using DamageSystem;

namespace StatusSystem
{
    /// <summary>
    /// 状态效果测试器
    /// 用于测试状态系统的各项功能，特别是敌人身上的效果
    /// </summary>
    public class StatusEffectTester : MonoBehaviour
    {
        [Header("测试目标")]
        [Tooltip("玩家实体（用于测试玩家状态效果）")]
        [SerializeField] private GameObject playerTarget;

        [Tooltip("敌人实体（用于测试敌人状态效果）")]
        [SerializeField] private GameObject enemyTarget;

        [Header("测试参数")]
        [Tooltip("测试伤害值")]
        [SerializeField] private int testDamage = 100;

        [Tooltip("测试状态数值")]
        [SerializeField] private float testStatusValue = 0.8f;

        [Tooltip("测试状态持续回合数")]
        [SerializeField] private int testStatusDuration = 3;

        private void Awake()
        {
            // 自动查找目标（如果未设置）
            if (playerTarget == null)
            {
                playerTarget = GameObject.FindGameObjectWithTag("Player");
            }

            if (enemyTarget == null)
            {
                GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
                if (enemy != null)
                {
                    enemyTarget = enemy;
                }
            }
        }

        #region 基础功能测试

        [ContextMenu("测试1: 为玩家添加受到伤害减少状态")]
        public void Test1_AddDamageTakenReductionToPlayer()
        {
            if (playerTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 玩家目标未设置");
                return;
            }

            StatusEffectManager statusManager = playerTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 玩家目标缺少 StatusEffectManager 组件");
                return;
            }

            statusManager.AddStatusEffect("护甲", StatusEffectType.DamageTakenReduction, testStatusValue, testStatusDuration);
            Debug.Log($"[StatusEffectTester] 测试1完成: 为玩家添加了'护甲'状态（受到伤害减少{(1 - testStatusValue) * 100}%，持续{testStatusDuration}回合）");
        }

        [ContextMenu("测试2: 为敌人添加受到伤害减少状态")]
        public void Test2_AddDamageTakenReductionToEnemy()
        {
            if (enemyTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标未设置");
                return;
            }

            StatusEffectManager statusManager = enemyTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标缺少 StatusEffectManager 组件");
                return;
            }

            statusManager.AddStatusEffect("护甲", StatusEffectType.DamageTakenReduction, testStatusValue, testStatusDuration);
            Debug.Log($"[StatusEffectTester] 测试2完成: 为敌人添加了'护甲'状态（受到伤害减少{(1 - testStatusValue) * 100}%，持续{testStatusDuration}回合）");
        }

        [ContextMenu("测试3: 为玩家添加攻击伤害增加状态")]
        public void Test3_AddDamageDealtIncreaseToPlayer()
        {
            if (playerTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 玩家目标未设置");
                return;
            }

            StatusEffectManager statusManager = playerTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 玩家目标缺少 StatusEffectManager 组件");
                return;
            }

            float increaseValue = 1.5f; // 增加50%
            statusManager.AddStatusEffect("力量", StatusEffectType.DamageDealtIncrease, increaseValue, testStatusDuration);
            Debug.Log($"[StatusEffectTester] 测试3完成: 为玩家添加了'力量'状态（攻击伤害增加{(increaseValue - 1) * 100}%，持续{testStatusDuration}回合）");
        }

        [ContextMenu("测试4: 为敌人添加攻击伤害增加状态")]
        public void Test4_AddDamageDealtIncreaseToEnemy()
        {
            if (enemyTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标未设置");
                return;
            }

            StatusEffectManager statusManager = enemyTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标缺少 StatusEffectManager 组件");
                return;
            }

            float increaseValue = 1.3f; // 增加30%
            statusManager.AddStatusEffect("狂暴", StatusEffectType.DamageDealtIncrease, increaseValue, testStatusDuration);
            Debug.Log($"[StatusEffectTester] 测试4完成: 为敌人添加了'狂暴'状态（攻击伤害增加{(increaseValue - 1) * 100}%，持续{testStatusDuration}回合）");
        }

        [ContextMenu("测试5: 为敌人添加受到伤害增加状态")]
        public void Test5_AddDamageTakenIncreaseToEnemy()
        {
            if (enemyTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标未设置");
                return;
            }

            StatusEffectManager statusManager = enemyTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标缺少 StatusEffectManager 组件");
                return;
            }

            float increaseValue = 1.5f; // 增加50%
            statusManager.AddStatusEffect("易伤", StatusEffectType.DamageTakenIncrease, increaseValue, testStatusDuration);
            Debug.Log($"[StatusEffectTester] 测试5完成: 为敌人添加了'易伤'状态（受到伤害增加{(increaseValue - 1) * 100}%，持续{testStatusDuration}回合）");
        }

        [ContextMenu("测试6: 为玩家添加攻击伤害减少状态")]
        public void Test6_AddDamageDealtReductionToPlayer()
        {
            if (playerTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 玩家目标未设置");
                return;
            }

            StatusEffectManager statusManager = playerTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 玩家目标缺少 StatusEffectManager 组件");
                return;
            }

            statusManager.AddStatusEffect("虚弱", StatusEffectType.DamageDealtReduction, testStatusValue, testStatusDuration);
            Debug.Log($"[StatusEffectTester] 测试6完成: 为玩家添加了'虚弱'状态（攻击伤害减少{(1 - testStatusValue) * 100}%，持续{testStatusDuration}回合）");
        }

        #endregion

        #region 状态效果计算测试

        [ContextMenu("测试7: 测试敌人受到伤害修正（应用状态效果）")]
        public void Test7_TestEnemyDamageTakenModifier()
        {
            if (enemyTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标未设置");
                return;
            }

            StatusEffectManager statusManager = enemyTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标缺少 StatusEffectManager 组件");
                return;
            }

            HealthComponent healthComponent = enemyTarget.GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标缺少 HealthComponent 组件");
                return;
            }

            float originalDamage = testDamage;
            float modifiedDamage = statusManager.ApplyDamageTakenModifier(originalDamage);
            float multiplier = statusManager.GetDamageTakenMultiplier();

            Debug.Log($"[StatusEffectTester] ========== 测试7: 敌人受到伤害修正 ==========");
            Debug.Log($"[StatusEffectTester] 原始伤害: {originalDamage:F2}");
            Debug.Log($"[StatusEffectTester] 修正倍数: {multiplier:F2}x");
            Debug.Log($"[StatusEffectTester] 修正后伤害: {modifiedDamage:F2}");
            Debug.Log($"[StatusEffectTester] 实际造成伤害: {healthComponent.TakeDamage(modifiedDamage):F2}");
            Debug.Log($"[StatusEffectTester] 敌人当前生命值: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");
            Debug.Log($"[StatusEffectTester] ==========================================");
        }

        [ContextMenu("测试8: 测试敌人造成伤害修正（应用状态效果）")]
        public void Test8_TestEnemyDamageDealtModifier()
        {
            if (enemyTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标未设置");
                return;
            }

            StatusEffectManager statusManager = enemyTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标缺少 StatusEffectManager 组件");
                return;
            }

            float originalDamage = testDamage;
            float modifiedDamage = statusManager.ApplyDamageDealtModifier(originalDamage);
            float multiplier = statusManager.GetDamageDealtMultiplier();

            Debug.Log($"[StatusEffectTester] ========== 测试8: 敌人造成伤害修正 ==========");
            Debug.Log($"[StatusEffectTester] 原始伤害: {originalDamage:F2}");
            Debug.Log($"[StatusEffectTester] 修正倍数: {multiplier:F2}x");
            Debug.Log($"[StatusEffectTester] 修正后伤害: {modifiedDamage:F2}");
            Debug.Log($"[StatusEffectTester] ==========================================");
        }

        [ContextMenu("测试9: 测试状态效果叠加（敌人多个状态相乘）")]
        public void Test9_TestStatusEffectStacking()
        {
            if (enemyTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标未设置");
                return;
            }

            StatusEffectManager statusManager = enemyTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标缺少 StatusEffectManager 组件");
                return;
            }

            // 清除所有状态
            statusManager.ClearAllStatusEffects();

            // 添加多个状态效果
            statusManager.AddStatusEffect("护甲1", StatusEffectType.DamageTakenReduction, 0.8f, 3);
            statusManager.AddStatusEffect("护甲2", StatusEffectType.DamageTakenReduction, 0.7f, 2);
            statusManager.AddStatusEffect("护盾", StatusEffectType.DamageTakenReduction, 0.5f, 1);

            float originalDamage = testDamage;
            float modifiedDamage = statusManager.ApplyDamageTakenModifier(originalDamage);
            float multiplier = statusManager.GetDamageTakenMultiplier();

            Debug.Log($"[StatusEffectTester] ========== 测试9: 状态效果叠加 ==========");
            Debug.Log($"[StatusEffectTester] 为敌人添加了3个'受到伤害减少'状态:");
            Debug.Log($"[StatusEffectTester]   - 护甲1: 0.8x (减少20%)");
            Debug.Log($"[StatusEffectTester]   - 护甲2: 0.7x (减少30%)");
            Debug.Log($"[StatusEffectTester]   - 护盾: 0.5x (减少50%)");
            Debug.Log($"[StatusEffectTester] 原始伤害: {originalDamage:F2}");
            Debug.Log($"[StatusEffectTester] 计算过程: {originalDamage:F2} × 0.8 × 0.7 × 0.5 = {originalDamage * 0.8f * 0.7f * 0.5f:F2}");
            Debug.Log($"[StatusEffectTester] 修正倍数: {multiplier:F4}x");
            Debug.Log($"[StatusEffectTester] 修正后伤害: {modifiedDamage:F2}");
            Debug.Log($"[StatusEffectTester] 预期结果: {originalDamage * 0.8f * 0.7f * 0.5f:F2}");
            Debug.Log($"[StatusEffectTester] ==========================================");
        }

        #endregion

        #region 回合结束测试

        [ContextMenu("测试10: 测试敌人状态效果回合结束（减少持续回合数）")]
        public void Test10_TestEnemyStatusTurnEnd()
        {
            if (enemyTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标未设置");
                return;
            }

            StatusEffectManager statusManager = enemyTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标缺少 StatusEffectManager 组件");
                return;
            }

            // 清除所有状态
            statusManager.ClearAllStatusEffects();

            // 添加不同持续回合数的状态
            statusManager.AddStatusEffect("临时护甲", StatusEffectType.DamageTakenReduction, 0.8f, 3);
            statusManager.AddStatusEffect("临时虚弱", StatusEffectType.DamageDealtReduction, 0.7f, 2);
            statusManager.AddStatusEffect("临时易伤", StatusEffectType.DamageTakenIncrease, 1.5f, 1);
            statusManager.AddStatusEffect("永久力量", StatusEffectType.DamageDealtIncrease, 1.3f, -1); // 永久

            Debug.Log($"[StatusEffectTester] ========== 测试10: 敌人状态效果回合结束 ==========");
            Debug.Log($"[StatusEffectTester] 回合开始前状态:");
            foreach (var status in statusManager.StatusEffects)
            {
                string durationStr = status.IsPermanent ? "永久" : $"{status.Duration}回合";
                Debug.Log($"[StatusEffectTester]   - {status.StatusName}: {status.EffectType}, {durationStr}");
            }

            // 模拟回合结束
            statusManager.OnTurnEnd();

            Debug.Log($"[StatusEffectTester] 回合结束后状态:");
            if (statusManager.StatusEffects.Count == 0)
            {
                Debug.Log($"[StatusEffectTester]   无状态效果（全部已过期）");
            }
            else
            {
                foreach (var status in statusManager.StatusEffects)
                {
                    string durationStr = status.IsPermanent ? "永久" : $"{status.Duration}回合";
                    Debug.Log($"[StatusEffectTester]   - {status.StatusName}: {status.EffectType}, {durationStr}");
                }
            }
            Debug.Log($"[StatusEffectTester] ==========================================");
        }

        #endregion

        #region 综合测试

        [ContextMenu("测试11: 综合测试 - 敌人完整状态效果流程")]
        public void Test11_ComprehensiveEnemyStatusTest()
        {
            if (enemyTarget == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标未设置");
                return;
            }

            StatusEffectManager statusManager = enemyTarget.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标缺少 StatusEffectManager 组件");
                return;
            }

            HealthComponent healthComponent = enemyTarget.GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                Debug.LogError("[StatusEffectTester] 敌人目标缺少 HealthComponent 组件");
                return;
            }

            // 重置敌人生命值
            healthComponent.SetCurrentHealth(healthComponent.MaxHealth);

            // 清除所有状态
            statusManager.ClearAllStatusEffects();

            Debug.Log($"[StatusEffectTester] ========== 测试11: 敌人完整状态效果流程 ==========");

            // 步骤1: 添加状态效果
            Debug.Log($"[StatusEffectTester] 步骤1: 添加状态效果");
            statusManager.AddStatusEffect("护甲", StatusEffectType.DamageTakenReduction, 0.8f, 3);
            statusManager.AddStatusEffect("狂暴", StatusEffectType.DamageDealtIncrease, 1.5f, 2);
            Debug.Log($"[StatusEffectTester]   添加了'护甲'状态（受到伤害减少20%，持续3回合）");
            Debug.Log($"[StatusEffectTester]   添加了'狂暴'状态（攻击伤害增加50%，持续2回合）");

            // 步骤2: 测试受到伤害修正
            Debug.Log($"[StatusEffectTester] 步骤2: 测试受到伤害修正");
            float originalDamage = testDamage;
            float modifiedDamage = statusManager.ApplyDamageTakenModifier(originalDamage);
            Debug.Log($"[StatusEffectTester]   原始伤害: {originalDamage:F2}");
            Debug.Log($"[StatusEffectTester]   修正后伤害: {modifiedDamage:F2} (倍数: {statusManager.GetDamageTakenMultiplier():F2}x)");
            
            // 实际造成伤害
            float actualDamage = healthComponent.TakeDamage(modifiedDamage);
            Debug.Log($"[StatusEffectTester]   实际造成伤害: {actualDamage:F2}");
            Debug.Log($"[StatusEffectTester]   敌人剩余生命值: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");

            // 步骤3: 测试造成伤害修正
            Debug.Log($"[StatusEffectTester] 步骤3: 测试造成伤害修正");
            float originalDealtDamage = testDamage;
            float modifiedDealtDamage = statusManager.ApplyDamageDealtModifier(originalDealtDamage);
            Debug.Log($"[StatusEffectTester]   原始伤害: {originalDealtDamage:F2}");
            Debug.Log($"[StatusEffectTester]   修正后伤害: {modifiedDealtDamage:F2} (倍数: {statusManager.GetDamageDealtMultiplier():F2}x)");

            // 步骤4: 模拟回合结束
            Debug.Log($"[StatusEffectTester] 步骤4: 模拟回合结束");
            statusManager.OnTurnEnd();
            Debug.Log($"[StatusEffectTester]   当前状态效果:");
            foreach (var status in statusManager.StatusEffects)
            {
                string durationStr = status.IsPermanent ? "永久" : $"{status.Duration}回合";
                Debug.Log($"[StatusEffectTester]     - {status.StatusName}: {durationStr}");
            }

            Debug.Log($"[StatusEffectTester] ==========================================");
        }

        [ContextMenu("测试12: 清除所有状态效果（玩家和敌人）")]
        public void Test12_ClearAllStatusEffects()
        {
            int clearedCount = 0;

            // 清除玩家状态
            if (playerTarget != null)
            {
                StatusEffectManager playerStatusManager = playerTarget.GetComponent<StatusEffectManager>();
                if (playerStatusManager != null)
                {
                    int playerStatusCount = playerStatusManager.StatusEffects.Count;
                    playerStatusManager.ClearAllStatusEffects();
                    clearedCount += playerStatusCount;
                    Debug.Log($"[StatusEffectTester] 清除了玩家身上的 {playerStatusCount} 个状态效果");
                }
            }

            // 清除敌人状态
            if (enemyTarget != null)
            {
                StatusEffectManager enemyStatusManager = enemyTarget.GetComponent<StatusEffectManager>();
                if (enemyStatusManager != null)
                {
                    int enemyStatusCount = enemyStatusManager.StatusEffects.Count;
                    enemyStatusManager.ClearAllStatusEffects();
                    clearedCount += enemyStatusCount;
                    Debug.Log($"[StatusEffectTester] 清除了敌人身上的 {enemyStatusCount} 个状态效果");
                }
            }

            Debug.Log($"[StatusEffectTester] 测试12完成: 总共清除了 {clearedCount} 个状态效果");
        }

        [ContextMenu("测试13: 显示所有状态效果（玩家和敌人）")]
        public void Test13_DisplayAllStatusEffects()
        {
            Debug.Log($"[StatusEffectTester] ========== 当前所有状态效果 ==========");

            // 显示玩家状态
            if (playerTarget != null)
            {
                StatusEffectManager playerStatusManager = playerTarget.GetComponent<StatusEffectManager>();
                if (playerStatusManager != null)
                {
                    Debug.Log($"[StatusEffectTester] 玩家状态效果 ({playerStatusManager.StatusEffects.Count}个):");
                    if (playerStatusManager.StatusEffects.Count == 0)
                    {
                        Debug.Log($"[StatusEffectTester]   无状态效果");
                    }
                    else
                    {
                        foreach (var status in playerStatusManager.StatusEffects)
                        {
                            string durationStr = status.IsPermanent ? "永久" : $"{status.Duration}回合";
                            Debug.Log($"[StatusEffectTester]   - {status.StatusName}: {status.EffectType}, {status.Value:F2}x, {durationStr}");
                        }
                    }
                }
            }

            // 显示敌人状态
            if (enemyTarget != null)
            {
                StatusEffectManager enemyStatusManager = enemyTarget.GetComponent<StatusEffectManager>();
                if (enemyStatusManager != null)
                {
                    Debug.Log($"[StatusEffectTester] 敌人状态效果 ({enemyStatusManager.StatusEffects.Count}个):");
                    if (enemyStatusManager.StatusEffects.Count == 0)
                    {
                        Debug.Log($"[StatusEffectTester]   无状态效果");
                    }
                    else
                    {
                        foreach (var status in enemyStatusManager.StatusEffects)
                        {
                            string durationStr = status.IsPermanent ? "永久" : $"{status.Duration}回合";
                            Debug.Log($"[StatusEffectTester]   - {status.StatusName}: {status.EffectType}, {status.Value:F2}x, {durationStr}");
                        }
                    }
                }
            }

            Debug.Log($"[StatusEffectTester] ==========================================");
        }

        #endregion

        #region 快速测试套件

        [ContextMenu("运行所有测试（敌人重点测试）")]
        public void RunAllEnemyTests()
        {
            Debug.Log($"[StatusEffectTester] ========== 开始运行所有测试（敌人重点测试） ==========");
            
            // 先清除所有状态
            Test12_ClearAllStatusEffects();
            
            // 基础功能测试
            Test2_AddDamageTakenReductionToEnemy();
            Test4_AddDamageDealtIncreaseToEnemy();
            Test5_AddDamageTakenIncreaseToEnemy();
            
            // 计算测试
            Test7_TestEnemyDamageTakenModifier();
            Test8_TestEnemyDamageDealtModifier();
            Test9_TestStatusEffectStacking();
            
            // 回合结束测试
            Test10_TestEnemyStatusTurnEnd();
            
            // 综合测试
            Test11_ComprehensiveEnemyStatusTest();
            
            // 显示最终状态
            Test13_DisplayAllStatusEffects();
            
            Debug.Log($"[StatusEffectTester] ========== 所有测试完成 ==========");
        }

        #endregion
    }
}

