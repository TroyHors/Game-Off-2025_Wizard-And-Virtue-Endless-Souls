using UnityEngine;
using UnityEngine.Events;
using StatusSystem;

namespace DamageSystem
{
    /// <summary>
    /// 生命组件
    /// 统一的生命组件，用于玩家和敌人
    /// 处理回血、扣血、死亡、护盾
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        [Header("生命值设置")]
        [Tooltip("最大生命值")]
        [SerializeField] private int maxHealth = 100;

        [Tooltip("当前生命值（支持小数点，内部存储为float）")]
        [SerializeField] private float currentHealth;

        [Header("护盾设置")]
        [Tooltip("当前护盾值（独立于血量计算）")]
        [SerializeField] private int currentShield = 0;

        [Header("事件")]
        [Tooltip("生命值变化时触发（当前生命值，最大生命值）")]
        [SerializeField] private UnityEvent<int, int> onHealthChanged = new UnityEvent<int, int>();

        [Tooltip("护盾值变化时触发（当前护盾值）")]
        [SerializeField] private UnityEvent<int> onShieldChanged = new UnityEvent<int>();

        [Tooltip("受到伤害时触发（伤害值，剩余生命值，剩余护盾值）")]
        [SerializeField] private UnityEvent<int, int, int> onDamageTaken = new UnityEvent<int, int, int>();

        [Tooltip("回血时触发（回复值，当前生命值）")]
        [SerializeField] private UnityEvent<int, int> onHealed = new UnityEvent<int, int>();

        [Tooltip("死亡时触发")]
        [SerializeField] private UnityEvent onDeath = new UnityEvent();

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHealth => maxHealth;

        /// <summary>
        /// 当前生命值（整数，用于显示）
        /// </summary>
        public int CurrentHealth => Mathf.RoundToInt(currentHealth);

        /// <summary>
        /// 当前生命值（浮点数，精确值）
        /// </summary>
        public float CurrentHealthFloat => currentHealth;

        /// <summary>
        /// 当前护盾值
        /// </summary>
        public int CurrentShield => currentShield;

        /// <summary>
        /// 是否已死亡
        /// </summary>
        public bool IsDead => currentHealth <= 0f;

        /// <summary>
        /// 生命值变化事件
        /// </summary>
        public UnityEvent<int, int> OnHealthChanged => onHealthChanged;

        /// <summary>
        /// 护盾值变化事件
        /// </summary>
        public UnityEvent<int> OnShieldChanged => onShieldChanged;

        /// <summary>
        /// 受到伤害事件
        /// </summary>
        public UnityEvent<int, int, int> OnDamageTaken => onDamageTaken;

        /// <summary>
        /// 回血事件
        /// </summary>
        public UnityEvent<int, int> OnHealed => onHealed;

        /// <summary>
        /// 死亡事件
        /// </summary>
        public UnityEvent OnDeath => onDeath;

        private void Awake()
        {
            // 初始化时，如果当前生命值未设置，则设置为最大生命值
            if (currentHealth <= 0f)
            {
                currentHealth = (float)maxHealth;
            }
            Debug.Log($"[HealthComponent] {gameObject.name} Awake: currentHealth = {currentHealth:F2}, maxHealth = {maxHealth}");
        }

        /// <summary>
        /// 受到伤害
        /// 先应用状态效果修正，再扣除护盾，护盾不足时扣除生命值
        /// </summary>
        /// <param name="damage">伤害值（必须为正数，支持小数点）</param>
        /// <returns>实际造成的伤害（扣除护盾后的生命值伤害）</returns>
        public float TakeDamage(float damage)
        {
            if (IsDead)
            {
                Debug.LogWarning($"[HealthComponent] {gameObject.name} 已死亡，无法受到伤害");
                return 0f;
            }

            if (damage <= 0)
            {
                Debug.LogWarning($"[HealthComponent] {gameObject.name} 伤害值必须为正数，当前值：{damage}");
                return 0f;
            }

            // 应用状态效果修正（受到伤害减少/增加）
            StatusEffectManager statusManager = GetComponent<StatusEffectManager>();
            if (statusManager != null)
            {
                damage = statusManager.ApplyDamageTakenModifier(damage);
            }

            float actualHealthDamage = 0f;
            float remainingDamage = damage;

            // 先扣除护盾
            if (currentShield > 0)
            {
                float shieldDamage = Mathf.Min(currentShield, remainingDamage);
                currentShield -= Mathf.RoundToInt(shieldDamage);
                remainingDamage -= shieldDamage;

                if (currentShield < 0)
                {
                    currentShield = 0;
                }

                onShieldChanged?.Invoke(currentShield);
            }

            // 护盾不足时扣除生命值（直接使用float，保留小数精度）
            if (remainingDamage > 0)
            {
                float healthBefore = currentHealth;
                actualHealthDamage = Mathf.Min(currentHealth, remainingDamage);
                currentHealth -= actualHealthDamage;

                if (currentHealth < 0f)
                {
                    currentHealth = 0f;
                }

                Debug.Log($"[HealthComponent] {gameObject.name} 扣血: {healthBefore:F2} - {actualHealthDamage:F2} = {currentHealth:F2} (显示: {CurrentHealth})");

                onHealthChanged?.Invoke(CurrentHealth, maxHealth);
            }

            // 触发受到伤害事件（使用原始伤害值和当前生命值）
            onDamageTaken?.Invoke(Mathf.RoundToInt(damage), CurrentHealth, currentShield);

            // 检查是否死亡
            if (IsDead)
            {
                HandleDeath();
            }

            return actualHealthDamage;
        }

        /// <summary>
        /// 回复生命值
        /// </summary>
        /// <param name="healAmount">回复值（必须为正数）</param>
        /// <returns>实际回复的生命值</returns>
        public int Heal(int healAmount)
        {
            if (IsDead)
            {
                Debug.LogWarning($"[HealthComponent] {gameObject.name} 已死亡，无法回复生命值");
                return 0;
            }

            if (healAmount <= 0)
            {
                Debug.LogWarning($"[HealthComponent] {gameObject.name} 回复值必须为正数，当前值：{healAmount}");
                return 0;
            }

            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + (float)healAmount, (float)maxHealth);
            int actualHeal = CurrentHealth - Mathf.RoundToInt(oldHealth);

            if (actualHeal > 0)
            {
                onHealthChanged?.Invoke(CurrentHealth, maxHealth);
                onHealed?.Invoke(actualHeal, CurrentHealth);
            }

            return actualHeal;
        }

        /// <summary>
        /// 增加护盾值
        /// </summary>
        /// <param name="shieldAmount">护盾增加值（必须为正数）</param>
        /// <returns>实际增加的护盾值</returns>
        public int AddShield(int shieldAmount)
        {
            if (IsDead)
            {
                Debug.LogWarning($"[HealthComponent] {gameObject.name} 已死亡，无法增加护盾");
                return 0;
            }

            if (shieldAmount <= 0)
            {
                Debug.LogWarning($"[HealthComponent] {gameObject.name} 护盾增加值必须为正数，当前值：{shieldAmount}");
                return 0;
            }

            currentShield += shieldAmount;
            onShieldChanged?.Invoke(currentShield);

            return shieldAmount;
        }

        /// <summary>
        /// 设置最大生命值（并相应调整当前生命值）
        /// </summary>
        /// <param name="newMaxHealth">新的最大生命值</param>
        public void SetMaxHealth(int newMaxHealth)
        {
            if (newMaxHealth <= 0)
            {
                Debug.LogWarning($"[HealthComponent] {gameObject.name} 最大生命值必须为正数，当前值：{newMaxHealth}");
                return;
            }

            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, (float)maxHealth);
            onHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        /// <summary>
        /// 处理死亡
        /// </summary>
        private void HandleDeath()
        {
            Debug.Log($"[HealthComponent] {gameObject.name} 死亡");
            onDeath?.Invoke();
        }

        /// <summary>
        /// 重置生命值和护盾（用于重新开始游戏等场景）
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = (float)maxHealth;
            currentShield = 0;
            onHealthChanged?.Invoke(CurrentHealth, maxHealth);
            onShieldChanged?.Invoke(currentShield);
        }

        /// <summary>
        /// 直接设置当前生命值（用于从数据同步到实体）
        /// </summary>
        /// <param name="newHealth">新的当前生命值</param>
        public void SetCurrentHealth(int newHealth)
        {
            currentHealth = Mathf.Clamp((float)newHealth, 0f, (float)maxHealth);
            onHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }
    }
}

