using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 卡牌系统主控制器
    /// 协调卡组和牌堆管理器，处理回合抽牌等核心逻辑
    /// </summary>
    public class CardSystem : MonoBehaviour
    {
        [Header("卡组设置")]
        [Tooltip("局外卡组（传统方式：直接存储Prefab引用）")]
        [SerializeField] private CardDeck deck = new CardDeck();

        [Header("动态卡组设置")]
        [Tooltip("卡牌Prefab注册表（ID到Prefab的映射）")]
        [SerializeField] private CardPrefabRegistry cardRegistry;

        [Tooltip("卡组数据（运行时根据此数据动态构建卡组）")]
        [SerializeField] private CardDeckData deckData;

        [Header("牌堆管理器")]
        [Tooltip("牌堆管理器组件")]
        [SerializeField] private CardPileManager pileManager;

        [Header("抽牌设置")]
        [Tooltip("每回合默认抽牌数量")]
        [SerializeField] private int cardsPerTurn = 5;

        /// <summary>
        /// 卡组管理器
        /// </summary>
        public CardDeck Deck => deck;

        /// <summary>
        /// 牌堆管理器
        /// </summary>
        public CardPileManager PileManager => pileManager;

        /// <summary>
        /// 每回合抽牌数量
        /// </summary>
        public int CardsPerTurn
        {
            get => cardsPerTurn;
            set => cardsPerTurn = Mathf.Max(0, value);
        }

        private void Awake()
        {
            // 如果没有指定牌堆管理器，尝试从当前GameObject获取
            if (pileManager == null)
            {
                pileManager = GetComponent<CardPileManager>();
            }

            if (pileManager == null)
            {
                Debug.LogError("[CardSystem] 未找到CardPileManager组件，请在Inspector中指定或添加CardPileManager组件");
            }
        }

        /// <summary>
        /// 初始化游戏，将卡组洗入牌堆
        /// 如果设置了deckData，则使用动态卡组；否则使用传统的deck
        /// </summary>
        public void InitializeGame()
        {
            if (pileManager == null)
            {
                Debug.LogError("[CardSystem] 无法初始化游戏：CardPileManager未设置");
                return;
            }

            List<GameObject> deckCards;

            // 优先使用动态卡组数据
            if (deckData != null)
            {
                deckCards = BuildDeckFromData(deckData);
                if (deckCards == null || deckCards.Count == 0)
                {
                    Debug.LogWarning("[CardSystem] 无法从卡组数据构建卡组，请检查CardPrefabRegistry和CardDeckData设置");
                    return;
                }
                Debug.Log($"[CardSystem] 使用动态卡组数据初始化，卡组大小：{deckCards.Count}");
            }
            else
            {
                // 使用传统卡组
                if (deck.IsEmpty())
                {
                    Debug.LogWarning("[CardSystem] 卡组为空，无法初始化游戏");
                    return;
                }
                deckCards = deck.CreateCopy();
                Debug.Log($"[CardSystem] 使用传统卡组初始化，卡组大小：{deckCards.Count}");
            }

            // 将卡组洗入牌堆
            pileManager.InitializeDrawPile(deckCards);

            Debug.Log($"[CardSystem] 游戏初始化完成，卡组大小：{deckCards.Count}");
        }

        /// <summary>
        /// 根据卡组数据动态构建卡组（Prefab列表）
        /// </summary>
        /// <param name="data">卡组数据</param>
        /// <returns>构建的卡组Prefab列表</returns>
        public List<GameObject> BuildDeckFromData(CardDeckData data)
        {
            if (data == null)
            {
                Debug.LogError("[CardSystem] 卡组数据为空");
                return new List<GameObject>();
            }

            if (cardRegistry == null)
            {
                Debug.LogError("[CardSystem] 卡牌Prefab注册表未设置，无法构建卡组");
                return new List<GameObject>();
            }

            List<GameObject> deckCards = new List<GameObject>();

            foreach (var entry in data.cards)
            {
                if (entry == null || string.IsNullOrEmpty(entry.cardId))
                {
                    continue;
                }

                GameObject cardPrefab = cardRegistry.GetCardPrefab(entry.cardId);
                if (cardPrefab == null)
                {
                    Debug.LogWarning($"[CardSystem] 无法找到ID为 '{entry.cardId}' 的卡牌Prefab，已跳过");
                    continue;
                }

                // 根据数量添加多张卡牌
                for (int i = 0; i < entry.count; i++)
                {
                    deckCards.Add(cardPrefab);
                }
            }

            Debug.Log($"[CardSystem] 从卡组数据 '{data.deckName}' 构建了 {deckCards.Count} 张卡牌（{data.UniqueCardCount} 种）");
            return deckCards;
        }

        /// <summary>
        /// 使用指定的卡组数据初始化游戏
        /// </summary>
        /// <param name="data">卡组数据</param>
        public void InitializeGameWithDeckData(CardDeckData data)
        {
            deckData = data;
            InitializeGame();
        }

        /// <summary>
        /// 回合开始，从牌堆抽取默认数量的牌到手牌
        /// </summary>
        /// <returns>实际抽取的牌数</returns>
        public int StartTurn()
        {
            return StartTurn(cardsPerTurn);
        }

        /// <summary>
        /// 回合开始，从牌堆抽取指定数量的牌到手牌
        /// </summary>
        /// <param name="drawCount">要抽取的牌数</param>
        /// <returns>实际抽取的牌数</returns>
        public int StartTurn(int drawCount)
        {
            if (pileManager == null)
            {
                Debug.LogError("[CardSystem] 无法抽牌：CardPileManager未设置");
                return 0;
            }

            // 如果牌堆为空，将弃牌堆洗回牌堆
            if (pileManager.IsDrawPileEmpty())
            {
                Debug.Log("[CardSystem] 牌堆为空，将弃牌堆洗回牌堆");
                pileManager.ReshuffleDiscardPileToDrawPile();
            }

            int drawnCount = pileManager.DrawCards(drawCount);
            Debug.Log($"[CardSystem] 回合开始，抽取了 {drawnCount} 张牌（请求 {drawCount} 张）");

            return drawnCount;
        }

        /// <summary>
        /// 使用卡牌（将手牌移动到弃牌堆）
        /// </summary>
        /// <param name="card">要使用的卡牌</param>
        /// <returns>是否成功使用</returns>
        public bool UseCard(GameObject card)
        {
            if (pileManager == null)
            {
                Debug.LogError("[CardSystem] 无法使用卡牌：CardPileManager未设置");
                return false;
            }

            bool success = pileManager.DiscardCard(card);
            if (success)
            {
                Debug.Log("[CardSystem] 卡牌已使用并移至弃牌堆");
            }

            return success;
        }

        /// <summary>
        /// 使用指定索引的手牌
        /// </summary>
        /// <param name="handIndex">手牌索引</param>
        /// <returns>是否成功使用</returns>
        public bool UseCardAt(int handIndex)
        {
            if (pileManager == null)
            {
                Debug.LogError("[CardSystem] 无法使用卡牌：CardPileManager未设置");
                return false;
            }

            return pileManager.DiscardCardAt(handIndex);
        }

        /// <summary>
        /// 添加卡牌到卡组
        /// </summary>
        /// <param name="cardPrefab">卡牌Prefab</param>
        /// <returns>是否添加成功</returns>
        public bool AddCardToDeck(GameObject cardPrefab)
        {
            return deck.AddCard(cardPrefab);
        }

        /// <summary>
        /// 批量添加卡牌到卡组
        /// </summary>
        /// <param name="cardPrefabs">卡牌Prefab列表</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int AddCardsToDeck(List<GameObject> cardPrefabs)
        {
            return deck.AddCards(cardPrefabs);
        }

        /// <summary>
        /// 批量添加卡牌到卡组
        /// </summary>
        /// <param name="cardPrefabs">卡牌Prefab数组</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int AddCardsToDeck(GameObject[] cardPrefabs)
        {
            return deck.AddCards(cardPrefabs);
        }

        /// <summary>
        /// 从列表初始化卡组（清空现有卡组后添加）
        /// </summary>
        /// <param name="cardPrefabs">卡牌Prefab列表</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int InitializeDeckFromList(List<GameObject> cardPrefabs)
        {
            return deck.InitializeFromList(cardPrefabs);
        }

        /// <summary>
        /// 从数组初始化卡组（清空现有卡组后添加）
        /// </summary>
        /// <param name="cardPrefabs">卡牌Prefab数组</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int InitializeDeckFromArray(GameObject[] cardPrefabs)
        {
            return deck.InitializeFromArray(cardPrefabs);
        }

        /// <summary>
        /// 从卡组移除卡牌
        /// </summary>
        /// <param name="cardPrefab">卡牌Prefab</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveCardFromDeck(GameObject cardPrefab)
        {
            return deck.RemoveCard(cardPrefab);
        }

        /// <summary>
        /// 将卡牌直接添加到牌堆（游戏进行中可用）
        /// </summary>
        /// <param name="cardPrefab">卡牌Prefab</param>
        /// <param name="shuffle">是否在添加后洗牌</param>
        /// <returns>是否添加成功</returns>
        public bool AddCardToDrawPile(GameObject cardPrefab, bool shuffle = false)
        {
            if (pileManager == null)
            {
                Debug.LogError("[CardSystem] 无法添加卡牌到牌堆：CardPileManager未设置");
                return false;
            }

            bool success = pileManager.AddCardToDrawPile(cardPrefab, shuffle);
            if (success)
            {
                Debug.Log($"[CardSystem] 卡牌已添加到牌堆（洗牌：{shuffle}）");
            }

            return success;
        }

        /// <summary>
        /// 批量将卡牌直接添加到牌堆（游戏进行中可用）
        /// </summary>
        /// <param name="cardPrefabs">卡牌Prefab列表</param>
        /// <param name="shuffle">是否在添加后洗牌</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int AddCardsToDrawPile(List<GameObject> cardPrefabs, bool shuffle = false)
        {
            if (pileManager == null)
            {
                Debug.LogError("[CardSystem] 无法添加卡牌到牌堆：CardPileManager未设置");
                return 0;
            }

            int addedCount = pileManager.AddCardsToDrawPile(cardPrefabs, shuffle);
            if (addedCount > 0)
            {
                Debug.Log($"[CardSystem] {addedCount} 张卡牌已添加到牌堆（洗牌：{shuffle}）");
            }

            return addedCount;
        }

        /// <summary>
        /// 批量将卡牌直接添加到牌堆（游戏进行中可用）
        /// </summary>
        /// <param name="cardPrefabs">卡牌Prefab数组</param>
        /// <param name="shuffle">是否在添加后洗牌</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int AddCardsToDrawPile(GameObject[] cardPrefabs, bool shuffle = false)
        {
            if (pileManager == null)
            {
                Debug.LogError("[CardSystem] 无法添加卡牌到牌堆：CardPileManager未设置");
                return 0;
            }

            int addedCount = pileManager.AddCardsToDrawPile(cardPrefabs, shuffle);
            if (addedCount > 0)
            {
                Debug.Log($"[CardSystem] {addedCount} 张卡牌已添加到牌堆（洗牌：{shuffle}）");
            }

            return addedCount;
        }

        /// <summary>
        /// 将卡牌添加到卡组并同时添加到牌堆（游戏进行中可用）
        /// </summary>
        /// <param name="cardPrefab">卡牌Prefab</param>
        /// <param name="shuffle">是否在添加到牌堆后洗牌</param>
        /// <returns>是否添加成功</returns>
        public bool AddCardToDeckAndDrawPile(GameObject cardPrefab, bool shuffle = false)
        {
            bool deckSuccess = AddCardToDeck(cardPrefab);
            bool pileSuccess = AddCardToDrawPile(cardPrefab, shuffle);
            
            return deckSuccess && pileSuccess;
        }

        /// <summary>
        /// 批量将卡牌添加到卡组并同时添加到牌堆（游戏进行中可用）
        /// </summary>
        /// <param name="cardPrefabs">卡牌Prefab列表</param>
        /// <param name="shuffle">是否在添加到牌堆后洗牌</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int AddCardsToDeckAndDrawPile(List<GameObject> cardPrefabs, bool shuffle = false)
        {
            int deckCount = AddCardsToDeck(cardPrefabs);
            int pileCount = AddCardsToDrawPile(cardPrefabs, shuffle);
            
            return Mathf.Min(deckCount, pileCount);
        }

        /// <summary>
        /// 批量将卡牌添加到卡组并同时添加到牌堆（游戏进行中可用）
        /// </summary>
        /// <param name="cardPrefabs">卡牌Prefab数组</param>
        /// <param name="shuffle">是否在添加到牌堆后洗牌</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int AddCardsToDeckAndDrawPile(GameObject[] cardPrefabs, bool shuffle = false)
        {
            int deckCount = AddCardsToDeck(cardPrefabs);
            int pileCount = AddCardsToDrawPile(cardPrefabs, shuffle);
            
            return Mathf.Min(deckCount, pileCount);
        }

        /// <summary>
        /// 获取当前手牌数量
        /// </summary>
        public int GetHandCount()
        {
            return pileManager != null ? pileManager.HandCount : 0;
        }

        /// <summary>
        /// 获取牌堆剩余数量
        /// </summary>
        public int GetDrawPileCount()
        {
            return pileManager != null ? pileManager.DrawPileCount : 0;
        }

        /// <summary>
        /// 获取弃牌堆数量
        /// </summary>
        public int GetDiscardPileCount()
        {
            return pileManager != null ? pileManager.DiscardPileCount : 0;
        }

        /// <summary>
        /// 设置卡牌Prefab注册表
        /// </summary>
        /// <param name="registry">注册表</param>
        public void SetCardRegistry(CardPrefabRegistry registry)
        {
            cardRegistry = registry;
        }

        /// <summary>
        /// 设置卡组数据
        /// </summary>
        /// <param name="data">卡组数据</param>
        public void SetDeckData(CardDeckData data)
        {
            deckData = data;
        }

        /// <summary>
        /// 获取当前使用的卡组数据
        /// </summary>
        public CardDeckData GetDeckData()
        {
            return deckData;
        }

        /// <summary>
        /// 获取卡牌Prefab注册表
        /// </summary>
        public CardPrefabRegistry GetCardRegistry()
        {
            return cardRegistry;
        }
    }
}

