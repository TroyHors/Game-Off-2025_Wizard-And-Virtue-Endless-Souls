using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// 玩家数据（持久化数据）
    /// 长期存在的玩家数据，包括血量、资源等
    /// 即使玩家实体不在场也可以修改这些数据
    /// 注意：卡组由 CardSystem 管理，不在此数据中
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Character System/Player Data")]
    public class PlayerData : ScriptableObject
    {
        [Header("生命值")]
        [Tooltip("最大生命值")]
        [SerializeField] private int maxHealth = 100;

        [Tooltip("当前生命值")]
        [SerializeField] private int currentHealth = 100;

        [Header("资源")]
        [Tooltip("当前资源值（可用于后续扩展）")]
        [SerializeField] private int currentResource = 0;

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHealth
        {
            get => maxHealth;
            set => maxHealth = Mathf.Max(1, value);
        }

        /// <summary>
        /// 当前生命值
        /// </summary>
        public int CurrentHealth
        {
            get => currentHealth;
            set => currentHealth = Mathf.Clamp(value, 0, maxHealth);
        }

        /// <summary>
        /// 当前资源值
        /// </summary>
        public int CurrentResource
        {
            get => currentResource;
            set => currentResource = Mathf.Max(0, value);
        }

        /// <summary>
        /// 是否已死亡
        /// </summary>
        public bool IsDead => currentHealth <= 0;

        /// <summary>
        /// 回复生命值（即使实体不在场也可以调用）
        /// </summary>
        /// <param name="healAmount">回复值</param>
        /// <returns>实际回复的生命值</returns>
        public int Heal(int healAmount)
        {
            if (IsDead)
            {
                Debug.LogWarning("[PlayerData] 玩家已死亡，无法回复生命值");
                return 0;
            }

            if (healAmount <= 0)
            {
                Debug.LogWarning($"[PlayerData] 回复值必须为正数，当前值：{healAmount}");
                return 0;
            }

            int oldHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
            int actualHeal = currentHealth - oldHealth;

            if (actualHeal > 0)
            {
                Debug.Log($"[PlayerData] 回复了 {actualHeal} 点生命值，当前生命值：{currentHealth}/{maxHealth}");
            }

            return actualHeal;
        }

        /// <summary>
        /// 受到伤害（即使实体不在场也可以调用，例如地图上的事件）
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <returns>实际造成的伤害</returns>
        public int TakeDamage(int damage)
        {
            if (IsDead)
            {
                Debug.LogWarning("[PlayerData] 玩家已死亡，无法受到伤害");
                return 0;
            }

            if (damage <= 0)
            {
                Debug.LogWarning($"[PlayerData] 伤害值必须为正数，当前值：{damage}");
                return 0;
            }

            int actualDamage = Mathf.Min(currentHealth, damage);
            currentHealth -= actualDamage;

            if (currentHealth < 0)
            {
                currentHealth = 0;
            }

            Debug.Log($"[PlayerData] 受到 {actualDamage} 点伤害，当前生命值：{currentHealth}/{maxHealth}");

            return actualDamage;
        }

        /// <summary>
        /// 设置最大生命值
        /// </summary>
        /// <param name="newMaxHealth">新的最大生命值</param>
        public void SetMaxHealth(int newMaxHealth)
        {
            if (newMaxHealth <= 0)
            {
                Debug.LogWarning($"[PlayerData] 最大生命值必须为正数，当前值：{newMaxHealth}");
                return;
            }

            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            Debug.Log($"[PlayerData] 最大生命值设置为 {maxHealth}，当前生命值：{currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// 重置生命值（用于重新开始游戏等场景）
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            Debug.Log($"[PlayerData] 生命值已重置为 {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// 从实体同步数据到玩家数据（战斗结束时调用）
        /// </summary>
        /// <param name="entityHealth">实体生命值组件</param>
        public void SyncFromEntity(DamageSystem.HealthComponent entityHealth)
        {
            if (entityHealth == null)
            {
                Debug.LogWarning("[PlayerData] 无法从空实体同步数据");
                return;
            }

            currentHealth = entityHealth.CurrentHealth;
            maxHealth = entityHealth.MaxHealth;

            Debug.Log($"[PlayerData] 从实体同步数据：生命值 {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// 将数据应用到实体（战斗开始时调用）
        /// </summary>
        /// <param name="entityHealth">实体生命值组件</param>
        public void ApplyToEntity(DamageSystem.HealthComponent entityHealth)
        {
            if (entityHealth == null)
            {
                Debug.LogWarning("[PlayerData] 无法将数据应用到空实体");
                return;
            }

            // 设置最大生命值
            entityHealth.SetMaxHealth(maxHealth);
            
            // 直接设置当前生命值
            entityHealth.SetCurrentHealth(currentHealth);

            Debug.Log($"[PlayerData] 将数据应用到实体：生命值 {entityHealth.CurrentHealth}/{entityHealth.MaxHealth}");
        }
    }
}

