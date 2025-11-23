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
        /// 伤害值（使用波峰强度的绝对值）
        /// </summary>
        public int damage;

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
        public PeakHit(GameObject target, int damage, int orderIndex)
        {
            this.target = target;
            this.damage = damage;
            this.orderIndex = orderIndex;
        }

        public override string ToString()
        {
            return $"PeakHit(Target:{target?.name ?? "null"}, Damage:{damage}, Order:{orderIndex})";
        }
    }
}

