using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 波牌格子组件
    /// 挂载到格子的GameObject上，处理波牌的放置和移除
    /// </summary>
    public class WaveGridSlot : MonoBehaviour
    {
        [Header("格子设置")]
        [Tooltip("格子在格表中的位置（必须与手牌波的位置对应）")]
        [SerializeField] private int gridPosition = 0;

        [Header("显示设置")]
        [Tooltip("放置波牌时的目标位置（相对于格子中心）")]
        [SerializeField] private Vector3 cardPlacementOffset = Vector3.zero;

        /// <summary>
        /// 格表管理器引用
        /// </summary>
        private HandWaveGridManager gridManager;

        /// <summary>
        /// 当前放置的波牌组件
        /// </summary>
        private WaveCardComponent currentCard;

        /// <summary>
        /// 格子在格表中的位置
        /// </summary>
        public int GridPosition => gridPosition;

        /// <summary>
        /// 格子是否被占用
        /// </summary>
        public bool IsOccupied => currentCard != null;

        /// <summary>
        /// 初始化格子
        /// </summary>
        /// <param name="manager">格表管理器</param>
        public void Initialize(HandWaveGridManager manager)
        {
            gridManager = manager;
        }

        /// <summary>
        /// 放置波牌到格子
        /// </summary>
        /// <param name="cardComponent">波牌组件</param>
        public void PlaceCard(WaveCardComponent cardComponent)
        {
            if (cardComponent == null)
            {
                Debug.LogWarning($"[WaveGridSlot] 位置 {gridPosition} 尝试放置空的波牌组件");
                return;
            }

            if (IsOccupied)
            {
                Debug.LogWarning($"[WaveGridSlot] 位置 {gridPosition} 已被占用，无法放置波牌");
                return;
            }

            currentCard = cardComponent;
            
            // 设置波牌的位置和父对象
            cardComponent.transform.SetParent(transform);
            cardComponent.transform.localPosition = cardPlacementOffset;
            cardComponent.transform.localRotation = Quaternion.identity;

            Debug.Log($"[WaveGridSlot] 位置 {gridPosition} 已放置波牌: {cardComponent.gameObject.name}");
        }

        /// <summary>
        /// 从格子移除波牌
        /// </summary>
        public void RemoveCard()
        {
            if (!IsOccupied)
            {
                Debug.LogWarning($"[WaveGridSlot] 位置 {gridPosition} 未被占用，无法移除波牌");
                return;
            }

            WaveCardComponent card = currentCard;
            currentCard = null;

            // 从父对象移除（但不销毁，可能还需要放回手牌）
            card.transform.SetParent(null);

            Debug.Log($"[WaveGridSlot] 位置 {gridPosition} 已移除波牌: {card.gameObject.name}");
        }

        /// <summary>
        /// 获取当前放置的波牌组件
        /// </summary>
        /// <returns>波牌组件，如果未被占用则返回null</returns>
        public WaveCardComponent GetCard()
        {
            return currentCard;
        }

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

