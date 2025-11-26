using UnityEngine;

namespace StatusSystem
{
    /// <summary>
    /// 状态效果类型
    /// </summary>
    public enum StatusEffectType
    {
        /// <summary>
        /// 受到伤害减少（数值为倍数，例如0.8表示减少20%，即受到80%伤害）
        /// </summary>
        DamageTakenReduction,

        /// <summary>
        /// 攻击伤害减少（数值为倍数，例如0.8表示减少20%，即造成80%伤害）
        /// </summary>
        DamageDealtReduction,

        /// <summary>
        /// 受到伤害增加（数值为倍数，例如1.2表示增加20%，即受到120%伤害）
        /// </summary>
        DamageTakenIncrease,

        /// <summary>
        /// 攻击伤害增加（数值为倍数，例如1.2表示增加20%，即造成120%伤害）
        /// </summary>
        DamageDealtIncrease
    }

    /// <summary>
    /// 状态效果数据
    /// 表示一个状态效果的完整信息
    /// </summary>
    [System.Serializable]
    public class StatusEffect
    {
        [Header("状态信息")]
        [Tooltip("状态名称（用于UI显示和识别）")]
        [SerializeField] private string statusName;

        [Tooltip("状态类型")]
        [SerializeField] private StatusEffectType effectType;

        [Tooltip("状态数值（倍数，例如0.8表示减少20%，1.2表示增加20%）")]
        [SerializeField] private float value;

        [Tooltip("持续回合数（-1表示永久）")]
        [SerializeField] private int duration = 1;

        /// <summary>
        /// 状态名称（用于UI显示和识别）
        /// </summary>
        public string StatusName => statusName;

        /// <summary>
        /// 状态类型
        /// </summary>
        public StatusEffectType EffectType => effectType;

        /// <summary>
        /// 状态数值（倍数）
        /// </summary>
        public float Value => value;

        /// <summary>
        /// 持续回合数（-1表示永久）
        /// </summary>
        public int Duration => duration;

        /// <summary>
        /// 是否已过期（持续回合数为0）
        /// </summary>
        public bool IsExpired => duration == 0;

        /// <summary>
        /// 是否永久（持续回合数为-1）
        /// </summary>
        public bool IsPermanent => duration == -1;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="statusName">状态名称</param>
        /// <param name="effectType">状态类型</param>
        /// <param name="value">状态数值（倍数）</param>
        /// <param name="duration">持续回合数（-1表示永久）</param>
        public StatusEffect(string statusName, StatusEffectType effectType, float value, int duration = 1)
        {
            this.statusName = statusName;
            this.effectType = effectType;
            this.value = value;
            this.duration = duration;
        }

        /// <summary>
        /// 减少持续回合数（回合结束时调用）
        /// </summary>
        public void ReduceDuration()
        {
            if (!IsPermanent && duration > 0)
            {
                duration--;
            }
        }

        /// <summary>
        /// 创建状态效果的副本
        /// </summary>
        public StatusEffect Clone()
        {
            return new StatusEffect(statusName, effectType, value, duration);
        }

        public override string ToString()
        {
            string durationStr = IsPermanent ? "永久" : $"{duration}回合";
            return $"{statusName}({effectType}, {value:F2}x, {durationStr})";
        }
    }
}

