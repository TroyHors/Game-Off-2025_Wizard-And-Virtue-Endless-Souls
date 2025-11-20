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
        [Tooltip("局外卡组")]
        [SerializeField] private CardDeck deck = new CardDeck();

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
        /// </summary>
        public void InitializeGame()
        {
            if (pileManager == null)
            {
                Debug.LogError("[CardSystem] 无法初始化游戏：CardPileManager未设置");
                return;
            }

            if (deck.IsEmpty())
            {
                Debug.LogWarning("[CardSystem] 卡组为空，无法初始化游戏");
                return;
            }

            // 将卡组洗入牌堆
            List<GameObject> deckCopy = deck.CreateCopy();
            pileManager.InitializeDrawPile(deckCopy);

            Debug.Log($"[CardSystem] 游戏初始化完成，卡组大小：{deck.Count}");
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
    }
}

