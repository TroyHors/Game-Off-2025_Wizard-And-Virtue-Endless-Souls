using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MapSystem
{
    /// <summary>
    /// 地图节点可视化组件
    /// 表示单个节点的UI显示
    /// </summary>
    public class MapNodeVisual : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI组件")]
        [Tooltip("节点图像")]
        [SerializeField] private Image nodeImage;

        [Tooltip("节点背景(可选)")]
        [SerializeField] private Image backgroundImage;

        [Tooltip("节点状态指示器(可选,用于显示已访问/当前/可访问等状态)")]
        [SerializeField] private Image statusIndicator;

        [Header("状态颜色")]
        [Tooltip("正常状态颜色")]
        [SerializeField] private Color normalColor = Color.white;

        [Tooltip("已访问状态颜色")]
        [SerializeField] private Color visitedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Tooltip("当前节点颜色")]
        [SerializeField] private Color currentColor = new Color(1f, 1f, 0f, 1f);

        [Tooltip("可访问节点颜色")]
        [SerializeField] private Color availableColor = new Color(0.5f, 1f, 0.5f, 1f);

        [Tooltip("不可访问节点颜色")]
        [SerializeField] private Color unavailableColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        /// <summary>
        /// 关联的地图节点
        /// </summary>
        public MapNode Node { get; private set; }

        /// <summary>
        /// 节点点击事件
        /// </summary>
        public System.Action<MapNodeVisual> OnNodeClicked;

        /// <summary>
        /// 当前节点状态
        /// </summary>
        public enum NodeState
        {
            Normal,      // 正常
            Visited,     // 已访问
            Current,     // 当前节点
            Available,   // 可访问
            Unavailable  // 不可访问
        }

        private NodeState currentState = NodeState.Normal;

        private void Awake()
        {
            // 如果没有手动指定,尝试自动查找组件
            if (nodeImage == null)
            {
                nodeImage = GetComponent<Image>();
            }

            if (backgroundImage == null)
            {
                backgroundImage = transform.Find("Background")?.GetComponent<Image>();
            }

            if (statusIndicator == null)
            {
                statusIndicator = transform.Find("StatusIndicator")?.GetComponent<Image>();
            }
        }

        /// <summary>
        /// 初始化节点可视化
        /// </summary>
        public void Initialize(MapNode node, Sprite sprite, Vector2 size)
        {
            Node = node;

            // 确保Image组件已找到(如果Awake还没执行或没找到)
            if (nodeImage == null)
            {
                nodeImage = GetComponent<Image>();
            }

            // 设置图像
            if (nodeImage != null)
            {
                nodeImage.sprite = sprite;
                nodeImage.preserveAspect = true;
            }
            else
            {
                Debug.LogWarning($"[MapNodeVisual] 节点 {node.NodeId} 缺少Image组件");
            }

            // 设置大小
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = size;
            }

            // 设置名称
            gameObject.name = $"Node_{node.NodeId}_L{node.Layer}C{node.Column}";
        }

        /// <summary>
        /// 设置节点状态
        /// </summary>
        public void SetState(NodeState state)
        {
            currentState = state;

            Color targetColor = normalColor;
            switch (state)
            {
                case NodeState.Visited:
                    targetColor = visitedColor;
                    break;
                case NodeState.Current:
                    targetColor = currentColor;
                    break;
                case NodeState.Available:
                    targetColor = availableColor;
                    break;
                case NodeState.Unavailable:
                    targetColor = unavailableColor;
                    break;
            }

            // 应用颜色到图像
            if (nodeImage != null)
            {
                nodeImage.color = targetColor;
            }

            // 更新状态指示器
            if (statusIndicator != null)
            {
                statusIndicator.gameObject.SetActive(state == NodeState.Current || state == NodeState.Available);
                statusIndicator.color = state == NodeState.Current ? currentColor : availableColor;
            }
        }

        /// <summary>
        /// 获取节点状态
        /// </summary>
        public NodeState GetState()
        {
            return currentState;
        }

        /// <summary>
        /// 处理点击事件
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            OnNodeClicked?.Invoke(this);
        }

        /// <summary>
        /// 设置节点图像
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            if (nodeImage != null)
            {
                nodeImage.sprite = sprite;
            }
        }

        /// <summary>
        /// 获取节点世界位置(用于绘制连接线)
        /// </summary>
        public Vector3 GetWorldPosition()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                return rectTransform.position;
            }
            return transform.position;
        }

        /// <summary>
        /// 获取节点中心点(用于绘制连接线)
        /// </summary>
        public Vector3 GetCenterPosition()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                return rectTransform.position;
            }
            return transform.position;
        }
    }
}

