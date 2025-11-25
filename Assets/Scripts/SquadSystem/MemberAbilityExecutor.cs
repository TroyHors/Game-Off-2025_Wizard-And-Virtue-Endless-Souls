using System.Collections.Generic;
using UnityEngine;
using DamageSystem;
using StatusSystem;

namespace SquadSystem
{
    /// <summary>
    /// 成员能力执行器
    /// 负责执行成员的能力效果
    /// </summary>
    public static class MemberAbilityExecutor
    {

        /// <summary>
        /// 执行成员能力
        /// </summary>
        /// <param name="ability">能力数据</param>
        /// <param name="member">成员GameObject</param>
        /// <param name="targetManager">目标管理器</param>
        public static void ExecuteAbility(MemberAbility ability, GameObject member, TargetManager targetManager)
        {
            if (ability == null || member == null || targetManager == null)
            {
                Debug.LogWarning("[MemberAbilityExecutor] 能力、成员或目标管理器为空，无法执行能力");
                return;
            }

            List<GameObject> targets = GetTargets(ability.Target, targetManager, member);
            if (targets == null || targets.Count == 0)
            {
                Debug.LogWarning($"[MemberAbilityExecutor] 无法找到目标，能力 '{ability.AbilityName}' 无法执行");
                return;
            }

            foreach (GameObject target in targets)
            {
                ExecuteAbilityOnTarget(ability, member, target);
            }
        }

        /// <summary>
        /// 获取能力目标列表
        /// </summary>
        private static List<GameObject> GetTargets(AbilityTarget targetType, TargetManager targetManager, GameObject member = null)
        {
            List<GameObject> targets = new List<GameObject>();

            switch (targetType)
            {
                case AbilityTarget.Player:
                    GameObject player = targetManager.Player;
                    if (player != null) targets.Add(player);
                    break;

                case AbilityTarget.Enemy:
                    GameObject enemy = targetManager.Enemy;
                    if (enemy != null) targets.Add(enemy);
                    break;

                case AbilityTarget.AllEnemies:
                    GameObject[] allEnemies = targetManager.GetAllEnemies();
                    if (allEnemies != null)
                    {
                        targets.AddRange(allEnemies);
                    }
                    break;

                case AbilityTarget.Self:
                    if (member != null) targets.Add(member);
                    break;

                case AbilityTarget.AllAllies:
                    GameObject playerAlly = targetManager.Player;
                    if (playerAlly != null) targets.Add(playerAlly);
                    // TODO: 添加所有成员到目标列表（需要从SquadManager获取）
                    break;
            }

            return targets;
        }

        /// <summary>
        /// 对目标执行能力效果
        /// </summary>
        private static void ExecuteAbilityOnTarget(MemberAbility ability, GameObject member, GameObject target)
        {
            switch (ability.AbilityType)
            {
                case MemberAbilityType.DirectDamage:
                    ExecuteDirectDamage(ability, target);
                    break;

                case MemberAbilityType.Heal:
                    ExecuteHeal(ability, target);
                    break;

                case MemberAbilityType.AddStatusEffect:
                    ExecuteAddStatusEffect(ability, target);
                    break;

                case MemberAbilityType.Custom:
                    ExecuteCustomAbility(ability, member, target);
                    break;
            }
        }

        /// <summary>
        /// 执行直伤
        /// </summary>
        private static void ExecuteDirectDamage(MemberAbility ability, GameObject target)
        {
            HealthComponent healthComponent = target.GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                Debug.LogWarning($"[MemberAbilityExecutor] 目标 {target.name} 缺少 HealthComponent，无法造成伤害");
                return;
            }

            // 检查目标是否已死亡
            if (healthComponent.IsDead)
            {
                Debug.Log($"[MemberAbilityExecutor] 目标 {target.name} 已死亡，跳过伤害");
                return;
            }

            float damage = ability.DamageValue;
            float actualDamage = healthComponent.TakeDamage(damage);
            Debug.Log($"[MemberAbilityExecutor] 能力 '{ability.AbilityName}' 对 {target.name} 造成 {damage:F2} 点伤害（实际造成 {actualDamage:F2} 点生命值伤害）");

            // 死亡事件由 HealthComponent 的 OnDeath 事件统一触发，无需在这里处理
        }

        /// <summary>
        /// 执行回血
        /// </summary>
        private static void ExecuteHeal(MemberAbility ability, GameObject target)
        {
            HealthComponent healthComponent = target.GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                Debug.LogWarning($"[MemberAbilityExecutor] 目标 {target.name} 缺少 HealthComponent，无法回复生命值");
                return;
            }

            int healAmount = Mathf.RoundToInt(ability.HealValue);
            healthComponent.Heal(healAmount);
            Debug.Log($"[MemberAbilityExecutor] 能力 '{ability.AbilityName}' 为 {target.name} 回复 {healAmount} 点生命值");
        }

        /// <summary>
        /// 执行添加状态效果
        /// </summary>
        private static void ExecuteAddStatusEffect(MemberAbility ability, GameObject target)
        {
            StatusEffectManager statusManager = target.GetComponent<StatusEffectManager>();
            if (statusManager == null)
            {
                Debug.LogWarning($"[MemberAbilityExecutor] 目标 {target.name} 缺少 StatusEffectManager，无法添加状态效果");
                return;
            }

            statusManager.AddStatusEffect(
                ability.StatusEffectName,
                ability.StatusEffectType,
                ability.StatusEffectValue,
                ability.StatusEffectDuration
            );
            Debug.Log($"[MemberAbilityExecutor] 能力 '{ability.AbilityName}' 为 {target.name} 添加状态效果: {ability.StatusEffectName}");
        }

        /// <summary>
        /// 执行自定义能力
        /// </summary>
        private static void ExecuteCustomAbility(MemberAbility ability, GameObject member, GameObject target)
        {
            // TODO: 实现自定义能力系统
            Debug.LogWarning($"[MemberAbilityExecutor] 自定义能力 '{ability.AbilityName}' 尚未实现");
        }
    }
}

