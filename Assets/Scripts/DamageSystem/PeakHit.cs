using UnityEngine;

namespace DamageSystem
{
    /// <summary>
    /// 波峰伤害数据
    /// 表示一个波峰对目标造成的伤害信息
    /// </summary>
    [System.Serializable]
    public class PeakHit
    {
        /// <summary>
        /// 目标实体（GameObject，必须挂载 HealthComponent）
        /// </summary>
        public GameObject target;

        /// <summary>
        /// 攻击者实体（GameObject，可选，用于应用攻击者的状态效果）
        /// </summary>
        public GameObject attacker;

        /// <summary>
        /// 伤害值（使用波峰强度的绝对值，支持小数点）
        /// </summary>
        public float damage;

        /// <summary>
        /// 序号/时间顺序（使用波峰位置确定，用于保证播放先后顺序）
        /// </summary>
        public int orderIndex;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="target">目标实体</param>
        /// <param name="damage">伤害值</param>
        /// <param name="orderIndex">序号</param>
        public PeakHit(GameObject target, float damage, int orderIndex) : this(target, null, damage, orderIndex)
        {
        }

        /// <summary>
        /// 构造函数（带攻击者）
        /// </summary>
        /// <param name="target">目标实体</param>
        /// <param name="attacker">攻击者实体</param>
        /// <param name="damage">伤害值</param>
        /// <param name="orderIndex">序号</param>
        public PeakHit(GameObject target, GameObject attacker, float damage, int orderIndex)
        {
            this.target = target;
            this.attacker = attacker;
            this.damage = damage;
            this.orderIndex = orderIndex;
        }

        public override string ToString()
        {
            return $"PeakHit(Attacker:{attacker?.name ?? "null"}, Target:{target?.name ?? "null"}, Damage:{damage:F2}, Order:{orderIndex})";
        }
    }
}

