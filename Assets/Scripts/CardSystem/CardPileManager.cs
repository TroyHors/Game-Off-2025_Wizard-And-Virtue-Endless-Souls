using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 手牌信息结构，存储卡牌实例和对应的Prefab引用
    /// </summary>
    [System.Serializable]
    public class HandCardInfo
    {
        public GameObject instance;  // 卡牌实例
        public GameObject prefab;    // 对应的Prefab引用

        public HandCardInfo(GameObject instance, GameObject prefab)
        {
            this.instance = instance;
            this.prefab = prefab;
        }
    }

    /// <summary>
    /// 牌堆管理器（局内）
    /// 管理牌堆、手牌和弃牌堆三个局内牌堆
    /// 牌堆和弃牌堆只存储数据（Prefab引用），只有手牌会生成实例
    /// </summary>
    public class CardPileManager : MonoBehaviour
    {
        [Header("牌堆设置")]
        [Tooltip("手牌 - 当前持有的牌（传统方式：直接作为子对象）")]
        [SerializeField] private Transform handContainer;

        [Tooltip("手牌槽位管理器（如果设置，将使用槽位系统进行拖动）")]
        [SerializeField] private HandSlotManager handSlotManager;

        /// <summary>
        /// 设置手牌槽位管理器
        /// </summary>
        /// <param name="manager">手牌槽位管理器</param>
        public void SetHandSlotManager(HandSlotManager manager)
        {
            handSlotManager = manager;
        }

        // 局内牌堆数据
        private List<GameObject> drawPile = new List<GameObject>();           // 牌堆（Prefab引用，不实例化）
        private List<HandCardInfo> hand = new List<HandCardInfo>();          // 手牌（实例）
        private List<GameObject> discardPile = new List<GameObject>();       // 弃牌堆（Prefab引用，不实例化）

        /// <summary>
        /// 牌堆中的卡牌数量
        /// </summary>
        public int DrawPileCount => drawPile.Count;

        /// <summary>
        /// 手牌数量
        /// </summary>
        public int HandCount => hand.Count;

        /// <summary>
        /// 弃牌堆中的卡牌数量
        /// </summary>
        public int DiscardPileCount => discardPile.Count;

        /// <summary>
        /// 获取手牌实例列表（只读）
        /// </summary>
        public IReadOnlyList<GameObject> Hand
        {
            get
            {
                List<GameObject> instances = new List<GameObject>();
                foreach (var cardInfo in hand)
                {
                    instances.Add(cardInfo.instance);
                }
                return instances;
            }
        }

        /// <summary>
        /// 初始化牌堆，将卡组洗牌后放入牌堆（只存储Prefab引用，不实例化）
        /// </summary>
        /// <param name="deckCards">卡组中的卡牌Prefab列表</param>
        public void InitializeDrawPile(List<GameObject> deckCards)
        {
            // 清空所有牌堆
            ClearAllPiles();

            // 只存储Prefab引用，不实例化
            drawPile = new List<GameObject>();
            foreach (var cardPrefab in deckCards)
            {
                if (cardPrefab != null)
                {
                    drawPile.Add(cardPrefab);
                }
            }

            // 洗牌
            ShuffleDrawPile();
        }

        /// <summary>
        /// 从牌堆抽取指定数量的牌到手牌（实例化卡牌）
        /// </summary>
        /// <param name="count">要抽取的牌数</param>
        /// <param name="maxHandSize">手牌上限（-1表示无上限）</param>
        /// <returns>实际抽取的牌数</returns>
        public int DrawCards(int count, int maxHandSize = -1)
        {
            int drawnCount = 0;
            
            // 计算实际可抽取的数量（考虑手牌上限）
            int actualCount = count;
            if (maxHandSize >= 0)
            {
                int availableSlots = maxHandSize - hand.Count;
                if (availableSlots <= 0)
                {
                    Debug.Log($"[CardPileManager] 手牌已满（当前：{hand.Count}，上限：{maxHandSize}），无法抽取更多卡牌");
                    return 0;
                }
                actualCount = Mathf.Min(count, availableSlots);
            }

            for (int i = 0; i < actualCount && drawPile.Count > 0; i++)
            {
                // 检查手牌上限（双重检查，防止在循环过程中手牌被其他操作修改）
                if (maxHandSize >= 0 && hand.Count >= maxHandSize)
                {
                    Debug.Log($"[CardPileManager] 抽取过程中手牌达到上限（{maxHandSize}），停止抽取");
                    break;
                }

                // 从牌堆取出Prefab引用
                GameObject cardPrefab = drawPile[0];
                drawPile.RemoveAt(0);

                // 实例化卡牌
                GameObject cardInstance = Instantiate(cardPrefab);
                cardInstance.SetActive(true);

                // 添加CardDragHandler组件（如果还没有）
                CardDragHandler dragHandler = cardInstance.GetComponent<CardDragHandler>();
                if (dragHandler == null)
                {
                    dragHandler = cardInstance.AddComponent<CardDragHandler>();
                }
                dragHandler.Status = CardStatus.Hand;

                // 放置到槽位或容器
                if (handSlotManager != null)
                {
                    // 使用槽位系统
                    HandSlot availableSlot = handSlotManager.GetFirstAvailableSlot();
                    if (availableSlot != null)
                    {
                        availableSlot.PlaceCard(dragHandler);
                        dragHandler.CurrentSlot = availableSlot;
                    }
                    else
                    {
                        Debug.LogWarning("[CardPileManager] 手牌槽位已满，无法放置卡牌");
                        Destroy(cardInstance);
                        continue;
                    }
                }
                else
                {
                    // 传统方式：直接放入容器
                    cardInstance.transform.SetParent(handContainer);
                }
                
                // 存储实例和Prefab的对应关系
                hand.Add(new HandCardInfo(cardInstance, cardPrefab));

                drawnCount++;
            }

            return drawnCount;
        }

        /// <summary>
        /// 将手牌中的牌移动到弃牌堆（销毁实例，存储Prefab引用）
        /// </summary>
        /// <param name="cardInstance">要丢弃的卡牌实例</param>
        /// <returns>是否成功丢弃</returns>
        public bool DiscardCard(GameObject cardInstance)
        {
            if (cardInstance == null)
            {
                Debug.LogWarning("[CardPileManager] 尝试丢弃空卡牌");
                return false;
            }

            // 查找对应的手牌信息
            HandCardInfo cardInfo = null;
            foreach (var info in hand)
            {
                if (info.instance == cardInstance)
                {
                    cardInfo = info;
                    break;
                }
            }

            if (cardInfo == null)
            {
                Debug.LogWarning("[CardPileManager] 尝试丢弃不在手牌中的卡牌");
                return false;
            }

            // 如果使用槽位系统，从槽位移除
            if (handSlotManager != null)
            {
                CardDragHandler dragHandler = cardInstance.GetComponent<CardDragHandler>();
                if (dragHandler != null && dragHandler.CurrentSlot != null)
                {
                    dragHandler.CurrentSlot.RemoveCard(dragHandler);
                }
            }

            // 从手牌移除
            hand.Remove(cardInfo);

            // 销毁实例
            if (cardInfo.instance != null)
            {
                Destroy(cardInfo.instance);
            }

            // 将Prefab引用放入弃牌堆（只存储数据，不实例化）
            discardPile.Add(cardInfo.prefab);

            return true;
        }

        /// <summary>
        /// 将手牌中的指定索引的牌移动到弃牌堆
        /// </summary>
        /// <param name="index">手牌索引</param>
        /// <returns>是否成功丢弃</returns>
        public bool DiscardCardAt(int index)
        {
            if (index < 0 || index >= hand.Count)
            {
                Debug.LogWarning($"[CardPileManager] 手牌索引 {index} 超出范围，手牌数量为 {hand.Count}");
                return false;
            }

            return DiscardCard(hand[index].instance);
        }

        /// <summary>
        /// 将弃牌堆洗回牌堆，并清空弃牌堆（只移动Prefab引用）
        /// </summary>
        public void ReshuffleDiscardPileToDrawPile()
        {
            // 将弃牌堆中的Prefab引用移回牌堆（只移动数据，不涉及实例）
            foreach (var cardPrefab in discardPile)
            {
                drawPile.Add(cardPrefab);
            }

            // 清空弃牌堆
            discardPile.Clear();

            // 洗牌
            ShuffleDrawPile();
        }

        /// <summary>
        /// 洗牌堆（洗Prefab引用）
        /// </summary>
        private void ShuffleDrawPile()
        {
            // Fisher-Yates 洗牌算法
            for (int i = drawPile.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                GameObject temp = drawPile[i];
                drawPile[i] = drawPile[randomIndex];
                drawPile[randomIndex] = temp;
            }
        }

        /// <summary>
        /// 清空所有牌堆
        /// </summary>
        public void ClearAllPiles()
        {
            // 销毁手牌中的所有实例（牌堆和弃牌堆只存储Prefab引用，不需要销毁）
            foreach (var cardInfo in hand)
            {
                if (cardInfo.instance != null)
                {
                    Destroy(cardInfo.instance);
                }
            }

            drawPile.Clear();
            hand.Clear();
            discardPile.Clear();
        }

        /// <summary>
        /// 检查牌堆是否为空
        /// </summary>
        public bool IsDrawPileEmpty()
        {
            return drawPile.Count == 0;
        }

        /// <summary>
        /// 直接向牌堆添加卡牌（Prefab引用，不实例化）
        /// </summary>
        /// <param name="cardPrefab">要添加的卡牌Prefab</param>
        /// <param name="shuffle">是否在添加后洗牌</param>
        /// <returns>是否添加成功</returns>
        public bool AddCardToDrawPile(GameObject cardPrefab, bool shuffle = false)
        {
            if (cardPrefab == null)
            {
                Debug.LogWarning("[CardPileManager] 尝试添加空卡牌到牌堆");
                return false;
            }

            drawPile.Add(cardPrefab);
            
            if (shuffle)
            {
                ShuffleDrawPile();
            }

            return true;
        }

        /// <summary>
        /// 批量向牌堆添加卡牌（Prefab引用，不实例化）
        /// </summary>
        /// <param name="cardPrefabs">要添加的卡牌Prefab列表</param>
        /// <param name="shuffle">是否在添加后洗牌</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int AddCardsToDrawPile(List<GameObject> cardPrefabs, bool shuffle = false)
        {
            if (cardPrefabs == null)
            {
                Debug.LogWarning("[CardPileManager] 尝试添加空的卡牌列表到牌堆");
                return 0;
            }

            int addedCount = 0;
            foreach (var cardPrefab in cardPrefabs)
            {
                if (cardPrefab != null)
                {
                    drawPile.Add(cardPrefab);
                    addedCount++;
                }
            }

            if (shuffle && addedCount > 0)
            {
                ShuffleDrawPile();
            }

            return addedCount;
        }

        /// <summary>
        /// 批量向牌堆添加卡牌（Prefab引用，不实例化）
        /// </summary>
        /// <param name="cardPrefabs">要添加的卡牌Prefab数组</param>
        /// <param name="shuffle">是否在添加后洗牌</param>
        /// <returns>成功添加的卡牌数量</returns>
        public int AddCardsToDrawPile(GameObject[] cardPrefabs, bool shuffle = false)
        {
            if (cardPrefabs == null)
            {
                Debug.LogWarning("[CardPileManager] 尝试添加空的卡牌数组到牌堆");
                return 0;
            }

            return AddCardsToDrawPile(new List<GameObject>(cardPrefabs), shuffle);
        }
    }
}

