using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MapSystem
{
    /// <summary>
    /// 地图可视化器
    /// 负责动态生成和显示地图UI
    /// </summary>
    public class MapVisualizer : MonoBehaviour
    {
        [Header("引用")]
        [Tooltip("地图管理器")]
        [SerializeField] private MapManager mapManager;

        [Tooltip("地图容器(所有节点和连接线将作为子对象生成在这里)")]
        [SerializeField] private RectTransform mapContainer;

        [Header("布局参数")]
        [Tooltip("层间距")]
        [SerializeField] private float layerSpacing = 150f;

        [Tooltip("节点间距(同一层内)")]
        [SerializeField] private float nodeSpacing = 120f;

        [Tooltip("节点位置随机偏移范围(让布局更自然)")]
        [SerializeField] private float nodeRandomOffset = 30f;

        [Tooltip("连接线宽度")]
        [SerializeField] private float lineWidth = 2f;

        [Tooltip("连接线颜色")]
        [SerializeField] private Color lineColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

        [Tooltip("已访问连接线颜色")]
        [SerializeField] private Color visitedLineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Header("自动布局")]
        [Tooltip("是否自动计算布局")]
        [SerializeField] private bool autoLayout = true;

        [Tooltip("底部间距(从容器底部开始的偏移)")]
        [SerializeField] private float bottomGap = 50f;

        [Tooltip("水平中心偏移")]
        [SerializeField] private float horizontalOffset = 0f;

        /// <summary>
        /// 节点可视化字典,key为节点ID
        /// </summary>
        private Dictionary<int, MapNodeVisual> nodeVisuals = new Dictionary<int, MapNodeVisual>();

        /// <summary>
        /// 连接线字典,key为"fromId-toId"
        /// </summary>
        private Dictionary<string, RectTransform> connectionLines = new Dictionary<string, RectTransform>();

        /// <summary>
        /// 节点点击事件
        /// </summary>
        public System.Action<MapNode> OnNodeClicked;

        private void Start()
        {
            // 如果没有手动指定,尝试自动查找MapManager
            if (mapManager == null)
            {
                mapManager = FindObjectOfType<MapManager>();
            }

            // 监听地图生成事件
            if (mapManager != null)
            {
                mapManager.OnMapGenerated += HandleMapGenerated;
                mapManager.OnNodeStateChanged += HandleNodeStateChanged;
            }
        }

        private void OnDestroy()
        {
            // 取消事件订阅
            if (mapManager != null)
            {
                mapManager.OnMapGenerated -= HandleMapGenerated;
                mapManager.OnNodeStateChanged -= HandleNodeStateChanged;
            }
        }

        /// <summary>
        /// 处理地图生成完成事件
        /// </summary>
        private void HandleMapGenerated(MapTopology topology)
        {
            GenerateVisualization();
        }

        /// <summary>
        /// 处理节点状态改变事件
        /// </summary>
        private void HandleNodeStateChanged()
        {
            UpdateNodeStates();
        }

        /// <summary>
        /// 生成地图可视化
        /// </summary>
        public void GenerateVisualization()
        {
            if (mapManager == null || mapManager.CurrentTopology == null)
            {
                Debug.LogWarning("[MapVisualizer] 地图未生成,无法创建可视化");
                return;
            }

            MapTopology topology = mapManager.CurrentTopology;
            MapGenerationConfig config = GetConfig();

            if (config == null)
            {
                Debug.LogError("[MapVisualizer] 无法获取地图配置");
                return;
            }

            // 清除现有可视化
            ClearVisualization();

            // 先创建节点可视化
            CreateNodeVisuals(topology, config);

            // 再创建连接线(需要节点位置信息)
            CreateConnectionLines(topology);

            // 更新节点状态
            UpdateNodeStates();

            Debug.Log($"[MapVisualizer] 地图可视化生成完成,共{nodeVisuals.Count}个节点");
        }

        /// <summary>
        /// 创建节点可视化
        /// </summary>
        private void CreateNodeVisuals(MapTopology topology, MapGenerationConfig config)
        {
            // 计算节点位置
            Dictionary<int, Vector2> nodePositions = CalculateNodePositions(topology);

            // 为每个节点创建可视化
            foreach (var node in topology.Nodes.Values)
            {
                // 获取节点图像和大小
                Sprite nodeSprite = GetNodeSprite(node, config);
                Vector2 nodeSize = GetNodeSize(node, config);

                // 动态创建节点GameObject
                GameObject nodeObj = new GameObject($"Node_{node.NodeId}_L{node.Layer}C{node.Column}");
                nodeObj.transform.SetParent(mapContainer, false);

                // 添加RectTransform
                RectTransform rectTransform = nodeObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = nodeSize;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                if (nodePositions.ContainsKey(node.NodeId))
                {
                    rectTransform.anchoredPosition = nodePositions[node.NodeId];
                }

                // 添加Image组件
                Image nodeImage = nodeObj.AddComponent<Image>();
                nodeImage.sprite = nodeSprite;
                nodeImage.preserveAspect = true;

                // 添加MapNodeVisual组件
                MapNodeVisual nodeVisual = nodeObj.AddComponent<MapNodeVisual>();
                // Initialize会设置图像和大小,但我们已经设置了,所以这里只是绑定节点数据
                nodeVisual.Initialize(node, nodeSprite, nodeSize);
                nodeVisual.OnNodeClicked += HandleNodeClicked;

                // 存储引用
                nodeVisuals[node.NodeId] = nodeVisual;
            }
        }

        /// <summary>
        /// 计算节点位置
        /// </summary>
        private Dictionary<int, Vector2> CalculateNodePositions(MapTopology topology)
        {
            Dictionary<int, Vector2> positions = new Dictionary<int, Vector2>();

            // 计算container的底部位置(相对于anchor)
            // 在Unity UI中，anchoredPosition是相对于anchor的
            // 如果anchor在底部，yPos=0就是底部
            // 如果anchor在中心，yPos=-rect.height/2是底部
            float containerBottom = 0f;
            if (mapContainer != null)
            {
                RectTransform containerRect = mapContainer;
                // 检查anchor是否在底部
                bool anchorAtBottom = Mathf.Approximately(containerRect.anchorMin.y, 0f) && 
                                     Mathf.Approximately(containerRect.anchorMax.y, 0f);
                
                if (!anchorAtBottom)
                {
                    // Anchor不在底部(可能在中心)，需要计算底部位置
                    // rect.yMin是相对于rect中心的底部位置
                    containerBottom = containerRect.rect.yMin;
                }
                // 如果anchor在底部，containerBottom保持为0
            }

            // 使用随机种子确保布局一致性
            System.Random random = new System.Random(topology.GetHashCode());

            // 按层计算位置
            for (int layer = 0; layer < topology.Height; layer++)
            {
                List<MapNode> layerNodes = topology.GetNodesAtLayer(layer);
                if (layerNodes.Count == 0)
                {
                    continue;
                }

                // 按列排序
                layerNodes = layerNodes.OrderBy(n => n.Column).ToList();

                // 计算该层的基础宽度(不严格居中，更自然)
                float baseSpacing = nodeSpacing;
                float totalBaseWidth = (layerNodes.Count - 1) * baseSpacing;
                float centerX = horizontalOffset;

                // 为每个节点分配位置，添加随机偏移让布局更自然
                for (int i = 0; i < layerNodes.Count; i++)
                {
                    MapNode node = layerNodes[i];
                    
                    // 基础X位置(相对于中心)
                    float baseX = centerX + (i - (layerNodes.Count - 1) / 2f) * baseSpacing;
                    
                    // 添加随机偏移(类似杀戮尖塔的自然布局)
                    float randomOffsetX = (float)(random.NextDouble() * 2 - 1) * nodeRandomOffset;
                    float randomOffsetY = (float)(random.NextDouble() * 2 - 1) * nodeRandomOffset * 0.5f; // Y偏移较小
                    
                    float xPos = baseX + randomOffsetX;
                    
                    // 计算Y位置(从底部开始,从下往上)
                    float yPos = containerBottom + bottomGap + layer * layerSpacing + randomOffsetY;
                    
                    positions[node.NodeId] = new Vector2(xPos, yPos);
                }
            }

            return positions;
        }

        /// <summary>
        /// 创建连接线
        /// </summary>
        private void CreateConnectionLines(MapTopology topology)
        {
            foreach (var node in topology.Nodes.Values)
            {
                foreach (int upperNeighborId in node.UpperNeighbors)
                {
                    MapNodeVisual fromVisual = nodeVisuals.ContainsKey(node.NodeId) ? nodeVisuals[node.NodeId] : null;
                    MapNodeVisual toVisual = nodeVisuals.ContainsKey(upperNeighborId) ? nodeVisuals[upperNeighborId] : null;

                    if (fromVisual != null && toVisual != null)
                    {
                        CreateConnectionLine(fromVisual, toVisual, topology);
                    }
                }
            }
        }

        /// <summary>
        /// 创建单条连接线
        /// </summary>
        private void CreateConnectionLine(MapNodeVisual fromVisual, MapNodeVisual toVisual, MapTopology topology)
        {
            string lineKey = $"{fromVisual.Node.NodeId}-{toVisual.Node.NodeId}";
            if (connectionLines.ContainsKey(lineKey))
            {
                return; // 已存在
            }

            // 动态创建连接线GameObject
            GameObject lineObj = new GameObject($"Line_{fromVisual.Node.NodeId}_to_{toVisual.Node.NodeId}");
            lineObj.transform.SetParent(mapContainer, false);

            // 添加RectTransform
            RectTransform lineRect = lineObj.AddComponent<RectTransform>();

            // 获取节点的RectTransform
            RectTransform fromRect = fromVisual.GetComponent<RectTransform>();
            RectTransform toRect = toVisual.GetComponent<RectTransform>();

            if (fromRect == null || toRect == null)
            {
                Debug.LogError("[MapVisualizer] 节点缺少RectTransform组件");
                Destroy(lineObj);
                return;
            }

            // 使用anchoredPosition计算位置(相对于同一个父容器)
            Vector2 fromPos = fromRect.anchoredPosition;
            Vector2 toPos = toRect.anchoredPosition;

            Vector2 direction = toPos - fromPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 设置连接线的anchoredPosition(中点)
            lineRect.anchoredPosition = (fromPos + toPos) / 2f;
            lineRect.sizeDelta = new Vector2(distance, lineWidth);
            lineRect.rotation = Quaternion.Euler(0, 0, angle);
            lineRect.anchorMin = new Vector2(0.5f, 0.5f);
            lineRect.anchorMax = new Vector2(0.5f, 0.5f);
            lineRect.pivot = new Vector2(0.5f, 0.5f);

            // 添加Image组件并设置颜色
            Image lineImage = lineObj.AddComponent<Image>();
            // 检查是否已访问
            bool isVisited = mapManager.IsNodeVisited(fromVisual.Node.NodeId) && 
                            mapManager.IsNodeVisited(toVisual.Node.NodeId);
            lineImage.color = isVisited ? visitedLineColor : lineColor;

            // 确保连接线在节点下方(通过设置SiblingIndex)
            lineObj.transform.SetAsFirstSibling();

            // 存储引用
            connectionLines[lineKey] = lineRect;
        }

        /// <summary>
        /// 获取节点图像
        /// </summary>
        private Sprite GetNodeSprite(MapNode node, MapGenerationConfig config)
        {
            // Boss节点使用特殊图像
            if (node.IsBoss && config.bossNodeSprite != null)
            {
                return config.bossNodeSprite;
            }

            // 根据节点类型查找图像(从NodeTypeConfig中获取)
            if (config.nodeTypeConfigs != null && !string.IsNullOrEmpty(node.NodeType))
            {
                foreach (var typeConfig in config.nodeTypeConfigs)
                {
                    if (typeConfig.nodeType == node.NodeType && typeConfig.sprite != null)
                    {
                        return typeConfig.sprite;
                    }
                }
            }

            // 使用默认图像
            return config.defaultNodeSprite;
        }

        /// <summary>
        /// 获取节点大小
        /// </summary>
        private Vector2 GetNodeSize(MapNode node, MapGenerationConfig config)
        {
            // Boss节点使用配置的大小
            if (node.IsBoss)
            {
                return config.bossNodeSize;
            }

            // 根据节点类型查找大小(从NodeTypeConfig中获取)
            if (config.nodeTypeConfigs != null && !string.IsNullOrEmpty(node.NodeType))
            {
                foreach (var typeConfig in config.nodeTypeConfigs)
                {
                    if (typeConfig.nodeType == node.NodeType)
                    {
                        return typeConfig.nodeSize;
                    }
                }
            }

            // 使用默认大小
            return new Vector2(80, 80);
        }

        /// <summary>
        /// 更新节点状态
        /// </summary>
        public void UpdateNodeStates()
        {
            if (mapManager == null || mapManager.CurrentTopology == null)
            {
                return;
            }

            bool hasSelectedStart = mapManager.HasSelectedStartNode();
            MapNode currentNode = mapManager.CurrentNode;
            List<MapNode> availableNodes = hasSelectedStart ? mapManager.GetAvailableNextNodes() : null;
            List<MapNode> availableStartNodes = hasSelectedStart ? null : mapManager.GetAvailableStartNodes();

            foreach (var kvp in nodeVisuals)
            {
                MapNodeVisual visual = kvp.Value;
                MapNode node = visual.Node;

                MapNodeVisual.NodeState state = MapNodeVisual.NodeState.Normal;

                if (!hasSelectedStart)
                {
                    // 未选择起始节点，底层节点可被选择
                    if (node.Layer == 0)
                    {
                        state = MapNodeVisual.NodeState.Available; // 可选择的起始节点
                    }
                    else
                    {
                        state = MapNodeVisual.NodeState.Unavailable; // 非底层节点不可选择
                    }
                }
                else
                {
                    // 已选择起始节点，正常状态逻辑
                    if (currentNode != null && node.NodeId == currentNode.NodeId)
                    {
                        state = MapNodeVisual.NodeState.Current;
                    }
                    else if (mapManager.IsNodeVisited(node.NodeId))
                    {
                        state = MapNodeVisual.NodeState.Visited;
                    }
                    else if (availableNodes != null && availableNodes.Any(n => n.NodeId == node.NodeId))
                    {
                        state = MapNodeVisual.NodeState.Available;
                    }
                    else if (node.Layer <= (currentNode?.Layer ?? -1))
                    {
                        // 上层节点不可访问
                        state = MapNodeVisual.NodeState.Unavailable;
                    }
                }

                visual.SetState(state);
            }

            // 更新连接线颜色
            UpdateConnectionLineColors();
        }

        /// <summary>
        /// 更新连接线颜色
        /// </summary>
        private void UpdateConnectionLineColors()
        {
            foreach (var kvp in connectionLines)
            {
                string[] ids = kvp.Key.Split('-');
                if (ids.Length != 2)
                {
                    continue;
                }

                int fromId = int.Parse(ids[0]);
                int toId = int.Parse(ids[1]);

                bool isVisited = mapManager.IsNodeVisited(fromId) && mapManager.IsNodeVisited(toId);

                Image lineImage = kvp.Value.GetComponent<Image>();
                if (lineImage != null)
                {
                    lineImage.color = isVisited ? visitedLineColor : lineColor;
                }
            }
        }

        /// <summary>
        /// 处理节点点击
        /// </summary>
        private void HandleNodeClicked(MapNodeVisual nodeVisual)
        {
            if (nodeVisual == null || nodeVisual.Node == null)
            {
                return;
            }

            // 检查是否已选择起始节点
            if (!mapManager.HasSelectedStartNode())
            {
                // 未选择起始节点，尝试选择该节点作为起始节点
                if (nodeVisual.Node.Layer == 0)
                {
                    bool success = mapManager.SelectStartNode(nodeVisual.Node.NodeId);
                    if (success)
                    {
                        // 更新可视化状态
                        UpdateNodeStates();
                        OnNodeClicked?.Invoke(nodeVisual.Node);
                    }
                }
                else
                {
                    Debug.Log($"[MapVisualizer] 请先选择底层节点作为起始节点");
                }
                return;
            }

            // 已选择起始节点，检查是否可以移动到该节点
            List<MapNode> availableNodes = mapManager.GetAvailableNextNodes();
            bool canMove = availableNodes != null && availableNodes.Any(n => n.NodeId == nodeVisual.Node.NodeId);

            if (canMove)
            {
                // 移动到节点
                bool success = mapManager.MoveToNode(nodeVisual.Node.NodeId);
                if (success)
                {
                    // 更新可视化状态
                    UpdateNodeStates();
                    OnNodeClicked?.Invoke(nodeVisual.Node);
                }
            }
            else
            {
                Debug.Log($"[MapVisualizer] 无法移动到节点 {nodeVisual.Node.NodeId}");
            }
        }

        /// <summary>
        /// 清除可视化
        /// </summary>
        public void ClearVisualization()
        {
            // 清除节点
            foreach (var visual in nodeVisuals.Values)
            {
                if (visual != null)
                {
                    visual.OnNodeClicked -= HandleNodeClicked;
                    Destroy(visual.gameObject);
                }
            }
            nodeVisuals.Clear();

            // 清除连接线
            foreach (var line in connectionLines.Values)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }
            connectionLines.Clear();
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        private MapGenerationConfig GetConfig()
        {
            if (mapManager != null)
            {
                return mapManager.Config;
            }
            return null;
        }

        /// <summary>
        /// 刷新可视化(当地图状态改变时调用)
        /// </summary>
        public void RefreshVisualization()
        {
            UpdateNodeStates();
        }
    }
}

