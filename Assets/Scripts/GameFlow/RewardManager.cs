using System.Collections.Generic;
using UnityEngine;
using CurrencySystem;
using CardSystem;
using WaveSystem;
using UI;

namespace GameFlow
{
    /// <summary>
    /// 奖励管理器
    /// 处理战斗结束后的奖励发放
    /// </summary>
    public class RewardManager : MonoBehaviour
    {
        [Header("金币奖励设置")]
        [Tooltip("战斗节点金币奖励范围（最小值，最大值）")]
        [SerializeField] private Vector2Int combatCoinRange = new Vector2Int(10, 20);

        [Tooltip("精英节点金币奖励范围（最小值，最大值）")]
        [SerializeField] private Vector2Int eliteCoinRange = new Vector2Int(20, 30);

        [Tooltip("Boss节点金币奖励范围（最小值，最大值）")]
        [SerializeField] private Vector2Int bossCoinRange = new Vector2Int(50, 100);

        [Header("精英节点卡牌奖励设置")]
        [Tooltip("精英节点奖励的波牌数量")]
        [SerializeField] private int eliteCardRewardCount = 3;

        [Header("系统引用")]
        [Tooltip("金币系统（如果为空，会自动查找）")]
        [SerializeField] private CoinSystem coinSystem;

        [Tooltip("卡牌系统（如果为空，会自动查找）")]
        [SerializeField] private CardSystem.CardSystem cardSystem;

        [Tooltip("卡牌Prefab注册表（用于根据波牌ID获取Prefab，如果为空，会自动查找）")]
        [SerializeField] private CardSystem.CardPrefabRegistry cardPrefabRegistry;

        [Header("UI设置")]
        [Tooltip("奖励容器（用于放置奖励物品和按钮）")]
        [SerializeField] private Transform rewardContainer;

        [Tooltip("购买按钮Prefab（用于生成购买按钮）")]
        [SerializeField] private GameObject purchaseButtonPrefab;

        [Tooltip("UI管理器（用于控制奖励面板显示，如果为空，会自动查找）")]
        [SerializeField] private UIManager uiManager;

        [Tooltip("战斗流程（用于结束流程，如果为空，会自动查找）")]
        [SerializeField] private CombatNodeFlow combatNodeFlow;

        /// <summary>
        /// 当前奖励的物品实例列表（用于清理）
        /// </summary>
        private List<GameObject> currentRewardInstances = new List<GameObject>();

        private void Awake()
        {
            // 自动查找系统引用
            if (coinSystem == null)
            {
                coinSystem = FindObjectOfType<CoinSystem>();
            }

            if (cardSystem == null)
            {
                cardSystem = FindObjectOfType<CardSystem.CardSystem>();
            }

            if (cardPrefabRegistry == null)
            {
                cardPrefabRegistry = FindObjectOfType<CardSystem.CardPrefabRegistry>();
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }

            if (combatNodeFlow == null)
            {
                combatNodeFlow = FindObjectOfType<CombatNodeFlow>();
            }

            // 如果没有设置容器，使用自身作为容器
            if (rewardContainer == null)
            {
                rewardContainer = transform;
            }
        }

        /// <summary>
        /// 发放奖励（根据节点类型）
        /// </summary>
        /// <param name="nodeType">节点类型</param>
        /// <param name="isBoss">是否为Boss节点</param>
        public void GiveReward(string nodeType, bool isBoss)
        {
            Debug.Log($"[RewardManager] 开始发放奖励: NodeType={nodeType}, IsBoss={isBoss}");

            // 确保找到 CombatNodeFlow（因为它是动态生成的，可能在 Awake 时还没有）
            if (combatNodeFlow == null)
            {
                combatNodeFlow = FindObjectOfType<CombatNodeFlow>();
                if (combatNodeFlow != null)
                {
                    Debug.Log("[RewardManager] 在 GiveReward 时找到了 CombatNodeFlow");
                }
                else
                {
                    Debug.LogWarning("[RewardManager] 未找到 CombatNodeFlow，结束流程时可能需要手动查找");
                }
            }

            // 确保所有系统引用都已找到
            EnsureSystemReferences();

            // 清理之前的奖励实例（不隐藏面板，因为我们要显示新的奖励）
            ClearRewardInstancesOnly();

            // 显示奖励面板
            if (uiManager != null)
            {
                uiManager.ShowRewardPanel();
                Debug.Log("[RewardManager] 已显示奖励面板");
            }
            else
            {
                Debug.LogWarning("[RewardManager] UI管理器未找到，无法显示奖励面板");
            }

            Debug.Log($"[RewardManager] 节点类型判断: nodeType='{nodeType}', isBoss={isBoss}");

            if (isBoss)
            {
                // Boss节点奖励
                Debug.Log("[RewardManager] 发放Boss节点奖励");
                GiveBossReward();
            }
            else if (nodeType == "Elite" || nodeType == "精英")
            {
                // 精英节点奖励（支持中英文）
                Debug.Log("[RewardManager] 发放精英节点奖励");
                GiveEliteReward();
            }
            else
            {
                // 普通战斗节点奖励
                Debug.Log("[RewardManager] 发放战斗节点奖励");
                GiveCombatReward();
            }
        }

        /// <summary>
        /// 确保所有系统引用都已找到（在发放奖励时调用）
        /// </summary>
        private void EnsureSystemReferences()
        {
            if (coinSystem == null)
            {
                coinSystem = FindObjectOfType<CoinSystem>();
            }

            if (cardSystem == null)
            {
                cardSystem = FindObjectOfType<CardSystem.CardSystem>();
            }

            if (cardPrefabRegistry == null)
            {
                cardPrefabRegistry = FindObjectOfType<CardSystem.CardPrefabRegistry>();
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }

            if (combatNodeFlow == null)
            {
                combatNodeFlow = FindObjectOfType<CombatNodeFlow>();
            }

            if (rewardContainer == null)
            {
                rewardContainer = transform;
                Debug.LogWarning("[RewardManager] 奖励容器未设置，使用自身作为容器");
            }
        }

        /// <summary>
        /// 发放战斗节点奖励
        /// </summary>
        private void GiveCombatReward()
        {
            int coins = Random.Range(combatCoinRange.x, combatCoinRange.y + 1);
            GiveCoins(coins);
            Debug.Log($"[RewardManager] 战斗节点奖励：{coins} 金币");
        }

        /// <summary>
        /// 发放精英节点奖励
        /// </summary>
        private void GiveEliteReward()
        {
            // 金币奖励
            int coins = Random.Range(eliteCoinRange.x, eliteCoinRange.y + 1);
            GiveCoins(coins);
            Debug.Log($"[RewardManager] 精英节点奖励：{coins} 金币");

            // 卡牌奖励
            Debug.Log($"[RewardManager] 准备发放卡牌奖励，数量: {eliteCardRewardCount}");
            Debug.Log($"[RewardManager] 当前状态检查 - cardPrefabRegistry: {(cardPrefabRegistry != null ? "已找到" : "NULL")}, purchaseButtonPrefab: {(purchaseButtonPrefab != null ? "已设置" : "NULL")}, rewardContainer: {(rewardContainer != null ? rewardContainer.name : "NULL")}");
            GiveCardRewards(eliteCardRewardCount);
            Debug.Log($"[RewardManager] GiveCardRewards 调用完成，当前实例数量: {currentRewardInstances.Count}");
        }

        /// <summary>
        /// 发放Boss节点奖励
        /// </summary>
        private void GiveBossReward()
        {
            int coins = Random.Range(bossCoinRange.x, bossCoinRange.y + 1);
            GiveCoins(coins);
            Debug.Log($"[RewardManager] Boss节点奖励：{coins} 金币");
        }

        /// <summary>
        /// 发放金币奖励
        /// </summary>
        private void GiveCoins(int amount)
        {
            if (coinSystem != null)
            {
                coinSystem.AddCoins(amount);
            }
            else
            {
                Debug.LogWarning("[RewardManager] 金币系统未找到，无法发放金币奖励");
            }
        }

        /// <summary>
        /// 发放卡牌奖励（生成波牌和购买按钮）
        /// </summary>
        private void GiveCardRewards(int count)
        {
            Debug.Log($"[RewardManager] GiveCardRewards 开始，数量: {count}");

            if (cardPrefabRegistry == null)
            {
                Debug.LogError("[RewardManager] 卡牌Prefab注册表未找到，无法发放卡牌奖励");
                return;
            }
            Debug.Log($"[RewardManager] cardPrefabRegistry 检查通过: {cardPrefabRegistry.name}");

            if (purchaseButtonPrefab == null)
            {
                Debug.LogError("[RewardManager] 购买按钮Prefab未设置，无法生成购买按钮");
                return;
            }
            Debug.Log($"[RewardManager] purchaseButtonPrefab 检查通过: {purchaseButtonPrefab.name}");

            if (rewardContainer == null)
            {
                Debug.LogError("[RewardManager] 奖励容器未设置，无法生成奖励实例");
                return;
            }
            Debug.Log($"[RewardManager] rewardContainer 检查通过: {rewardContainer.name}, 激活状态: {rewardContainer.gameObject.activeSelf}");

            // 获取所有可用的卡牌ID
            List<string> allCardIds = cardPrefabRegistry.GetAllCardIds();
            if (allCardIds == null || allCardIds.Count == 0)
            {
                Debug.LogWarning("[RewardManager] 卡牌Prefab注册表为空，无法发放卡牌奖励");
                return;
            }

            Debug.Log($"[RewardManager] 从注册表中获取到 {allCardIds.Count} 个可用卡牌ID: {string.Join(", ", allCardIds)}");

            // 随机选择指定数量的波牌ID
            List<string> selectedCardIds = new List<string>();
            List<string> availableCardIds = new List<string>(allCardIds);

            for (int i = 0; i < count && availableCardIds.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableCardIds.Count);
                selectedCardIds.Add(availableCardIds[randomIndex]);
                availableCardIds.RemoveAt(randomIndex);
            }

            Debug.Log($"[RewardManager] 随机选择了 {selectedCardIds.Count} 个波牌ID: {string.Join(", ", selectedCardIds)}");

            // 为每个选中的波牌生成实例和按钮
            int successCount = 0;
            foreach (string cardId in selectedCardIds)
            {
                Debug.Log($"[RewardManager] 开始为卡牌ID '{cardId}' 创建奖励实例");
                bool success = CreateCardReward(cardId);
                if (success)
                {
                    successCount++;
                    Debug.Log($"[RewardManager] 卡牌ID '{cardId}' 创建成功");
                }
                else
                {
                    Debug.LogError($"[RewardManager] 卡牌ID '{cardId}' 创建失败");
                }
            }

            Debug.Log($"[RewardManager] GiveCardRewards 完成，成功生成 {successCount}/{selectedCardIds.Count} 个波牌奖励，当前实例列表数量: {currentRewardInstances.Count}");
        }

        /// <summary>
        /// 创建单个波牌奖励（波牌实例和购买按钮）
        /// </summary>
        /// <returns>是否成功创建</returns>
        private bool CreateCardReward(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogError($"[RewardManager] CreateCardReward: 卡牌ID为空");
                return false;
            }

            Debug.Log($"[RewardManager] CreateCardReward 开始处理: {cardId}");

            // 检查 cardPrefabRegistry
            if (cardPrefabRegistry == null)
            {
                Debug.LogError($"[RewardManager] CreateCardReward: cardPrefabRegistry 为 null");
                return false;
            }

            // 从卡牌Prefab注册表获取波牌Prefab
            GameObject cardPrefab = cardPrefabRegistry.GetCardPrefab(cardId);
            if (cardPrefab == null)
            {
                Debug.LogWarning($"[RewardManager] CreateCardReward: 未找到波牌ID '{cardId}' 对应的Prefab");
                return false;
            }

            Debug.Log($"[RewardManager] CreateCardReward: 找到Prefab '{cardPrefab.name}'");

            // 检查奖励容器
            if (rewardContainer == null)
            {
                Debug.LogError($"[RewardManager] CreateCardReward: rewardContainer 为 null");
                return false;
            }

            Debug.Log($"[RewardManager] CreateCardReward: 奖励容器 '{rewardContainer.name}', 激活: {rewardContainer.gameObject.activeSelf}");

            // 生成波牌实例（使用从注册表获取的Prefab）
            GameObject cardInstance = Instantiate(cardPrefab, rewardContainer);
            if (cardInstance == null)
            {
                Debug.LogError($"[RewardManager] CreateCardReward: Instantiate 返回 null");
                return false;
            }

            cardInstance.name = $"RewardCard_{cardId}";
            cardInstance.SetActive(true); // 确保实例是激活的

            Debug.Log($"[RewardManager] CreateCardReward: 波牌实例已创建 '{cardInstance.name}', 激活: {cardInstance.activeSelf}, 父对象: {(cardInstance.transform.parent != null ? cardInstance.transform.parent.name : "null")}");

            currentRewardInstances.Add(cardInstance);
            Debug.Log($"[RewardManager] CreateCardReward: 已添加到实例列表，当前数量: {currentRewardInstances.Count}");

            // 检查购买按钮Prefab
            if (purchaseButtonPrefab == null)
            {
                Debug.LogError($"[RewardManager] CreateCardReward: purchaseButtonPrefab 为 null");
                return false;
            }

            // 生成购买按钮（作为卡牌的子对象）
            GameObject buttonInstance = Instantiate(purchaseButtonPrefab, cardInstance.transform);
            if (buttonInstance == null)
            {
                Debug.LogError($"[RewardManager] CreateCardReward: 按钮实例化失败");
                return false;
            }

            buttonInstance.name = $"PurchaseButton_{cardId}";
            buttonInstance.SetActive(true); // 确保按钮是激活的

            Debug.Log($"[RewardManager] CreateCardReward: 按钮实例已创建 '{buttonInstance.name}', 激活: {buttonInstance.activeSelf}, 父对象: {(buttonInstance.transform.parent != null ? buttonInstance.transform.parent.name : "null")}");

            // 初始化购买按钮（价格为0，因为是奖励，所有参数自动设置）
            PurchaseButton purchaseButton = buttonInstance.GetComponent<PurchaseButton>();
            if (purchaseButton != null)
            {
                purchaseButton.Initialize(cardId, PurchaseButton.ItemType.WaveCard, customPrice: 0);
                // 订阅购买成功事件，购买后销毁实例和按钮
                purchaseButton.OnPurchaseSuccess.AddListener((string purchasedCardId) =>
                {
                    if (purchasedCardId == cardId)
                    {
                        Destroy(cardInstance);
                        Destroy(buttonInstance);
                        currentRewardInstances.Remove(cardInstance);
                        currentRewardInstances.Remove(buttonInstance);
                        Debug.Log($"[RewardManager] 波牌 '{purchasedCardId}' 已被购买，已销毁奖励实例");
                        
                        // 检查是否所有奖励都已选择，如果是则隐藏奖励面板
                        if (currentRewardInstances.Count == 0)
                        {
                            ConfirmRewardSelection();
                        }
                    }
                });
                Debug.Log($"[RewardManager] CreateCardReward: 购买按钮初始化完成");
            }
            else
            {
                Debug.LogWarning($"[RewardManager] CreateCardReward: 购买按钮Prefab缺少 PurchaseButton 组件");
            }

            currentRewardInstances.Add(buttonInstance);
            Debug.Log($"[RewardManager] CreateCardReward: 完成处理 '{cardId}', 最终实例列表数量: {currentRewardInstances.Count}");
            return true;
        }

        /// <summary>
        /// 清理所有奖励实例（不隐藏面板）
        /// </summary>
        private void ClearRewardInstancesOnly()
        {
            foreach (GameObject instance in currentRewardInstances)
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
            currentRewardInstances.Clear();
        }

        /// <summary>
        /// 清理所有奖励实例并隐藏面板
        /// </summary>
        public void ClearRewards()
        {
            ClearRewardInstancesOnly();

            // 隐藏奖励面板
            if (uiManager != null)
            {
                uiManager.HideRewardPanel();
            }
        }

        /// <summary>
        /// 确认奖励选择（玩家选择完奖励后调用，隐藏奖励面板）
        /// </summary>
        public void ConfirmRewardSelection()
        {
            // 隐藏奖励面板
            if (uiManager != null)
            {
                uiManager.HideRewardPanel();
            }
        }

        /// <summary>
        /// 完成奖励并结束流程（供按钮调用）
        /// 玩家通过场景上的按钮调用此方法，确认奖励后结束战斗流程
        /// </summary>
        public void FinishRewardAndFlow()
        {
            Debug.Log("[RewardManager] 玩家确认奖励，准备结束流程");

            // 隐藏奖励面板
            ConfirmRewardSelection();

            // 确保找到 CombatNodeFlow（可能在运行时才创建）
            if (combatNodeFlow == null)
            {
                combatNodeFlow = FindObjectOfType<CombatNodeFlow>();
            }

            // 结束战斗流程
            if (combatNodeFlow != null)
            {
                Debug.Log("[RewardManager] 找到 CombatNodeFlow，调用 FinishRewardAndFlow()");
                combatNodeFlow.FinishRewardAndFlow();
            }
            else
            {
                Debug.LogError("[RewardManager] 未找到 CombatNodeFlow，无法结束流程！请检查场景中是否有 CombatNodeFlow 组件");
            }
        }

        /// <summary>
        /// 获取波牌ID（从奖励实例中获取）
        /// </summary>
        /// <param name="cardInstance">波牌实例</param>
        /// <returns>波牌ID，如果无效则返回空字符串</returns>
        public string GetCardIdFromInstance(GameObject cardInstance)
        {
            if (cardInstance == null)
            {
                return string.Empty;
            }

            // 从实例名称中提取ID（格式：RewardCard_{cardId}）
            string name = cardInstance.name;
            if (name.StartsWith("RewardCard_"))
            {
                return name.Substring("RewardCard_".Length);
            }

            return string.Empty;
        }
    }
}

