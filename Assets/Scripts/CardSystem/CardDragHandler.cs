using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CardSystem
{
    /// <summary>
    /// 卡牌拖动处理器
    /// 处理卡牌的拖动逻辑
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("拖动设置")]
        [Tooltip("拖动时是否跟随鼠标")]
        [SerializeField] private bool followMouse = true;

        [Tooltip("拖动时的缩放比例")]
        [SerializeField] private float dragScale = 1.1f;

        [Tooltip("拖动时的层级偏移")]
        [SerializeField] private int dragSortingOrder = 100;

        private RectTransform rectTransform;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private Vector3 originalPosition;
        private Transform originalParent;
        private int originalSiblingIndex;
        private int originalSortingOrder;
        private Vector3 originalScale;

        /// <summary>
        /// 当前卡牌所在的槽位
        /// </summary>
        public ICardSlot CurrentSlot { get; set; }

        /// <summary>
        /// 卡牌状态（手牌/待使用）
        /// </summary>
        public CardStatus Status { get; set; } = CardStatus.Hand;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // 保存原始状态
            originalPosition = rectTransform.anchoredPosition;
            originalParent = rectTransform.parent;
            originalSiblingIndex = rectTransform.GetSiblingIndex();
            originalScale = rectTransform.localScale;

            // 设置为可拖动但不可交互
            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false;

            // 移动到Canvas根节点，确保在最上层
            if (canvas != null)
            {
                rectTransform.SetParent(canvas.transform);
            }

            // 设置拖动时的缩放
            rectTransform.localScale = originalScale * dragScale;

            // 设置排序顺序
            if (canvas != null && canvas.sortingOrder < dragSortingOrder)
            {
                canvas.sortingOrder = dragSortingOrder;
            }

            // 通知当前槽位开始拖动
            if (CurrentSlot != null)
            {
                CurrentSlot.OnCardBeginDrag(this);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (followMouse && canvas != null)
            {
                // 将屏幕坐标转换为Canvas坐标
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    eventData.position,
                    canvas.worldCamera,
                    out localPoint);

                rectTransform.anchoredPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // 恢复原始状态
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            rectTransform.localScale = originalScale;

            // 尝试找到目标槽位
            ICardSlot targetSlot = FindTargetSlot(eventData);

            if (targetSlot != null && targetSlot.CanAcceptCard(this))
            {
                // 移动到目标槽位
                MoveToSlot(targetSlot);
            }
            else
            {
                // 返回原始位置
                ReturnToOriginalPosition();
            }

            // 通知当前槽位拖动结束
            if (CurrentSlot != null)
            {
                CurrentSlot.OnCardEndDrag(this);
            }
        }

        /// <summary>
        /// 查找目标槽位（支持自动吸附到最近的槽位）
        /// </summary>
        private ICardSlot FindTargetSlot(PointerEventData eventData)
        {
            // 方法1：使用Raycast查找鼠标下的槽位
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                ICardSlot slot = result.gameObject.GetComponent<ICardSlot>();
                if (slot != null && slot != CurrentSlot && slot.CanAcceptCard(this))
                {
                    return slot;
                }
            }

            // 方法2：如果没有找到，尝试查找最近的槽位（自动吸附）
            return FindNearestSlot(eventData);
        }

        /// <summary>
        /// 查找最近的槽位（自动吸附）
        /// </summary>
        private ICardSlot FindNearestSlot(PointerEventData eventData)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return null;
            }

            // 将屏幕坐标转换为Canvas坐标
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out localPoint);

            ICardSlot nearestSlot = null;
            float nearestDistance = float.MaxValue;
            float snapDistance = 150f; // 吸附距离（像素）

            // 查找所有ICardSlot组件
            List<ICardSlot> allSlots = new List<ICardSlot>();
            MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in allMonoBehaviours)
            {
                if (mb is ICardSlot slot && slot != CurrentSlot && slot.CanAcceptCard(this))
                {
                    allSlots.Add(slot);
                }
            }

            if (allSlots.Count == 0)
            {
                return null;
            }

            foreach (ICardSlot slot in allSlots)
            {
                if (slot == CurrentSlot || !slot.CanAcceptCard(this))
                {
                    continue;
                }

                // 获取槽位的世界位置
                MonoBehaviour slotMono = slot as MonoBehaviour;
                if (slotMono == null)
                {
                    continue;
                }

                RectTransform slotRect = slotMono.GetComponent<RectTransform>();
                if (slotRect == null)
                {
                    continue;
                }

                // 转换为Canvas坐标
                Vector2 slotLocalPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, slotRect.position),
                    canvas.worldCamera,
                    out slotLocalPoint);

                // 计算距离
                float distance = Vector2.Distance(localPoint, slotLocalPoint);
                if (distance < nearestDistance && distance <= snapDistance)
                {
                    nearestDistance = distance;
                    nearestSlot = slot;
                }
            }

            return nearestSlot;
        }

        /// <summary>
        /// 移动到指定槽位
        /// </summary>
        private void MoveToSlot(ICardSlot targetSlot)
        {
            if (CurrentSlot != null)
            {
                CurrentSlot.RemoveCard(this);
            }

            targetSlot.PlaceCard(this);
            CurrentSlot = targetSlot;
        }

        /// <summary>
        /// 返回原始位置
        /// </summary>
        private void ReturnToOriginalPosition()
        {
            rectTransform.SetParent(originalParent);
            rectTransform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    /// <summary>
    /// 卡牌状态
    /// </summary>
    public enum CardStatus
    {
        Hand,       // 手牌
        Pending     // 待使用（在格表中）
    }
}

