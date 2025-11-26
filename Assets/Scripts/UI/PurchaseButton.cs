using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CurrencySystem;
using CardSystem;
using SquadSystem;

namespace UI
{
    /// <summary>
    /// 购买按钮组件
    /// 支持购买波牌和成员，自动处理金币扣除和物品添加
    /// </summary>
    public class PurchaseButton : MonoBehaviour
    {
        [Header("物品信息")]
        [Tooltip("物品ID（波牌ID或成员ID）")]
        [SerializeField] private string itemId;

        [Tooltip("物品类型")]
        [SerializeField] private ItemType itemType = ItemType.WaveCard;

        [Header("价格设置")]
        [Tooltip("价格（如果为-1，则使用物品数据中的默认价格）")]
        [SerializeField] private int price = -1;

        [Header("系统引用")]
        [Tooltip("金币系统（如果为空，会自动查找）")]
        [SerializeField] private CoinSystem coinSystem;

        [Tooltip("卡牌系统（购买波牌时需要，如果为空，会自动查找）")]
        [SerializeField] private CardSystem.CardSystem cardSystem;

        [Tooltip("小队管理器（购买成员时需要，如果为空，会自动查找）")]
        [SerializeField] private SquadManager squadManager;

        [Tooltip("波牌数据注册表（用于获取波牌价格，如果为空，会尝试从卡牌Prefab的WaveCardComponent获取）")]
        [SerializeField] private WaveSystem.WaveCardDataRegistry waveCardDataRegistry;

        [Header("UI组件")]
        [Tooltip("按钮组件（如果为空，会自动获取）")]
        [SerializeField] private Button button;

        [Header("事件")]
        [Tooltip("购买成功时触发（物品ID）")]
        [SerializeField] private UnityEngine.Events.UnityEvent<string> onPurchaseSuccess = new UnityEngine.Events.UnityEvent<string>();

        /// <summary>
        /// 购买成功事件
        /// </summary>
        public UnityEngine.Events.UnityEvent<string> OnPurchaseSuccess => onPurchaseSuccess;

        /// <summary>
        /// 物品类型枚举
        /// </summary>
        public enum ItemType
        {
            WaveCard,  // 波牌
            Member     // 成员
        }

        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId
        {
            get => itemId;
            set
            {
                itemId = value;
                UpdatePrice();
            }
        }

        /// <summary>
        /// 物品类型
        /// </summary>
        public ItemType Type
        {
            get => itemType;
            set => itemType = value;
        }

        /// <summary>
        /// 价格（只读）
        /// </summary>
        public int Price { get; private set; }

        private void Awake()
        {
            // 自动获取按钮组件
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            // 确保按钮可交互
            if (button != null)
            {
                button.interactable = true;
            }

            // 自动查找系统引用
            if (coinSystem == null)
            {
                coinSystem = FindObjectOfType<CoinSystem>();
            }

            if (cardSystem == null)
            {
                cardSystem = FindObjectOfType<CardSystem.CardSystem>();
            }

            if (squadManager == null)
            {
                squadManager = FindObjectOfType<SquadManager>();
            }

            // 订阅按钮点击事件
            if (button != null)
            {
                button.onClick.RemoveAllListeners(); // 清除之前的监听器，避免重复订阅
                button.onClick.AddListener(OnButtonClicked);
                Debug.Log($"[PurchaseButton] 按钮点击事件已订阅，按钮可交互: {button.interactable}");
            }
            else
            {
                Debug.LogWarning("[PurchaseButton] 未找到Button组件，无法订阅点击事件");
            }

            // 更新价格显示
            UpdatePrice();
        }

        /// <summary>
        /// 初始化按钮（设置物品ID和类型）
        /// </summary>
        /// <param name="id">物品ID</param>
        /// <param name="type">物品类型</param>
        /// <param name="customPrice">自定义价格（-1表示使用默认价格）</param>
        public void Initialize(string id, ItemType type, int customPrice = -1)
        {
            itemId = id;
            itemType = type;
            price = customPrice;
            UpdatePrice();

            // 确保按钮组件已获取并可交互
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.interactable = true;
                // 确保点击事件已订阅（重新订阅以确保正确）
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
                Debug.Log($"[PurchaseButton] Initialize完成: itemId={id}, type={type}, price={Price}, interactable={button.interactable}");
            }
            else
            {
                Debug.LogError("[PurchaseButton] Initialize失败: 未找到Button组件");
            }
        }

        /// <summary>
        /// 更新价格（从物品数据获取或使用自定义价格）
        /// </summary>
        private void UpdatePrice()
        {
            if (price >= 0)
            {
                // 使用自定义价格
                Price = price;
            }
            else
            {
                // 从物品数据获取默认价格
                Price = GetDefaultPrice();
            }

            Debug.Log($"[PurchaseButton] UpdatePrice: price字段={price}, 最终Price={Price}, itemId={itemId}, itemType={itemType}");
            // 价格已更新（不再显示价格文本）
        }

        /// <summary>
        /// 获取物品的默认价格
        /// </summary>
        private int GetDefaultPrice()
        {
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[PurchaseButton] GetDefaultPrice: itemId为空，返回0");
                return 0;
            }

            Debug.Log($"[PurchaseButton] GetDefaultPrice: 开始获取价格，itemId={itemId}, itemType={itemType}");

            switch (itemType)
            {
                case ItemType.WaveCard:
                    // 方法1: 尝试从卡牌Prefab的WaveCardComponent获取价格
                    if (cardSystem != null)
                    {
                        CardSystem.CardPrefabRegistry cardPrefabRegistry = cardSystem.GetCardRegistry();
                        if (cardPrefabRegistry != null)
                        {
                            GameObject cardPrefab = cardPrefabRegistry.GetCardPrefab(itemId);
                            if (cardPrefab != null)
                            {
                                WaveSystem.WaveCardComponent cardComponent = cardPrefab.GetComponent<WaveSystem.WaveCardComponent>();
                                if (cardComponent != null)
                                {
                                    int cardPrice = cardComponent.CardPrice;
                                    Debug.Log($"[PurchaseButton] GetDefaultPrice: 从WaveCardComponent获取到价格 {cardPrice}");
                                    return cardPrice;
                                }
                            }
                        }
                    }

                    // 方法2: 尝试从WaveCardDataRegistry获取价格
                    WaveSystem.WaveCardDataRegistry cardRegistry = waveCardDataRegistry;
                    if (cardRegistry == null)
                    {
                        // 尝试从场景中查找（虽然ScriptableObject通常不能这样查找，但有些情况下可能有MonoBehaviour持有引用）
                        // 这里我们跳过，因为ScriptableObject不能通过FindObjectOfType查找
                    }

                    if (cardRegistry != null)
                    {
                        var cardData = cardRegistry.GetWaveCardData(itemId);
                        if (cardData != null)
                        {
                            int cardPrice = cardData.CardPrice;
                            Debug.Log($"[PurchaseButton] GetDefaultPrice: 从WaveCardDataRegistry获取到价格 {cardPrice}");
                            return cardPrice;
                        }
                        else
                        {
                            Debug.LogWarning($"[PurchaseButton] GetDefaultPrice: WaveCardDataRegistry中未找到波牌ID '{itemId}' 的数据");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[PurchaseButton] GetDefaultPrice: WaveCardDataRegistry未设置，且无法从WaveCardComponent获取价格");
                    }
                    break;

                case ItemType.Member:
                    // 从成员注册表获取价格
                    if (squadManager != null && squadManager.SquadData != null)
                    {
                        var memberDataRegistry = FindObjectOfType<MemberDataRegistry>();
                        if (memberDataRegistry != null)
                        {
                            var memberData = memberDataRegistry.GetMemberData(itemId);
                            if (memberData != null)
                            {
                                int hireCost = memberData.HireCost;
                                Debug.Log($"[PurchaseButton] GetDefaultPrice: 从MemberDataRegistry获取到价格 {hireCost}");
                                return hireCost;
                            }
                            else
                            {
                                Debug.LogWarning($"[PurchaseButton] GetDefaultPrice: 未找到成员ID '{itemId}' 的数据");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[PurchaseButton] GetDefaultPrice: MemberDataRegistry未找到");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[PurchaseButton] GetDefaultPrice: SquadManager或SquadData未找到");
                    }
                    break;
            }

            Debug.LogWarning($"[PurchaseButton] GetDefaultPrice: 无法获取价格，返回0");
            return 0;
        }

        /// <summary>
        /// 按钮点击事件处理
        /// </summary>
        private void OnButtonClicked()
        {
            Debug.Log($"[PurchaseButton] OnButtonClicked 被调用: itemId={itemId}, type={itemType}, price={Price}");

            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("[PurchaseButton] 物品ID为空，无法购买");
                return;
            }

            if (coinSystem == null)
            {
                Debug.LogError("[PurchaseButton] 金币系统未找到，无法购买");
                return;
            }

            // 如果价格为0，直接免费购买，不需要检查金币
            if (Price > 0)
            {
                // 检查金币是否足够
                if (!coinSystem.HasEnoughCoins(Price))
                {
                    Debug.LogWarning($"[PurchaseButton] 金币不足，无法购买物品 '{itemId}'（需要 {Price} 金币，当前：{coinSystem.CurrentCoins}）");
                    return;
                }

                // 尝试扣除金币
                if (!coinSystem.TrySpendCoins(Price))
                {
                    Debug.LogWarning($"[PurchaseButton] 扣除金币失败，无法购买物品 '{itemId}'");
                    return;
                }
            }
            else
            {
                Debug.Log($"[PurchaseButton] 物品 '{itemId}' 价格为0，免费购买");
            }

            // 根据物品类型添加到对应数据
            bool success = false;
            switch (itemType)
            {
                case ItemType.WaveCard:
                    success = AddWaveCardToDeck(itemId);
                    break;

                case ItemType.Member:
                    success = AddMemberToSquad(itemId);
                    break;
            }

            if (success)
            {
                Debug.Log($"[PurchaseButton] 成功购买物品 '{itemId}'（类型：{itemType}，价格：{Price}）");
                // 触发购买成功事件
                onPurchaseSuccess?.Invoke(itemId);
            }
            else
            {
                // 购买失败，退还金币（如果价格大于0）
                if (Price > 0)
                {
                    coinSystem.AddCoins(Price);
                    Debug.LogError($"[PurchaseButton] 购买物品 '{itemId}' 失败，已退还 {Price} 金币");
                }
                else
                {
                    Debug.LogError($"[PurchaseButton] 购买物品 '{itemId}' 失败（价格为0，无需退还金币）");
                }
            }
        }

        /// <summary>
        /// 添加波牌到牌堆
        /// </summary>
        private bool AddWaveCardToDeck(string cardId)
        {
            if (cardSystem == null)
            {
                Debug.LogError("[PurchaseButton] 卡牌系统未找到，无法添加波牌");
                return false;
            }

            // 优先从卡牌Prefab注册表获取（通过CardSystem获取，因为CardPrefabRegistry是ScriptableObject）
            GameObject cardPrefab = null;
            CardPrefabRegistry cardPrefabRegistry = null;
            
            // 从CardSystem获取注册表（因为CardPrefabRegistry是ScriptableObject，不能通过FindObjectOfType查找）
            if (cardSystem != null)
            {
                cardPrefabRegistry = cardSystem.GetCardRegistry();
                if (cardPrefabRegistry != null)
                {
                    Debug.Log($"[PurchaseButton] 从CardSystem获取到CardPrefabRegistry: {cardPrefabRegistry.name}");
                }
                else
                {
                    Debug.LogWarning("[PurchaseButton] CardSystem存在，但GetCardRegistry()返回null，请检查CardSystem的cardRegistry字段是否已设置");
                }
            }
            else
            {
                Debug.LogWarning("[PurchaseButton] CardSystem为null，无法获取CardPrefabRegistry");
            }
            
            if (cardPrefabRegistry != null)
            {
                // 先检查注册表中是否有这个ID
                List<string> allIds = cardPrefabRegistry.GetAllCardIds();
                Debug.Log($"[PurchaseButton] CardPrefabRegistry包含 {allIds.Count} 个卡牌ID: {string.Join(", ", allIds)}");
                
                cardPrefab = cardPrefabRegistry.GetCardPrefab(cardId);
                if (cardPrefab != null)
                {
                    Debug.Log($"[PurchaseButton] 从CardSystem的CardPrefabRegistry获取到波牌Prefab: {cardId} -> {cardPrefab.name}");
                }
                else
                {
                    Debug.LogWarning($"[PurchaseButton] CardPrefabRegistry中未找到ID '{cardId}' 的Prefab（注册表中的ID列表已在上方显示）");
                }
            }

            // 如果卡牌Prefab注册表没有，尝试从波牌数据注册表获取
            if (cardPrefab == null)
            {
                WaveSystem.WaveCardDataRegistry cardDataRegistry = FindObjectOfType<WaveSystem.WaveCardDataRegistry>();
                if (cardDataRegistry != null)
                {
                    var cardData = cardDataRegistry.GetWaveCardData(cardId);
                    if (cardData != null && cardData.CardPrefab != null)
                    {
                        cardPrefab = cardData.CardPrefab;
                        Debug.Log($"[PurchaseButton] 从WaveCardDataRegistry获取到波牌Prefab: {cardId}");
                    }
                }
            }

            if (cardPrefab == null)
            {
                Debug.LogError($"[PurchaseButton] 未找到波牌ID '{cardId}' 对应的Prefab（已检查CardPrefabRegistry和WaveCardDataRegistry）");
                return false;
            }

            // 添加到牌堆
            bool success = cardSystem.AddCardToDeckAndDrawPile(cardPrefab, shuffle: true);
            if (success)
            {
                Debug.Log($"[PurchaseButton] 波牌 '{cardId}' 已添加到牌堆");
            }

            return success;
        }

        /// <summary>
        /// 添加成员到小队
        /// </summary>
        private bool AddMemberToSquad(string memberId)
        {
            if (squadManager == null)
            {
                Debug.LogError("[PurchaseButton] 小队管理器未找到，无法添加成员");
                return false;
            }

            bool success = squadManager.AddMember(memberId);
            if (success)
            {
                Debug.Log($"[PurchaseButton] 成员 '{memberId}' 已添加到小队");
            }

            return success;
        }


        private void OnDestroy()
        {
            // 取消订阅按钮事件
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}

