using System.Collections.Generic;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 手牌波格表管理器
    /// 管理手牌波和格表系统，处理波牌的放置和配对
    /// </summary>
    public class HandWaveGridManager : MonoBehaviour
    {
        [Header("手牌波设置")]
        [Tooltip("手牌波的最小位置（格表起始位置）")]
        [SerializeField] private int minGridPosition = -10;
        
        [Tooltip("手牌波的最大位置（格表结束位置）")]
        [SerializeField] private int maxGridPosition = 10;
        
        [Header("格表设置")]
        [Tooltip("格表容器（所有格子会作为此GameObject的子对象动态生成）")]
        [SerializeField] private Transform gridContainer;
        
        [Tooltip("格子Prefab（必须包含WaveGridSlot组件）")]
        [SerializeField] private GameObject slotPrefab;
        
        [Header("卡牌系统引用")]
        [Tooltip("卡牌系统（用于将待使用的卡牌放入弃牌堆）")]
        [SerializeField] private CardSystem.CardSystem cardSystem;

        [Header("调试")]
        [Tooltip("是否在控制台打印手牌波详情")]
        [SerializeField] private bool debugPrintWaveDetails = true;

        /// <summary>
        /// 手牌波管理器
        /// </summary>
        private HandWaveManager handWaveManager = new HandWaveManager();

        /// <summary>
        /// 格子字典：位置 -> 格子组件
        /// </summary>
        private Dictionary<int, WaveGridSlot> gridSlots = new Dictionary<int, WaveGridSlot>();

        /// <summary>
        /// 当前手牌波（只读）
        /// </summary>
        public Wave HandWave => handWaveManager.HandWave;

        /// <summary>
        /// 手牌波管理器（只读）
        /// </summary>
        public HandWaveManager HandWaveManager => handWaveManager;

        /// <summary>
        /// 获取格表的最小位置
        /// </summary>
        public int MinGridPosition => minGridPosition;

        /// <summary>
        /// 获取格表的最大位置
        /// </summary>
        public int MaxGridPosition => maxGridPosition;

        private void Awake()
        {
            InitializeGrid();
        }

        /// <summary>
        /// 初始化格表
        /// </summary>
        private void InitializeGrid()
        {
            gridSlots.Clear();

            if (gridContainer == null)
            {
                Debug.LogWarning("[HandWaveGridManager] 格表容器未设置，无法初始化格子");
                return;
            }

            if (slotPrefab == null)
            {
                Debug.LogWarning("[HandWaveGridManager] 格子Prefab未设置，无法生成格子");
                return;
            }

            // 清除旧的格子
            foreach (Transform child in gridContainer)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(child.gameObject);
#endif
                }
            }

            // 动态生成格子
            for (int position = minGridPosition; position <= maxGridPosition; position++)
            {
                GameObject slotObj = Instantiate(slotPrefab, gridContainer);
                slotObj.name = $"Slot_{position}";
                
                WaveGridSlot slot = slotObj.GetComponent<WaveGridSlot>();
                if (slot == null)
                {
                    Debug.LogError($"[HandWaveGridManager] 格子Prefab缺少WaveGridSlot组件");
                    Destroy(slotObj);
                    continue;
                }

                // 设置格子位置
                slot.SetGridPosition(position);
                slot.Initialize(this);
                gridSlots[position] = slot;
            }

            Debug.Log($"[HandWaveGridManager] 格表初始化完成，共 {gridSlots.Count} 个格子（位置范围：{minGridPosition} 到 {maxGridPosition}）");
        }

        /// <summary>
        /// 在指定位置放置波牌（通过拖动系统调用）
        /// </summary>
        /// <param name="cardDragHandler">卡牌拖动处理器</param>
        /// <param name="gridPosition">格子位置</param>
        /// <returns>放置后的手牌波状态</returns>
        public Wave PlaceCardAtPosition(CardSystem.CardDragHandler cardDragHandler, int gridPosition)
        {
            if (cardDragHandler == null)
            {
                Debug.LogWarning("[HandWaveGridManager] 尝试放置空的卡牌拖动处理器");
                return handWaveManager.HandWave.Clone();
            }

            WaveCardComponent cardComponent = cardDragHandler.GetComponent<WaveCardComponent>();
            if (cardComponent == null)
            {
                Debug.LogWarning("[HandWaveGridManager] 卡牌拖动处理器没有WaveCardComponent组件");
                return handWaveManager.HandWave.Clone();
            }

            return PlaceCardAtPosition(cardComponent, gridPosition);
        }

        /// <summary>
        /// 在指定位置放置波牌
        /// </summary>
        /// <param name="cardComponent">波牌组件</param>
        /// <param name="gridPosition">格子位置</param>
        /// <returns>放置后的手牌波状态</returns>
        public Wave PlaceCardAtPosition(WaveCardComponent cardComponent, int gridPosition)
        {
            if (cardComponent == null)
            {
                Debug.LogWarning("[HandWaveGridManager] 尝试放置空的波牌组件");
                return handWaveManager.HandWave.Clone();
            }

            if (!gridSlots.ContainsKey(gridPosition))
            {
                Debug.LogWarning($"[HandWaveGridManager] 位置 {gridPosition} 不存在格子");
                return handWaveManager.HandWave.Clone();
            }

            WaveGridSlot slot = gridSlots[gridPosition];

            // 创建波牌数据，使用格子位置作为最尾端位置
            WaveCard card = new WaveCard(cardComponent.Wave, gridPosition);

            // 放置波牌到格子（支持多个波牌）
            slot.PlaceCard(cardComponent);

            // 如果卡牌有CardDragHandler，更新其状态和槽位引用
            CardSystem.CardDragHandler dragHandler = cardComponent.GetComponent<CardSystem.CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.Status = CardSystem.CardStatus.Pending;
                dragHandler.CurrentSlot = slot;
            }

            // 与手牌波配对
            List<Wave> resultWaves = handWaveManager.PlaceCard(card);

            if (debugPrintWaveDetails)
            {
                PrintHandWaveDetails();
            }

            return handWaveManager.HandWave.Clone();
        }

        /// <summary>
        /// 从指定位置撤回波牌
        /// </summary>
        /// <param name="gridPosition">格子位置</param>
        /// <param name="cardComponent">要撤回的波牌组件（如果为null，则撤回第一个）</param>
        /// <returns>撤回后的手牌波状态</returns>
        public Wave WithdrawCardFromPosition(int gridPosition, WaveCardComponent cardComponent = null)
        {
            if (!gridSlots.ContainsKey(gridPosition))
            {
                Debug.LogWarning($"[HandWaveGridManager] 位置 {gridPosition} 不存在格子");
                return handWaveManager.HandWave.Clone();
            }

            WaveGridSlot slot = gridSlots[gridPosition];
            if (!slot.IsOccupied)
            {
                Debug.LogWarning($"[HandWaveGridManager] 位置 {gridPosition} 的格子未被占用");
                return handWaveManager.HandWave.Clone();
            }

            // 如果没有指定波牌，获取第一个
            if (cardComponent == null)
            {
                cardComponent = slot.GetFirstCard();
            }

            if (cardComponent == null)
            {
                Debug.LogWarning($"[HandWaveGridManager] 位置 {gridPosition} 的格子中的波牌组件为空");
                return handWaveManager.HandWave.Clone();
            }

            // 创建波牌数据，使用格子位置作为最尾端位置
            WaveCard card = new WaveCard(cardComponent.Wave, gridPosition);

            // 从格子移除波牌
            slot.RemoveCard(cardComponent);

            // 与手牌波配对（使用负波）
            List<Wave> resultWaves = handWaveManager.WithdrawCard(card);

            if (debugPrintWaveDetails)
            {
                PrintHandWaveDetails();
            }

            return handWaveManager.HandWave.Clone();
        }

        /// <summary>
        /// 发出手牌波
        /// </summary>
        /// <returns>发出的波（首个波峰对齐到0号位）</returns>
        public Wave EmitHandWave()
        {
            Wave emittedWave = handWaveManager.EmitWave();
            
            if (debugPrintWaveDetails)
            {
                Debug.Log("[HandWaveGridManager] 发出手牌波");
                PrintWaveDetails(emittedWave, "发出的波");
            }

            return emittedWave;
        }

        /// <summary>
        /// 结束回合（供UI按钮调用，无返回值）
        /// 1. 将手牌波转换为最小位置是0的波并发出
        /// 2. 将标记为"待使用"的卡牌（在格表中的）放入弃牌堆
        /// </summary>
        public void EndTurn()
        {
            EndTurnWithResult();
        }

        /// <summary>
        /// 结束回合（返回发出的波）
        /// 1. 将手牌波转换为最小位置是0的波并发出
        /// 2. 将标记为"待使用"的卡牌（在格表中的）放入弃牌堆
        /// </summary>
        /// <returns>发出的波（最小位置对齐到0号位）</returns>
        public Wave EndTurnWithResult()
        {
            // 1. 先发出手牌波（最小位置对齐到0号位）
            // 注意：在发出之前，手牌波应该包含所有待使用卡牌的效果
            Wave emittedWave = EmitHandWave();

            // 2. 收集所有标记为"待使用"的卡牌（在格表中的）
            List<CardSystem.CardDragHandler> pendingCards = new List<CardSystem.CardDragHandler>();
            
            foreach (var slot in gridSlots.Values)
            {
                var cards = slot.GetAllCards();
                foreach (var waveCard in cards)
                {
                    CardSystem.CardDragHandler dragHandler = waveCard.GetComponent<CardSystem.CardDragHandler>();
                    if (dragHandler != null && dragHandler.Status == CardSystem.CardStatus.Pending)
                    {
                        pendingCards.Add(dragHandler);
                    }
                }
            }

            // 3. 将这些卡牌放入弃牌堆
            if (cardSystem != null)
            {
                foreach (var card in pendingCards)
                {
                    GameObject cardInstance = card.gameObject;
                    
                    // 从格表中移除（这会触发WithdrawCard，从手牌波中撤回）
                    // 注意：手牌波已经发出，所以撤回不会影响已发出的波
                    if (card.CurrentSlot != null)
                    {
                        card.CurrentSlot.RemoveCard(card);
                    }

                    // 放入弃牌堆（通过CardSystem）
                    // 注意：卡牌在格表中时仍然在CardPileManager的hand列表中，所以UseCard可以正常工作
                    bool success = cardSystem.UseCard(cardInstance);
                    
                    if (success)
                    {
                        Debug.Log($"[HandWaveGridManager] 将待使用的卡牌放入弃牌堆: {cardInstance.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[HandWaveGridManager] 无法将待使用的卡牌放入弃牌堆: {cardInstance.name}（可能不在手牌中）");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[HandWaveGridManager] 卡牌系统未设置，无法将待使用的卡牌放入弃牌堆");
            }

            Debug.Log($"[HandWaveGridManager] 回合结束，发出了 {emittedWave.PeakCount} 个波峰的波，移除了 {pendingCards.Count} 张待使用的卡牌");

            return emittedWave;
        }

        /// <summary>
        /// 重置手牌波
        /// </summary>
        public void ResetHandWave()
        {
            handWaveManager.ResetHandWave();
            
            // 清除所有格子中的波牌
            foreach (var slot in gridSlots.Values)
            {
                if (slot.IsOccupied)
                {
                    slot.RemoveCard();
                }
            }

            if (debugPrintWaveDetails)
            {
                Debug.Log("[HandWaveGridManager] 手牌波已重置");
            }
        }

        /// <summary>
        /// 获取指定位置的格子
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>格子组件，如果不存在则返回null</returns>
        public WaveGridSlot GetSlot(int position)
        {
            gridSlots.TryGetValue(position, out WaveGridSlot slot);
            return slot;
        }

        /// <summary>
        /// 打印手牌波详情
        /// </summary>
        private void PrintHandWaveDetails()
        {
            PrintWaveDetails(handWaveManager.HandWave, "当前手牌波");
        }

        /// <summary>
        /// 打印波的详细信息
        /// </summary>
        private void PrintWaveDetails(Wave wave, string label)
        {
            Debug.Log($"--- {label} ---");
            Debug.Log($"  PeakCount: {wave.PeakCount}, IsEmpty: {wave.IsEmpty}");
            Debug.Log($"  波的方向: {(wave.AttackDirection.HasValue ? (wave.AttackDirection.Value ? "→敌人" : "→其他") : "未定义（空波）")}");

            if (wave.IsEmpty)
            {
                Debug.Log("  (空波)");
            }
            else
            {
                var sortedPeaks = wave.GetSortedPeaks();
                foreach (var (position, peak) in sortedPeaks)
                {
                    string direction = peak.AttackDirection ? "→敌人" : "→其他";
                    Debug.Log($"  位置{position}: 强度={peak.Value}, 方向={direction}");
                }
            }
        }

        /// <summary>
        /// 在Inspector中手动刷新格表（用于运行时调试）
        /// </summary>
        [ContextMenu("刷新格表")]
        public void RefreshGrid()
        {
            InitializeGrid();
        }
    }
}

