using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace StatusSystem
{
    /// <summary>
    /// 状态效果管理器
    /// 管理角色身上的所有状态效果，处理状态效果的叠加、持续回合、效果计算等
    /// 敌人和玩家通用
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        [Header("事件")]
        [Tooltip("状态效果添加时触发（状态效果）")]
        [SerializeField] private UnityEvent<StatusEffect> onStatusAdded = new UnityEvent<StatusEffect>();

        [Tooltip("状态效果移除时触发（状态效果名称）")]
        [SerializeField] private UnityEvent<string> onStatusRemoved = new UnityEvent<string>();

        [Tooltip("状态效果更新时触发（所有状态效果列表）")]
        [SerializeField] private UnityEvent<List<StatusEffect>> onStatusUpdated = new UnityEvent<List<StatusEffect>>();

        /// <summary>
        /// 当前所有状态效果列表（只读）
        /// </summary>
        private List<StatusEffect> statusEffects = new List<StatusEffect>();

        /// <summary>
        /// 当前所有状态效果列表（只读）
        /// </summary>
        public IReadOnlyList<StatusEffect> StatusEffects => statusEffects;

        /// <summary>
        /// 状态效果添加事件
        /// </summary>
        public UnityEvent<StatusEffect> OnStatusAdded => onStatusAdded;

        /// <summary>
        /// 状态效果移除事件
        /// </summary>
        public UnityEvent<string> OnStatusRemoved => onStatusRemoved;

        /// <summary>
        /// 状态效果更新事件
        /// </summary>
        public UnityEvent<List<StatusEffect>> OnStatusUpdated => onStatusUpdated;

        /// <summary>
        /// 添加状态效果
        /// </summary>
        /// <param name="statusEffect">要添加的状态效果</param>
        public void AddStatusEffect(StatusEffect statusEffect)
        {
            if (statusEffect == null)
            {
                Debug.LogWarning($"[StatusEffectManager] {gameObject.name} 尝试添加空的状态效果");
                return;
            }

            // 添加状态效果（允许同类型状态叠加）
            statusEffects.Add(statusEffect.Clone());
            
            Debug.Log($"[StatusEffectManager] {gameObject.name} 添加状态效果: {statusEffect}");
            onStatusAdded?.Invoke(statusEffect);
            onStatusUpdated?.Invoke(new List<StatusEffect>(statusEffects));
        }

        /// <summary>
        /// 添加状态效果（便捷方法）
        /// </summary>
        /// <param name="statusName">状态名称</param>
        /// <param name="effectType">状态类型</param>
        /// <param name="value">状态数值（倍数）</param>
        /// <param name="duration">持续回合数（-1表示永久）</param>
        public void AddStatusEffect(string statusName, StatusEffectType effectType, float value, int duration = 1)
        {
            StatusEffect statusEffect = new StatusEffect(statusName, effectType, value, duration);
            AddStatusEffect(statusEffect);
        }

        /// <summary>
        /// 移除指定名称的状态效果（移除所有同名状态）
        /// </summary>
        /// <param name="statusName">状态名称</param>
        /// <returns>移除的数量</returns>
        public int RemoveStatusEffect(string statusName)
        {
            int removedCount = statusEffects.RemoveAll(s => s.StatusName == statusName);
            
            if (removedCount > 0)
            {
                Debug.Log($"[StatusEffectManager] {gameObject.name} 移除状态效果: {statusName} (共{removedCount}个)");
                onStatusRemoved?.Invoke(statusName);
                onStatusUpdated?.Invoke(new List<StatusEffect>(statusEffects));
            }

            return removedCount;
        }

        /// <summary>
        /// 移除所有状态效果
        /// </summary>
        public void ClearAllStatusEffects()
        {
            int count = statusEffects.Count;
            statusEffects.Clear();
            
            if (count > 0)
            {
                Debug.Log($"[StatusEffectManager] {gameObject.name} 清除所有状态效果 (共{count}个)");
                onStatusUpdated?.Invoke(new List<StatusEffect>(statusEffects));
            }
        }

        /// <summary>
        /// 回合结束处理（减少所有状态效果的持续回合数，移除过期的状态）
        /// </summary>
        public void OnTurnEnd()
        {
            // 减少所有状态效果的持续回合数
            foreach (var status in statusEffects)
            {
                status.ReduceDuration();
            }

            // 移除过期的状态效果
            int removedCount = statusEffects.RemoveAll(s => s.IsExpired);
            
            if (removedCount > 0)
            {
                Debug.Log($"[StatusEffectManager] {gameObject.name} 回合结束，移除 {removedCount} 个过期状态效果");
                onStatusUpdated?.Invoke(new List<StatusEffect>(statusEffects));
            }
        }

        /// <summary>
        /// 计算受到伤害的修正倍数
        /// 将所有"受到伤害减少/增加"的状态效果相乘
        /// </summary>
        /// <returns>修正倍数（例如0.8表示受到80%伤害，1.2表示受到120%伤害）</returns>
        public float GetDamageTakenMultiplier()
        {
            float multiplier = 1f;

            foreach (var status in statusEffects)
            {
                if (status.EffectType == StatusEffectType.DamageTakenReduction)
                {
                    multiplier *= status.Value;
                }
                else if (status.EffectType == StatusEffectType.DamageTakenIncrease)
                {
                    multiplier *= status.Value;
                }
            }

            return multiplier;
        }

        /// <summary>
        /// 计算造成伤害的修正倍数
        /// 将所有"攻击伤害减少/增加"的状态效果相乘
        /// </summary>
        /// <returns>修正倍数（例如0.8表示造成80%伤害，1.2表示造成120%伤害）</returns>
        public float GetDamageDealtMultiplier()
        {
            float multiplier = 1f;

            foreach (var status in statusEffects)
            {
                if (status.EffectType == StatusEffectType.DamageDealtReduction)
                {
                    multiplier *= status.Value;
                }
                else if (status.EffectType == StatusEffectType.DamageDealtIncrease)
                {
                    multiplier *= status.Value;
                }
            }

            return multiplier;
        }

        /// <summary>
        /// 应用受到伤害修正（在受到伤害时调用）
        /// </summary>
        /// <param name="originalDamage">原始伤害值（支持小数点）</param>
        /// <returns>修正后的伤害值（支持小数点）</returns>
        public float ApplyDamageTakenModifier(float originalDamage)
        {
            float multiplier = GetDamageTakenMultiplier();
            float modifiedDamage = originalDamage * multiplier;
            
            if (multiplier != 1f)
            {
                Debug.Log($"[StatusEffectManager] {gameObject.name} 受到伤害修正: {originalDamage:F2} -> {modifiedDamage:F2} (倍数: {multiplier:F2}x)");
            }

            return modifiedDamage;
        }

        /// <summary>
        /// 应用造成伤害修正（在造成伤害时调用）
        /// </summary>
        /// <param name="originalDamage">原始伤害值（支持小数点）</param>
        /// <returns>修正后的伤害值（支持小数点）</returns>
        public float ApplyDamageDealtModifier(float originalDamage)
        {
            float multiplier = GetDamageDealtMultiplier();
            float modifiedDamage = originalDamage * multiplier;
            
            if (multiplier != 1f)
            {
                Debug.Log($"[StatusEffectManager] {gameObject.name} 造成伤害修正: {originalDamage:F2} -> {modifiedDamage:F2} (倍数: {multiplier:F2}x)");
            }

            return modifiedDamage;
        }

        /// <summary>
        /// 获取指定类型的状态效果列表
        /// </summary>
        /// <param name="effectType">状态类型</param>
        /// <returns>状态效果列表</returns>
        public List<StatusEffect> GetStatusEffectsByType(StatusEffectType effectType)
        {
            return statusEffects.Where(s => s.EffectType == effectType).ToList();
        }

        /// <summary>
        /// 检查是否有指定名称的状态效果
        /// </summary>
        /// <param name="statusName">状态名称</param>
        /// <returns>是否存在</returns>
        public bool HasStatusEffect(string statusName)
        {
            return statusEffects.Any(s => s.StatusName == statusName);
        }

        /// <summary>
        /// 获取指定名称的状态效果数量
        /// </summary>
        /// <param name="statusName">状态名称</param>
        /// <returns>数量</returns>
        public int GetStatusEffectCount(string statusName)
        {
            return statusEffects.Count(s => s.StatusName == statusName);
        }
    }
}

