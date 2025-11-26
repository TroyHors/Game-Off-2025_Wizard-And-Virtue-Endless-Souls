using UnityEngine;

namespace SquadSystem
{
    /// <summary>
    /// 成员能力类型
    /// </summary>
    public enum MemberAbilityType
    {
        /// <summary>
        /// 直伤 - 直接造成伤害
        /// </summary>
        DirectDamage,

        /// <summary>
        /// 回血 - 回复生命值
        /// </summary>
        Heal,

        /// <summary>
        /// 添加状态效果 - 为目标添加状态效果
        /// </summary>
        AddStatusEffect,

        /// <summary>
        /// 自定义 - 通过脚本实现的自定义能力
        /// </summary>
        Custom
    }

    /// <summary>
    /// 成员能力数据
    /// 定义成员的能力配置
    /// </summary>
    [System.Serializable]
    public class MemberAbility
    {
        [Header("能力信息")]
        [Tooltip("能力名称（用于UI显示）")]
        [SerializeField] private string abilityName;

        [Tooltip("能力类型")]
        [SerializeField] private MemberAbilityType abilityType;

        [Header("触发条件")]
        [Tooltip("触发时机（回合开始/回合结束/战斗开始等）")]
        [SerializeField] private AbilityTrigger trigger = AbilityTrigger.TurnStart;

        [Header("目标")]
        [Tooltip("目标类型（玩家/敌人/所有敌人等）")]
        [SerializeField] private AbilityTarget target = AbilityTarget.Player;

        [Header("效果参数")]
        [Tooltip("伤害值（用于直伤类型）")]
        [SerializeField] private float damageValue = 0f;

        [Tooltip("回复值（用于回血类型）")]
        [SerializeField] private float healValue = 0f;

        [Tooltip("状态效果名称（用于添加状态效果类型）")]
        [SerializeField] private string statusEffectName;

        [Tooltip("状态效果类型（用于添加状态效果）")]
        [SerializeField] private StatusSystem.StatusEffectType statusEffectType = StatusSystem.StatusEffectType.DamageTakenReduction;

        [Tooltip("状态效果数值（用于添加状态效果）")]
        [SerializeField] private float statusEffectValue = 1f;

        [Tooltip("状态效果持续回合数（-1表示永久）")]
        [SerializeField] private int statusEffectDuration = 1;

        [Header("自定义能力")]
        [Tooltip("自定义能力脚本类名（用于自定义类型）")]
        [SerializeField] private string customAbilityClassName;

        /// <summary>
        /// 能力名称
        /// </summary>
        public string AbilityName => abilityName;

        /// <summary>
        /// 能力类型
        /// </summary>
        public MemberAbilityType AbilityType => abilityType;

        /// <summary>
        /// 触发时机
        /// </summary>
        public AbilityTrigger Trigger => trigger;

        /// <summary>
        /// 目标类型
        /// </summary>
        public AbilityTarget Target => target;

        /// <summary>
        /// 伤害值
        /// </summary>
        public float DamageValue => damageValue;

        /// <summary>
        /// 回复值
        /// </summary>
        public float HealValue => healValue;

        /// <summary>
        /// 状态效果名称
        /// </summary>
        public string StatusEffectName => statusEffectName;

        /// <summary>
        /// 状态效果类型
        /// </summary>
        public StatusSystem.StatusEffectType StatusEffectType => statusEffectType;

        /// <summary>
        /// 状态效果数值
        /// </summary>
        public float StatusEffectValue => statusEffectValue;

        /// <summary>
        /// 状态效果持续回合数
        /// </summary>
        public int StatusEffectDuration => statusEffectDuration;

        /// <summary>
        /// 自定义能力脚本类名
        /// </summary>
        public string CustomAbilityClassName => customAbilityClassName;
    }

    /// <summary>
    /// 能力触发时机
    /// </summary>
    public enum AbilityTrigger
    {
        /// <summary>
        /// 战斗开始
        /// </summary>
        CombatStart,

        /// <summary>
        /// 回合开始
        /// </summary>
        TurnStart,

        /// <summary>
        /// 回合结束
        /// </summary>
        TurnEnd,

        /// <summary>
        /// 每次攻击
        /// </summary>
        OnAttack,

        /// <summary>
        /// 受到伤害时
        /// </summary>
        OnTakeDamage
    }

    /// <summary>
    /// 能力目标类型
    /// </summary>
    public enum AbilityTarget
    {
        /// <summary>
        /// 玩家
        /// </summary>
        Player,

        /// <summary>
        /// 敌人（单个，随机选择）
        /// </summary>
        Enemy,

        /// <summary>
        /// 所有敌人
        /// </summary>
        AllEnemies,

        /// <summary>
        /// 自己（成员自身）
        /// </summary>
        Self,

        /// <summary>
        /// 所有友军（包括玩家和所有成员）
        /// </summary>
        AllAllies
    }
}

