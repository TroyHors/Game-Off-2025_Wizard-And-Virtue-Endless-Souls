using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 手牌槽位
    /// 用于手牌堆中的卡牌槽位
    /// </summary>
    public class HandSlot : MonoBehaviour, ICardSlot
    {
        [Header("槽位设置")]
        [Tooltip("放置卡牌时的目标位置（相对于槽位中心）")]
        [SerializeField] private Vector3 cardPlacementOffset = Vector3.zero;

        /// <summary>
        /// 当前放置的卡牌
        /// </summary>
        private CardDragHandler currentCard;

        /// <summary>
        /// 槽位是否被占用
        /// </summary>
        public bool IsOccupied => currentCard != null;

        /// <summary>
        /// 是否可以接受指定的卡牌
        /// </summary>
        public bool CanAcceptCard(CardDragHandler card)
        {
            if (card == null)
            {
                return false;
            }

            // 手牌槽位可以接受任何卡牌（如果未被占用）
            return !IsOccupied;
        }

        /// <summary>
        /// 放置卡牌到槽位
        /// </summary>
        public void PlaceCard(CardDragHandler card)
        {
            if (card == null)
            {
                Debug.LogWarning("[HandSlot] 尝试放置空的卡牌");
                return;
            }

            if (IsOccupied && currentCard != card)
            {
                Debug.LogWarning("[HandSlot] 槽位已被占用，无法放置卡牌");
                return;
            }

            currentCard = card;
            card.Status = CardStatus.Hand;

            // 设置卡牌的位置和父对象
            card.transform.SetParent(transform);
            card.GetComponent<RectTransform>().anchoredPosition = cardPlacementOffset;
            card.transform.localRotation = Quaternion.identity;
            card.transform.localScale = Vector3.one;

            Debug.Log($"[HandSlot] {gameObject.name} 已放置卡牌: {card.gameObject.name}");
        }

        /// <summary>
        /// 从槽位移除卡牌
        /// </summary>
        public void RemoveCard(CardDragHandler card)
        {
            if (currentCard == card)
            {
                currentCard = null;
                Debug.Log($"[HandSlot] {gameObject.name} 已移除卡牌: {card.gameObject.name}");
            }
        }

        /// <summary>
        /// 清除槽位（直接清空，不检查卡牌）
        /// 用于重置槽位状态
        /// </summary>
        public void Clear()
        {
            if (currentCard != null)
            {
                currentCard = null;
                Debug.Log($"[HandSlot] {gameObject.name} 槽位已清除");
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

        /// <summary>
        /// 获取当前卡牌
        /// </summary>
        public CardDragHandler GetCard()
        {
            return currentCard;
        }
    }
}

