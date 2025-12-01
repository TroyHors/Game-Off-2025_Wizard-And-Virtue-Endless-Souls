using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DamageSystem;

namespace UI
{
    /// <summary>
    /// 血量条显示组件
    /// 用于显示玩家或敌人的血量
    /// 支持显示血量条、血量文本和护盾
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [Header("目标设置")]
        [Tooltip("生命组件（如果为空，会尝试通过Tag自动查找）")]
        [SerializeField] private HealthComponent healthComponent;

        [Tooltip("是否自动查找目标（通过Tag）")]
        [SerializeField] private bool autoFindTarget = true;

        [Tooltip("目标Tag（Player或Enemy，用于自动查找）")]
        [SerializeField] private string targetTag = "Player";

        [Header("UI组件")]
        [Tooltip("血量条Slider组件（必须设置，用于显示血量百分比）")]
        [SerializeField] private Slider healthBarSlider;

        [Tooltip("血量文本（TextMeshProUGUI组件，可选，显示当前血量/最大血量）")]
        [SerializeField] private TextMeshProUGUI healthText;

        [Tooltip("护盾文本（TextMeshProUGUI组件，可选，显示当前护盾值）")]
        [SerializeField] private TextMeshProUGUI shieldText;

        [Header("显示设置")]
        [Tooltip("是否显示血量文本")]
        [SerializeField] private bool showHealthText = true;

        [Tooltip("是否显示护盾文本")]
        [SerializeField] private bool showShieldText = true;

        [Tooltip("血量文本格式（{0}=当前血量，{1}=最大血量）")]
        [SerializeField] private string healthTextFormat = "{0}/{1}";

        [Tooltip("护盾文本格式（{0}=护盾值）")]
        [SerializeField] private string shieldTextFormat = "护盾: {0}";

        [Header("动态查找设置")]
        [Tooltip("查找间隔（秒），用于定期尝试查找动态生成的目标")]
        [SerializeField] private float findInterval = 0.1f;

        [Tooltip("最大查找次数（0表示无限次，直到找到为止）")]
        [SerializeField] private int maxFindAttempts = 0;

        /// <summary>
        /// 查找协程
        /// </summary>
        private Coroutine findCoroutine;

        private void Awake()
        {
            // 如果没有设置UI组件，尝试自动查找
            if (healthBarSlider == null)
            {
                healthBarSlider = GetComponentInChildren<Slider>();
            }

            if (healthText == null && showHealthText)
            {
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) healthText = texts[0];
            }

            if (shieldText == null && showShieldText)
            {
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 1) shieldText = texts[1];
            }

            // 设置血量条填充方向（根据Tag设置，即使还没找到目标）
            SetupHealthBarFillDirection();

            // 验证引用
            if (healthBarSlider == null)
            {
                Debug.LogError($"[HealthBar] {gameObject.name} 血量条Slider未设置，无法显示血量条");
            }
            else
            {
                // 确保Slider不可交互（只用于显示）
                healthBarSlider.interactable = false;
            }
        }

        private void Start()
        {
            // 如果还没有找到目标，开始定期查找
            if (healthComponent == null && autoFindTarget)
            {
                StartFindCoroutine();
            }
            else if (healthComponent != null)
            {
                // 如果已经有目标，直接订阅事件
                SubscribeToHealthChanges();
                UpdateHealthDisplay();
            }
        }

        private void OnEnable()
        {
            // 如果还没有找到目标，开始定期查找
            if (healthComponent == null && autoFindTarget)
            {
                StartFindCoroutine();
            }
            else if (healthComponent != null)
            {
                // 确保事件已订阅
                SubscribeToHealthChanges();
                // 更新显示
                UpdateHealthDisplay();
            }
        }

        private void OnDisable()
        {
            // 停止查找协程
            StopFindCoroutine();

            // 取消订阅事件
            UnsubscribeFromHealthChanges();
        }

        /// <summary>
        /// 设置血量条填充方向
        /// 敌人从右往左填充，玩家从左往右填充
        /// </summary>
        private void SetupHealthBarFillDirection()
        {
            if (healthBarSlider == null)
            {
                return;
            }

            // 根据目标Tag判断填充方向
            // Enemy: 从右往左 (Slider.Direction.RightToLeft)
            // Player: 从左往右 (Slider.Direction.LeftToRight)
            bool isEnemy = !string.IsNullOrEmpty(targetTag) && targetTag.Equals("Enemy", System.StringComparison.OrdinalIgnoreCase);
            healthBarSlider.direction = isEnemy ? Slider.Direction.RightToLeft : Slider.Direction.LeftToRight;
        }

        /// <summary>
        /// 开始查找协程
        /// </summary>
        private void StartFindCoroutine()
        {
            if (findCoroutine == null)
            {
                findCoroutine = StartCoroutine(FindTargetCoroutine());
            }
        }

        /// <summary>
        /// 停止查找协程
        /// </summary>
        private void StopFindCoroutine()
        {
            if (findCoroutine != null)
            {
                StopCoroutine(findCoroutine);
                findCoroutine = null;
            }
        }

        /// <summary>
        /// 定期查找目标的协程
        /// </summary>
        private IEnumerator FindTargetCoroutine()
        {
            int attempts = 0;

            while (healthComponent == null)
            {
                // 检查是否超过最大尝试次数
                if (maxFindAttempts > 0 && attempts >= maxFindAttempts)
                {
                    Debug.LogWarning($"[HealthBar] {gameObject.name} 已达到最大查找次数 ({maxFindAttempts})，停止查找");
                    break;
                }

                // 尝试查找目标
                FindHealthComponentByTag();

                // 如果找到了目标，停止协程
                if (healthComponent != null)
                {
                    // 订阅事件
                    SubscribeToHealthChanges();
                    
                    // 立即更新一次（获取当前状态）
                    UpdateHealthDisplay();
                    UpdateShieldDisplay();
                    
                    // 延迟一帧再更新一次，确保HealthComponent完全初始化（如果数据还没同步）
                    StartCoroutine(DelayedUpdateDisplay());
                    
                    Debug.Log($"[HealthBar] {gameObject.name} 成功找到目标，停止查找");
                    break;
                }

                attempts++;
                yield return new WaitForSeconds(findInterval);
            }

            findCoroutine = null;
        }

        /// <summary>
        /// 通过Tag查找HealthComponent
        /// </summary>
        /// <returns>是否成功找到目标</returns>
        private bool FindHealthComponentByTag()
        {
            if (string.IsNullOrEmpty(targetTag))
            {
                return false;
            }

            GameObject targetObject = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObject != null)
            {
                HealthComponent foundComponent = targetObject.GetComponent<HealthComponent>();
                if (foundComponent != null)
                {
                    healthComponent = foundComponent;
                    Debug.Log($"[HealthBar] {gameObject.name} 通过Tag '{targetTag}' 找到HealthComponent: {targetObject.name}");
                    // 找到目标后，重新设置填充方向（以防Tag变化）
                    SetupHealthBarFillDirection();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 订阅血量变化事件
        /// </summary>
        private void SubscribeToHealthChanges()
        {
            if (healthComponent != null)
            {
                // 先移除监听器，避免重复订阅
                healthComponent.OnHealthChanged.RemoveListener(OnHealthChanged);
                healthComponent.OnShieldChanged.RemoveListener(OnShieldChanged);

                // 添加监听器
                healthComponent.OnHealthChanged.AddListener(OnHealthChanged);
                healthComponent.OnShieldChanged.AddListener(OnShieldChanged);
            }
        }

        /// <summary>
        /// 取消订阅血量变化事件
        /// </summary>
        private void UnsubscribeFromHealthChanges()
        {
            if (healthComponent != null)
            {
                healthComponent.OnHealthChanged.RemoveListener(OnHealthChanged);
                healthComponent.OnShieldChanged.RemoveListener(OnShieldChanged);
            }
        }

        /// <summary>
        /// 血量变化事件处理
        /// </summary>
        /// <param name="currentHealth">当前血量</param>
        /// <param name="maxHealth">最大血量</param>
        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            UpdateHealthDisplay();
        }

        /// <summary>
        /// 护盾变化事件处理
        /// </summary>
        /// <param name="currentShield">当前护盾值</param>
        private void OnShieldChanged(int currentShield)
        {
            UpdateShieldDisplay();
        }

        /// <summary>
        /// 延迟更新显示的协程（确保HealthComponent完全初始化）
        /// </summary>
        private IEnumerator DelayedUpdateDisplay()
        {
            // 等待一帧，确保HealthComponent完全初始化
            yield return null;
            
            // 再次检查HealthComponent是否有效
            if (healthComponent != null)
            {
                UpdateHealthDisplay();
                UpdateShieldDisplay();
                
                // 添加调试日志
                int currentHealth = healthComponent.CurrentHealth;
                int maxHealth = healthComponent.MaxHealth;
                float sliderValue = healthBarSlider != null ? healthBarSlider.value : 0f;
                Debug.Log($"[HealthBar] {gameObject.name} 延迟更新显示: 血量={currentHealth}/{maxHealth}, sliderValue={sliderValue:F2}");
            }
        }

        /// <summary>
        /// 更新血量显示
        /// </summary>
        private void UpdateHealthDisplay()
        {
            if (healthComponent == null)
            {
                // 如果HealthComponent为空，显示空值
                if (healthBarSlider != null)
                {
                    healthBarSlider.value = 0f;
                }

                if (healthText != null && showHealthText)
                {
                    healthText.text = "0/0";
                }

                return;
            }

            int currentHealth = healthComponent.CurrentHealth;
            int maxHealth = healthComponent.MaxHealth;

            // 添加调试日志（仅在数据异常时）
            if (maxHealth <= 0)
            {
                Debug.LogWarning($"[HealthBar] {gameObject.name} 最大血量为0或负数: {maxHealth}，当前血量: {currentHealth}");
            }

            // 更新血量条
            if (healthBarSlider != null)
            {
                float oldValue = healthBarSlider.value;
                float healthPercentage = maxHealth > 0 ? (float)currentHealth / (float)maxHealth : 0f;
                float newValue = Mathf.Clamp01(healthPercentage);
                healthBarSlider.value = newValue;
                
                // 调试日志（仅在值变化时，避免日志过多）
                if (Mathf.Abs(oldValue - newValue) > 0.01f)
                {
                    Debug.Log($"[HealthBar] {gameObject.name} 更新血量条: {currentHealth}/{maxHealth} = {healthPercentage:F2} -> sliderValue={oldValue:F2}->{newValue:F2}");
                }
            }

            // 更新血量文本
            if (healthText != null && showHealthText)
            {
                healthText.text = string.Format(healthTextFormat, currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// 更新护盾显示
        /// </summary>
        private void UpdateShieldDisplay()
        {
            if (healthComponent == null)
            {
                if (shieldText != null && showShieldText)
                {
                    shieldText.text = string.Format(shieldTextFormat, 0);
                }
                return;
            }

            int currentShield = healthComponent.CurrentShield;

            // 更新护盾文本
            if (shieldText != null && showShieldText)
            {
                if (currentShield > 0)
                {
                    shieldText.text = string.Format(shieldTextFormat, currentShield);
                    shieldText.gameObject.SetActive(true);
                }
                else
                {
                    shieldText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 手动设置目标HealthComponent（供外部调用）
        /// </summary>
        /// <param name="targetHealthComponent">目标HealthComponent</param>
        public void SetTarget(HealthComponent targetHealthComponent)
        {
            // 取消旧目标的订阅
            UnsubscribeFromHealthChanges();

            // 设置新目标
            healthComponent = targetHealthComponent;

            // 根据目标对象的Tag判断填充方向
            if (targetHealthComponent != null)
            {
                GameObject targetObject = targetHealthComponent.gameObject;
                if (targetObject.CompareTag("Enemy"))
                {
                    targetTag = "Enemy";
                }
                else if (targetObject.CompareTag("Player"))
                {
                    targetTag = "Player";
                }
                SetupHealthBarFillDirection();
            }

            // 订阅新目标的事件
            SubscribeToHealthChanges();

            // 更新显示
            UpdateHealthDisplay();
            UpdateShieldDisplay();
        }

        /// <summary>
        /// 手动刷新目标（重新查找）
        /// </summary>
        public void RefreshTarget()
        {
            if (autoFindTarget)
            {
                // 取消旧目标的订阅
                UnsubscribeFromHealthChanges();

                // 重置目标
                healthComponent = null;

                // 重新开始查找
                if (findCoroutine == null)
                {
                    StartFindCoroutine();
                }
                else
                {
                    // 如果协程正在运行，直接尝试查找一次
                    if (FindHealthComponentByTag())
                    {
                        StopFindCoroutine();
                        SubscribeToHealthChanges();
                        UpdateHealthDisplay();
                        UpdateShieldDisplay();
                    }
                }
            }
        }
    }
}

