using System.Collections.Generic;
using UnityEngine;
using CurrencySystem;
using CardSystem;
using WaveSystem;
using UI;

namespace GameFlow
{
    /// <summary>
    /// 商店管理器
    /// 处理商店商品的展示和购买
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        [Header("商店设置")]
        [Tooltip("商店商品数量（随机选择的波牌数量）")]
        [SerializeField] private int shopItemCount = 3;

        [Header("系统引用")]
        [Tooltip("卡牌Prefab注册表（用于根据波牌ID获取Prefab，如果为空，会自动查找）")]
        [SerializeField] private CardPrefabRegistry cardPrefabRegistry;

        [Header("UI设置")]
        [Tooltip("商店容器（用于放置商品和按钮）")]
        [SerializeField] private Transform shopContainer;

        [Tooltip("购买按钮Prefab（用于生成购买按钮）")]
        [SerializeField] private GameObject purchaseButtonPrefab;

        [Tooltip("UI管理器（用于控制商店面板显示，如果为空，会自动查找）")]
        [SerializeField] private UIManager uiManager;

        /// <summary>
        /// 当前商店的商品实例列表（用于清理）
        /// </summary>
        private List<GameObject> currentShopInstances = new List<GameObject>();

        /// <summary>
        /// 当前商店的商品ID列表（用于其他组件获取）
        /// </summary>
        private List<string> currentShopItemIds = new List<string>();

        /// <summary>
        /// 当前商店的商品ID列表（只读，供其他组件获取）
        /// </summary>
        public IReadOnlyList<string> CurrentShopItemIds => currentShopItemIds;

        private void Awake()
        {
            // 自动查找系统引用
            if (cardPrefabRegistry == null)
            {
                // 尝试从CardSystem获取
                CardSystem.CardSystem cardSystem = FindObjectOfType<CardSystem.CardSystem>();
                if (cardSystem != null)
                {
                    cardPrefabRegistry = cardSystem.GetCardRegistry();
                }
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }

            // 如果没有设置容器，使用自身作为容器
            if (shopContainer == null)
            {
                shopContainer = transform;
            }
        }

        /// <summary>
        /// 刷新商店（随机选择新的商品）
        /// </summary>
        public void RefreshShop()
        {
            Debug.Log($"[ShopManager] 刷新商店，商品数量: {shopItemCount}");

            // 清理之前的商品实例
            ClearShop();

            // 确保系统引用已找到
            EnsureSystemReferences();

            // 随机选择商品
            GenerateShopItems();
        }

        /// <summary>
        /// 生成商店商品
        /// </summary>
        private void GenerateShopItems()
        {
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

            Debug.Log($"[ShopManager] 随机选择了 {selectedCardIds.Count} 个波牌ID作为商品: {string.Join(", ", selectedCardIds)}");

            // 更新当前商品ID列表
            currentShopItemIds.Clear();
            currentShopItemIds.AddRange(selectedCardIds);

            // 为每个选中的波牌生成实例和按钮
            int successCount = 0;
            foreach (string cardId in selectedCardIds)
            {
                Debug.Log($"[ShopManager] 开始为卡牌ID '{cardId}' 创建商店商品");
                bool success = CreateShopItem(cardId);
                if (success)
                {
                    successCount++;
                    Debug.Log($"[ShopManager] 卡牌ID '{cardId}' 创建成功");
                }
                else
                {
                    Debug.LogError($"[ShopManager] 卡牌ID '{cardId}' 创建失败");
                }
            }

            Debug.Log($"[ShopManager] 商店商品生成完成，成功生成 {successCount}/{selectedCardIds.Count} 个商品，当前实例列表数量: {currentShopInstances.Count}");
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

            cardInstance.name = $"ShopItem_{cardId}";
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

            // 初始化购买按钮（使用默认价格，不是0）
            PurchaseButton purchaseButton = buttonInstance.GetComponent<PurchaseButton>();
            if (purchaseButton != null)
            {
                // 价格为-1表示使用物品数据中的默认价格
                purchaseButton.Initialize(cardId, PurchaseButton.ItemType.WaveCard, customPrice: -1);
                // 订阅购买成功事件，购买后销毁实例和按钮
                purchaseButton.OnPurchaseSuccess.AddListener((string purchasedCardId) =>
                {
                    if (purchasedCardId == cardId)
                    {
                        Destroy(cardInstance);
                        Destroy(buttonInstance);
                        currentShopInstances.Remove(cardInstance);
                        currentShopInstances.Remove(buttonInstance);
                        currentShopItemIds.Remove(cardId);
                        Debug.Log($"[ShopManager] 波牌 '{purchasedCardId}' 已被购买，已销毁商店商品实例");
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
        /// 确保所有系统引用都已找到
        /// </summary>
        private void EnsureSystemReferences()
        {
            if (cardPrefabRegistry == null)
            {
                // 尝试从CardSystem获取
                CardSystem.CardSystem cardSystem = FindObjectOfType<CardSystem.CardSystem>();
                if (cardSystem != null)
                {
                    cardPrefabRegistry = cardSystem.GetCardRegistry();
                }
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }

            if (shopContainer == null)
            {
                shopContainer = transform;
                Debug.LogWarning("[ShopManager] 商店容器未设置，使用自身作为容器");
            }
        }

        /// <summary>
        /// 清理所有商店商品实例
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
        }

        /// <summary>
        /// 显示商店面板
        /// </summary>
        public void ShowShop()
        {
            if (uiManager != null)
            {
                uiManager.ShowShopPanel();
            }
        }

        /// <summary>
        /// 隐藏商店面板
        /// </summary>
        public void HideShop()
        {
            if (uiManager != null)
            {
                uiManager.HideShopPanel();
            }
        }

        /// <summary>
        /// 获取波牌ID（从商店商品实例中获取）
        /// </summary>
        /// <param name="cardInstance">波牌实例</param>
        /// <returns>波牌ID，如果无效则返回空字符串</returns>
        public string GetCardIdFromInstance(GameObject cardInstance)
        {
            if (cardInstance == null)
            {
                return string.Empty;
            }

            // 从实例名称中提取ID（格式：ShopItem_{cardId}）
            string name = cardInstance.name;
            if (name.StartsWith("ShopItem_"))
            {
                return name.Substring("ShopItem_".Length);
            }

            return string.Empty;
        }

        /// <summary>
        /// 根据索引获取商品ID
        /// </summary>
        /// <param name="index">商品索引（从0开始）</param>
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
        /// 获取商品数量
        /// </summary>
        /// <returns>当前商品数量</returns>
        public int GetShopItemCount()
        {
            return currentShopItemIds.Count;
        }
    }
}

