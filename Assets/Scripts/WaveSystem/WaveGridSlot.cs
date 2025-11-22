using System.Collections.Generic;
using UnityEngine;
using CardSystem;

namespace WaveSystem
{
    /// <summary>
    /// 波牌格子组件
    /// 挂载到格子的GameObject上，处理波牌的放置和移除
    /// </summary>
    public class WaveGridSlot : MonoBehaviour, ICardSlot
    {
        [Header("格子设置")]
        [Tooltip("格子在格表中的位置（必须与手牌波的位置对应）")]
        [SerializeField] private int gridPosition = 0;

        /// <summary>
        /// 设置格子位置（用于动态生成时）
        /// </summary>
        /// <param name="position">位置</param>
        public void SetGridPosition(int position)
        {
            gridPosition = position;
        }

        [Header("显示设置")]
        [Tooltip("放置波牌时的目标位置（相对于格子中心）")]
        [SerializeField] private Vector3 cardPlacementOffset = Vector3.zero;

        /// <summary>
        /// 格表管理器引用
        /// </summary>
        private HandWaveGridManager gridManager;

        /// <summary>
        /// 当前放置的波牌组件列表（支持多个波牌）
        /// </summary>
        private List<WaveCardComponent> cards = new List<WaveCardComponent>();

        /// <summary>
        /// 格子在格表中的位置
        /// </summary>
        public int GridPosition => gridPosition;

        /// <summary>
        /// 格子是否被占用
        /// </summary>
        public bool IsOccupied => cards.Count > 0;

        /// <summary>
        /// 格子中的波牌数量
        /// </summary>
        public int CardCount => cards.Count;

        /// <summary>
        /// 初始化格子
        /// </summary>
        /// <param name="manager">格表管理器</param>
        public void Initialize(HandWaveGridManager manager)
        {
            gridManager = manager;
        }

        /// <summary>
        /// 放置波牌到格子（支持多个波牌）
        /// </summary>
        /// <param name="cardComponent">波牌组件</param>
        public void PlaceCard(WaveCardComponent cardComponent)
        {
            if (cardComponent == null)
            {
                Debug.LogWarning($"[WaveGridSlot] 位置 {gridPosition} 尝试放置空的波牌组件");
                return;
            }

            if (cards.Contains(cardComponent))
            {
                Debug.LogWarning($"[WaveGridSlot] 位置 {gridPosition} 已包含该波牌");
                return;
            }

            cards.Add(cardComponent);
            
            // 设置波牌的位置和父对象
            cardComponent.transform.SetParent(transform);
            
            // 计算偏移位置（多个波牌时，可以堆叠显示）
            Vector3 offset = cardPlacementOffset;
            if (cards.Count > 1)
            {
                // 简单的堆叠偏移（可以根据需要调整）
                offset += Vector3.up * (cards.Count - 1) * 0.1f;
            }
            
            // 使用RectTransform或Transform设置位置
            RectTransform rectTransform = cardComponent.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = offset;
            }
            else
            {
                cardComponent.transform.localPosition = offset;
            }
            
            cardComponent.transform.localRotation = Quaternion.identity;
            cardComponent.transform.localScale = Vector3.one;

            // 更新CardDragHandler状态（如果存在）
            CardSystem.CardDragHandler dragHandler = cardComponent.GetComponent<CardSystem.CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.Status = CardSystem.CardStatus.Pending;
                dragHandler.CurrentSlot = this;
            }

            Debug.Log($"[WaveGridSlot] 位置 {gridPosition} 已放置波牌: {cardComponent.gameObject.name}（当前共 {cards.Count} 个波牌）");
        }

        /// <summary>
        /// 从格子移除波牌
        /// </summary>
        /// <param name="cardComponent">要移除的波牌组件（如果为null，则移除第一个）</param>
        public void RemoveCard(WaveCardComponent cardComponent = null)
        {
            if (!IsOccupied)
            {
                Debug.LogWarning($"[WaveGridSlot] 位置 {gridPosition} 未被占用，无法移除波牌");
                return;
            }

            if (cardComponent == null)
            {
                cardComponent = cards[0];
            }

            if (!cards.Contains(cardComponent))
            {
                Debug.LogWarning($"[WaveGridSlot] 位置 {gridPosition} 不包含该波牌");
                return;
            }

            cards.Remove(cardComponent);

            // 从父对象移除（但不销毁，可能还需要放回手牌）
            cardComponent.transform.SetParent(null);

            // 重新排列剩余波牌的位置
            for (int i = 0; i < cards.Count; i++)
            {
                Vector3 offset = cardPlacementOffset;
                if (cards.Count > 1)
                {
                    offset += Vector3.up * i * 0.1f;
                }
                cards[i].transform.localPosition = offset;
            }

            Debug.Log($"[WaveGridSlot] 位置 {gridPosition} 已移除波牌: {cardComponent.gameObject.name}（剩余 {cards.Count} 个波牌）");
        }

        /// <summary>
        /// 获取第一个波牌组件
        /// </summary>
        /// <returns>波牌组件，如果未被占用则返回null</returns>
        public WaveCardComponent GetFirstCard()
        {
            return cards.Count > 0 ? cards[0] : null;
        }

        /// <summary>
        /// 获取所有波牌组件
        /// </summary>
        /// <returns>波牌组件列表</returns>
        public List<WaveCardComponent> GetAllCards()
        {
            return new List<WaveCardComponent>(cards);
        }

        /// <summary>
        /// 获取指定索引的波牌组件
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>波牌组件，如果索引无效则返回null</returns>
        public WaveCardComponent GetCard(int index)
        {
            if (index >= 0 && index < cards.Count)
            {
                return cards[index];
            }
            return null;
        }

        #region ICardSlot 实现

        /// <summary>
        /// 是否可以接受指定的卡牌
        /// </summary>
        public bool CanAcceptCard(CardDragHandler card)
        {
            if (card == null)
            {
                return false;
            }

            // 检查卡牌是否有WaveCardComponent组件（波牌）
            WaveCardComponent waveCard = card.GetComponent<WaveCardComponent>();
            if (waveCard == null)
            {
                return false; // 不是波牌，不能放在波牌格子中
            }

            // 格子可以接受多个波牌，所以总是返回true
            return true;
        }

        /// <summary>
        /// 放置卡牌到槽位（ICardSlot接口）
        /// </summary>
        public void PlaceCard(CardDragHandler card)
        {
            if (card == null)
            {
                Debug.LogWarning("[WaveGridSlot] 尝试放置空的卡牌");
                return;
            }

            WaveCardComponent waveCard = card.GetComponent<WaveCardComponent>();
            if (waveCard == null)
            {
                Debug.LogWarning("[WaveGridSlot] 尝试放置非波牌到波牌格子");
                return;
            }

            // 使用现有的PlaceCard方法（会自动更新CardDragHandler状态）
            PlaceCard(waveCard);

            // 与手牌波配对（通过GridManager）
            if (gridManager != null)
            {
                gridManager.PlaceCardAtPosition(waveCard, gridPosition);
            }
        }

        /// <summary>
        /// 从槽位移除卡牌（ICardSlot接口）
        /// </summary>
        public void RemoveCard(CardDragHandler card)
        {
            if (card == null)
            {
                return;
            }

            WaveCardComponent waveCard = card.GetComponent<WaveCardComponent>();
            if (waveCard != null)
            {
                // 从手牌波撤回（通过GridManager）
                if (gridManager != null)
                {
                    gridManager.WithdrawCardFromPosition(gridPosition, waveCard);
                }
                
                // 移除卡牌
                RemoveCard(waveCard);
            }
        }

        /// <summary>
        /// 卡牌开始拖动时调用
        /// </summary>
        public void OnCardBeginDrag(CardDragHandler card)
        {
            // 可以在这里添加拖动开始时的视觉效果
        }

        /// <summary>
        /// 卡牌拖动结束时调用
        /// </summary>
        public void OnCardEndDrag(CardDragHandler card)
        {
            // 可以在这里添加拖动结束时的视觉效果
        }

        #endregion

        /// <summary>
        /// 在Inspector中设置格子位置
        /// </summary>
        private void OnValidate()
        {
            // 确保位置在合理范围内（可以在Inspector中手动调整）
            if (gridManager != null)
            {
                if (gridPosition < gridManager.MinGridPosition || gridPosition > gridManager.MaxGridPosition)
                {
                    Debug.LogWarning($"[WaveGridSlot] {gameObject.name} 的位置 {gridPosition} 超出格表范围 [{gridManager.MinGridPosition}, {gridManager.MaxGridPosition}]");
                }
            }
        }

        /// <summary>
        /// 在Scene视图中显示格子位置（仅编辑器）
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 绘制格子位置标签
            Gizmos.color = IsOccupied ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position + cardPlacementOffset, Vector3.one * 0.5f);
            
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, $"Pos: {gridPosition}");
#endif
        }
    }
}

