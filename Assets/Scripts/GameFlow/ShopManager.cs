using System.Collections.Generic;
using UnityEngine;
using CurrencySystem;
using CardSystem;
using UI;

namespace GameFlow
{
    /// <summary>
    /// 商店管理器
    /// 处理商店节点的商品展示和购买
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        [Header("商店设置")]
        [Tooltip("商店商品数量（随机选择的波牌数量）")]
        [SerializeField] private int shopItemCount = 3;

        [Header("系统引用")]
        [Tooltip("金币系统（如果为空，会自动查找）")]
        [SerializeField] private CoinSystem coinSystem;

        [Tooltip("卡牌系统（如果为空，会自动查找）")]
        [SerializeField] private CardSystem.CardSystem cardSystem;

        [Tooltip("卡牌Prefab注册表（用于根据波牌ID获取Prefab，如果为空，会自动查找）")]
        [SerializeField] private CardSystem.CardPrefabRegistry cardPrefabRegistry;

        [Header("UI设置")]
        [Tooltip("商店容器（用于放置商品和按钮，如果为空，会自动查找名为'ShopContainer'的子对象）")]
        [SerializeField] private Transform shopContainer;

        [Tooltip("购买按钮Prefab（用于生成购买按钮）")]
        [SerializeField] private GameObject purchaseButtonPrefab;

        [Tooltip("UI管理器（用于控制商店面板显示，如果为空，会自动查找）")]
        [SerializeField] private UIManager uiManager;

        [Tooltip("商店流程（用于结束流程，如果为空，会自动查找）")]
        [SerializeField] private ShopNodeFlow shopNodeFlow;

        /// <summary>
        /// 当前商店的商品实例列表（用于清理）
        /// </summary>
        private List<GameObject> currentShopInstances = new List<GameObject>();

        /// <summary>
        /// 当前商店的商品ID列表（用于外部获取）
        /// </summary>
        private List<string> currentShopItemIds = new List<string>();

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

            if (shopNodeFlow == null)
            {
                shopNodeFlow = GetComponent<ShopNodeFlow>();
            }

            if (shopNodeFlow == null)
            {
                shopNodeFlow = FindObjectOfType<ShopNodeFlow>();
            }

            // 如果没有设置容器，尝试查找名为'ShopContainer'的子对象
            if (shopContainer == null)
            {
                shopContainer = transform.Find("ShopContainer");
            }

            // 如果还是找不到，使用自身作为容器
            if (shopContainer == null)
            {
                shopContainer = transform;
                Debug.LogWarning("[ShopManager] 商店容器未设置，使用自身作为容器");
            }
        }

        /// <summary>
        /// 初始化商店（随机选择商品并生成）
        /// </summary>
        public void InitializeShop()
        {
            Debug.Log($"[ShopManager] 开始初始化商店，商品数量: {shopItemCount}");

            // 清理之前的商品
            ClearShop();

            // 确保所有系统引用都已找到
            EnsureSystemReferences();

            // 随机选择商品
            GenerateShopItems();

            // 显示商店面板
            if (uiManager != null)
            {
                uiManager.ShowShopPanel();
                Debug.Log("[ShopManager] 已显示商店面板");
            }
            else
            {
                Debug.LogWarning("[ShopManager] UI管理器未找到，无法显示商店面板");
            }
        }

        /// <summary>
        /// 确保所有系统引用都已找到
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
                // 尝试从CardSystem获取
                if (cardSystem != null)
                {
                    cardPrefabRegistry = cardSystem.GetCardRegistry();
                }
                if (cardPrefabRegistry == null)
                {
                    Debug.LogWarning("[ShopManager] 卡牌Prefab注册表未找到");
                }
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }

            // 如果没有设置容器，尝试查找名为'ShopContainer'的子对象
            if (shopContainer == null)
            {
                shopContainer = transform.Find("ShopContainer");
            }

            // 如果还是找不到，使用自身作为容器
            if (shopContainer == null)
            {
                shopContainer = transform;
                Debug.LogWarning("[ShopManager] 商店容器未设置，使用自身作为容器");
            }
        }

        /// <summary>
        /// 生成商店商品（随机选择波牌并生成实例和按钮）
        /// </summary>
        private void GenerateShopItems()
        {
            Debug.Log($"[ShopManager] 开始生成商店商品，数量: {shopItemCount}");

            if (cardPrefabRegistry == null)
            {
                Debug.LogError("[ShopManager] 卡牌Prefab注册表未找到，无法生成商店商品");
                return;
            }

            if (purchaseButtonPrefab == null)
            {
                Debug.LogError("[ShopManager] 购买按钮Prefab未设置，无法生成购买按钮");
                return;
            }

            if (shopContainer == null)
            {
                Debug.LogError("[ShopManager] 商店容器未设置，无法生成商品实例");
                return;
            }

            // 获取所有可用的卡牌ID
            List<string> allCardIds = cardPrefabRegistry.GetAllCardIds();
            if (allCardIds == null || allCardIds.Count == 0)
            {
                Debug.LogWarning("[ShopManager] 卡牌Prefab注册表为空，无法生成商店商品");
                return;
            }

            Debug.Log($"[ShopManager] 从注册表中获取到 {allCardIds.Count} 个可用卡牌ID");

            // 随机选择指定数量的波牌ID
            List<string> selectedCardIds = new List<string>();
            List<string> availableCardIds = new List<string>(allCardIds);

            for (int i = 0; i < shopItemCount && availableCardIds.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableCardIds.Count);
                selectedCardIds.Add(availableCardIds[randomIndex]);
                availableCardIds.RemoveAt(randomIndex);
            }

            Debug.Log($"[ShopManager] 随机选择了 {selectedCardIds.Count} 个波牌ID: {string.Join(", ", selectedCardIds)}");

            // 保存商品ID列表（供外部获取）
            currentShopItemIds = new List<string>(selectedCardIds);

            // 为每个选中的波牌生成实例和按钮
            foreach (string cardId in selectedCardIds)
            {
                CreateShopItem(cardId);
            }

            Debug.Log($"[ShopManager] 生成了 {selectedCardIds.Count} 个商店商品");
        }

        /// <summary>
        /// 创建单个商店商品（波牌实例和购买按钮）
        /// </summary>
        /// <returns>是否成功创建</returns>
        private bool CreateShopItem(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogError($"[ShopManager] CreateShopItem: 卡牌ID为空");
                return false;
            }

            Debug.Log($"[ShopManager] CreateShopItem 开始处理: {cardId}");

            // 检查 cardPrefabRegistry
            if (cardPrefabRegistry == null)
            {
                Debug.LogError($"[ShopManager] CreateShopItem: cardPrefabRegistry 为 null");
                return false;
            }

            // 从卡牌Prefab注册表获取波牌Prefab
            GameObject cardPrefab = cardPrefabRegistry.GetCardPrefab(cardId);
            if (cardPrefab == null)
            {
                Debug.LogWarning($"[ShopManager] CreateShopItem: 未找到波牌ID '{cardId}' 对应的Prefab");
                return false;
            }

            Debug.Log($"[ShopManager] CreateShopItem: 找到Prefab '{cardPrefab.name}'");

            // 检查商店容器
            if (shopContainer == null)
            {
                Debug.LogError($"[ShopManager] CreateShopItem: shopContainer 为 null");
                return false;
            }

            Debug.Log($"[ShopManager] CreateShopItem: 商店容器 '{shopContainer.name}', 激活: {shopContainer.gameObject.activeSelf}");

            // 生成波牌实例（使用从注册表获取的Prefab）
            GameObject cardInstance = Instantiate(cardPrefab, shopContainer);
            if (cardInstance == null)
            {
                Debug.LogError($"[ShopManager] CreateShopItem: Instantiate 返回 null");
                return false;
            }

            cardInstance.name = $"ShopCard_{cardId}";
            cardInstance.SetActive(true); // 确保实例是激活的

            Debug.Log($"[ShopManager] CreateShopItem: 波牌实例已创建 '{cardInstance.name}', 激活: {cardInstance.activeSelf}, 父对象: {(cardInstance.transform.parent != null ? cardInstance.transform.parent.name : "null")}");

            currentShopInstances.Add(cardInstance);
            Debug.Log($"[ShopManager] CreateShopItem: 已添加到实例列表，当前数量: {currentShopInstances.Count}");

            // 检查购买按钮Prefab
            if (purchaseButtonPrefab == null)
            {
                Debug.LogError($"[ShopManager] CreateShopItem: purchaseButtonPrefab 为 null");
                return false;
            }

            // 生成购买按钮（作为卡牌的子对象）
            GameObject buttonInstance = Instantiate(purchaseButtonPrefab, cardInstance.transform);
            if (buttonInstance == null)
            {
                Debug.LogError($"[ShopManager] CreateShopItem: 按钮实例化失败");
                return false;
            }

            buttonInstance.name = $"PurchaseButton_{cardId}";
            buttonInstance.SetActive(true); // 确保按钮是激活的

            Debug.Log($"[ShopManager] CreateShopItem: 按钮实例已创建 '{buttonInstance.name}', 激活: {buttonInstance.activeSelf}");

            // 初始化购买按钮（使用默认价格，从物品数据获取）
            PurchaseButton purchaseButton = buttonInstance.GetComponent<PurchaseButton>();
            if (purchaseButton != null)
            {
                purchaseButton.Initialize(cardId, PurchaseButton.ItemType.WaveCard);
                // 订阅购买成功事件，购买后销毁实例和按钮
                purchaseButton.OnPurchaseSuccess.AddListener((string purchasedCardId) =>
                {
                    if (purchasedCardId == cardId)
                    {
                        Destroy(cardInstance);
                        currentShopInstances.Remove(cardInstance);
                        currentShopItemIds.Remove(cardId);
                        Debug.Log($"[ShopManager] 波牌 '{purchasedCardId}' 已被购买，已销毁商品实例");
                    }
                });
                Debug.Log($"[ShopManager] CreateShopItem: 购买按钮初始化完成");
            }
            else
            {
                Debug.LogWarning($"[ShopManager] CreateShopItem: 购买按钮Prefab缺少 PurchaseButton 组件");
            }

            currentShopInstances.Add(buttonInstance);
            Debug.Log($"[ShopManager] CreateShopItem: 完成处理 '{cardId}', 最终实例列表数量: {currentShopInstances.Count}");
            return true;
        }

        /// <summary>
        /// 清理所有商店商品
        /// </summary>
        public void ClearShop()
        {
            foreach (GameObject instance in currentShopInstances)
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
            currentShopInstances.Clear();
            currentShopItemIds.Clear();

            // 隐藏商店面板
            if (uiManager != null)
            {
                uiManager.HideShopPanel();
            }
        }

        /// <summary>
        /// 获取当前商店的所有商品ID列表
        /// </summary>
        /// <returns>商品ID列表</returns>
        public List<string> GetShopItemIds()
        {
            return new List<string>(currentShopItemIds);
        }

        /// <summary>
        /// 获取指定索引的商品ID
        /// </summary>
        /// <param name="index">商品索引</param>
        /// <returns>商品ID，如果索引无效则返回空字符串</returns>
        public string GetShopItemId(int index)
        {
            if (index >= 0 && index < currentShopItemIds.Count)
            {
                return currentShopItemIds[index];
            }
            return string.Empty;
        }

        /// <summary>
        /// 从商品实例获取波牌ID
        /// </summary>
        /// <param name="cardInstance">波牌实例</param>
        /// <returns>波牌ID，如果无效则返回空字符串</returns>
        public string GetCardIdFromInstance(GameObject cardInstance)
        {
            if (cardInstance == null)
            {
                return string.Empty;
            }

            // 从实例名称中提取ID（格式：ShopCard_{cardId}）
            string name = cardInstance.name;
            if (name.StartsWith("ShopCard_"))
            {
                return name.Substring("ShopCard_".Length);
            }

            return string.Empty;
        }

        /// <summary>
        /// 设置商店容器（供ShopNodeFlow调用）
        /// </summary>
        /// <param name="container">商店容器</param>
        public void SetShopContainer(Transform container)
        {
            shopContainer = container;
            Debug.Log($"[ShopManager] 商店容器已设置: {(container != null ? container.name : "null")}");
        }

        /// <summary>
        /// 完成商店并结束流程（供按钮调用）
        /// 玩家通过场景上的按钮调用此方法，确认商店后结束商店流程
        /// </summary>
        public void FinishShopAndFlow()
        {
            Debug.Log("[ShopManager] 玩家确认商店，准备结束流程");

            // 清理商店
            ClearShop();

            // 确保找到 ShopNodeFlow（可能在运行时才创建）
            if (shopNodeFlow == null)
            {
                shopNodeFlow = FindObjectOfType<ShopNodeFlow>();
            }

            // 结束商店流程
            if (shopNodeFlow != null)
            {
                Debug.Log("[ShopManager] 找到 ShopNodeFlow，调用 FinishShopAndFlow()");
                shopNodeFlow.FinishShopAndFlow();
            }
            else
            {
                Debug.LogError("[ShopManager] 未找到 ShopNodeFlow，无法结束流程！请检查场景中是否有 ShopNodeFlow 组件");
            }
        }
    }
}

