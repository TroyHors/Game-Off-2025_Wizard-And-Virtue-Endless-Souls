using System.Collections.Generic;
using UnityEngine;

namespace MapSystem
{
    /// <summary>
    /// 地图管理器
    /// 负责地图的生成、管理和运行时行为
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        [Header("配置")]
        [Tooltip("地图生成配置")]
        [SerializeField] private MapGenerationConfig config;

        [Header("运行时状态")]
        [Tooltip("当前地图拓扑")]
        [SerializeField] private MapTopology currentTopology;

        [Tooltip("当前玩家所在节点ID")]
        [SerializeField] private int currentNodeId = -1;

        [Tooltip("已访问的节点ID集合")]
        [SerializeField] private HashSet<int> visitedNodes = new HashSet<int>();

        [Header("随机种子")]
        [Tooltip("地图生成种子(-1表示随机)")]
        [SerializeField] private int mapSeed = -1;

        [Tooltip("是否在Start时自动生成地图")]
        [SerializeField] private bool generateOnStart = false;

        /// <summary>
        /// 当前地图拓扑
        /// </summary>
        public MapTopology CurrentTopology => currentTopology;

        /// <summary>
        /// 地图生成配置
        /// </summary>
        public MapGenerationConfig Config => config;

        /// <summary>
        /// 当前玩家所在节点
        /// </summary>
        public MapNode CurrentNode => currentTopology != null ? currentTopology.GetNode(currentNodeId) : null;

        /// <summary>
        /// 当前节点ID
        /// </summary>
        public int CurrentNodeId => currentNodeId;

        /// <summary>
        /// 是否已访问指定节点
        /// </summary>
        public bool IsNodeVisited(int nodeId) => visitedNodes.Contains(nodeId);

        private void Start()
        {
            if (generateOnStart)
            {
                Debug.Log("[MapManager] generateOnStart 为 true，开始生成地图");
                GenerateMap();
            }
            else
            {
                Debug.LogWarning("[MapManager] generateOnStart 为 false，地图不会自动生成。请手动调用 GenerateMap() 或设置 generateOnStart 为 true");
            }
        }

        /// <summary>
        /// 生成地图
        /// </summary>
        /// <param name="seed">随机种子(-1表示随机)</param>
        public void GenerateMap(int seed = -1)
        {
            if (config == null)
            {
                Debug.LogError("[MapManager] 地图生成配置未设置");
                return;
            }

            int actualSeed = seed >= 0 ? seed : (mapSeed >= 0 ? mapSeed : Random.Range(0, int.MaxValue));
            mapSeed = actualSeed;

            Debug.Log($"[MapManager] 开始生成地图,种子: {actualSeed}");

            // 阶段1: 生成拓扑
            currentTopology = TopologyGenerator.Generate(config, actualSeed);

            if (currentTopology == null)
            {
                Debug.LogError("[MapManager] 拓扑生成失败");
                return;
            }

            // 阶段2: 填充内容
            List<ContentFiller.IPathConstraint> constraints = CreatePathConstraints();
            ContentFiller.FillContent(currentTopology, config, actualSeed, constraints);

            // 不自动初始化玩家位置，等待玩家选择起始节点
            currentNodeId = -1;
            visitedNodes.Clear();

            Debug.Log($"[MapManager] 地图生成完成 {currentTopology.GetStatistics()}，等待玩家选择起始节点");

            // 触发地图生成完成事件
            OnMapGenerated?.Invoke(currentTopology);
        }

        /// <summary>
        /// 创建路径合理性约束
        /// </summary>
        private List<ContentFiller.IPathConstraint> CreatePathConstraints()
        {
            List<ContentFiller.IPathConstraint> constraints = new List<ContentFiller.IPathConstraint>();

            // 这里可以添加具体的约束实现
            // 例如: 最少营火节点数、最多连续精英节点数等
            // 由于用户要求留出接口,这里只返回空列表
            // 用户可以在Inspector中配置约束,或通过代码添加

            return constraints;
        }

        /// <summary>
        /// 选择起始节点(只能选择底层节点)
        /// </summary>
        /// <param name="nodeId">要选择的节点ID</param>
        /// <returns>是否选择成功</returns>
        public bool SelectStartNode(int nodeId)
        {
            if (currentTopology == null)
            {
                Debug.LogError("[MapManager] 无法选择起始节点: 地图未生成");
                return false;
            }

            // 检查是否已经选择了起始节点
            if (currentNodeId != -1)
            {
                Debug.LogWarning("[MapManager] 已经选择了起始节点，无法重新选择");
                return false;
            }

            MapNode node = currentTopology.GetNode(nodeId);
            if (node == null)
            {
                Debug.LogError($"[MapManager] 节点 {nodeId} 不存在");
                return false;
            }

            // 只能选择底层(第0层)的节点作为起始节点
            if (node.Layer != 0)
            {
                Debug.LogWarning($"[MapManager] 只能选择底层节点作为起始节点，节点 {nodeId} 在第 {node.Layer} 层");
                return false;
            }

            // 设置起始节点
            currentNodeId = nodeId;
            visitedNodes.Clear();
            visitedNodes.Add(nodeId);

            Debug.Log($"[MapManager] 玩家选择了起始节点: Node[{nodeId}] L{node.Layer}C{node.Column} Type:{node.NodeType}");

            // 触发节点进入事件
            OnNodeEntered?.Invoke(node);

            // 触发节点状态改变事件
            OnNodeStateChanged?.Invoke();

            return true;
        }

        /// <summary>
        /// 检查是否已选择起始节点
        /// </summary>
        public bool HasSelectedStartNode()
        {
            return currentNodeId != -1;
        }

        /// <summary>
        /// 获取可选择的起始节点列表(底层所有节点)
        /// </summary>
        public List<MapNode> GetAvailableStartNodes()
        {
            if (currentTopology == null)
            {
                return new List<MapNode>();
            }

            return currentTopology.GetNodesAtLayer(0);
        }

        /// <summary>
        /// 移动到指定节点
        /// </summary>
        /// <param name="targetNodeId">目标节点ID</param>
        /// <returns>是否移动成功</returns>
        public bool MoveToNode(int targetNodeId)
        {
            if (currentTopology == null)
            {
                Debug.LogError("[MapManager] 地图未生成");
                return false;
            }

            // 检查是否已选择起始节点
            if (currentNodeId == -1)
            {
                Debug.LogWarning("[MapManager] 请先选择起始节点");
                return false;
            }

            MapNode currentNode = currentTopology.GetNode(currentNodeId);
            MapNode targetNode = currentTopology.GetNode(targetNodeId);

            if (currentNode == null || targetNode == null)
            {
                Debug.LogError($"[MapManager] 节点不存在: current={currentNodeId} target={targetNodeId}");
                return false;
            }

            // 检查目标节点是否是当前节点的上层邻居
            if (!currentNode.UpperNeighbors.Contains(targetNodeId))
            {
                Debug.LogWarning($"[MapManager] 无法移动到Node[{targetNodeId}]: 不是当前节点的上层邻居");
                return false;
            }

            // 检查层数关系
            if (targetNode.Layer <= currentNode.Layer)
            {
                Debug.LogWarning($"[MapManager] 无法移动到Node[{targetNodeId}]: 只能向上移动");
                return false;
            }

            // 执行移动
            currentNodeId = targetNodeId;
            visitedNodes.Add(targetNodeId);

            Debug.Log($"[MapManager] 移动到Node[{targetNodeId}] L{targetNode.Layer}C{targetNode.Column} Type:{targetNode.NodeType}");

            // 触发移动事件
            OnNodeEntered?.Invoke(targetNode);

            // 触发节点状态改变事件(用于更新可视化)
            OnNodeStateChanged?.Invoke();

            return true;
        }

        /// <summary>
        /// 获取当前节点可以移动到的上层节点列表
        /// </summary>
        public List<MapNode> GetAvailableNextNodes()
        {
            if (currentTopology == null || currentNodeId == -1)
            {
                return new List<MapNode>();
            }

            MapNode currentNode = currentTopology.GetNode(currentNodeId);
            if (currentNode == null)
            {
                return new List<MapNode>();
            }

            List<MapNode> availableNodes = new List<MapNode>();
            foreach (int neighborId in currentNode.UpperNeighbors)
            {
                MapNode neighbor = currentTopology.GetNode(neighborId);
                if (neighbor != null)
                {
                    availableNodes.Add(neighbor);
                }
            }

            return availableNodes;
        }

        /// <summary>
        /// 检查是否到达Boss节点
        /// </summary>
        public bool IsAtBoss()
        {
            if (currentTopology == null || currentNodeId == -1)
            {
                return false;
            }

            return currentNodeId == currentTopology.BossNodeId;
        }

        /// <summary>
        /// 获取指定节点的信息字符串
        /// </summary>
        public string GetNodeInfo(int nodeId)
        {
            if (currentTopology == null)
            {
                return "地图未生成";
            }

            MapNode node = currentTopology.GetNode(nodeId);
            if (node == null)
            {
                return $"节点{nodeId}不存在";
            }

            string info = node.ToString();
            if (visitedNodes.Contains(nodeId))
            {
                info += " [已访问]";
            }
            if (nodeId == currentNodeId)
            {
                info += " [当前位置]";
            }

            return info;
        }

        /// <summary>
        /// 节点进入事件
        /// </summary>
        public System.Action<MapNode> OnNodeEntered;

        /// <summary>
        /// 地图生成完成事件
        /// </summary>
        public System.Action<MapTopology> OnMapGenerated;

        /// <summary>
        /// 节点状态改变事件(用于更新可视化)
        /// </summary>
        public System.Action OnNodeStateChanged;

        /// <summary>
        /// 重置地图(清除访问记录和玩家位置)
        /// </summary>
        public void ResetMap()
        {
            currentNodeId = -1;
            visitedNodes.Clear();
            OnNodeStateChanged?.Invoke();
            Debug.Log("[MapManager] 地图已重置，等待玩家重新选择起始节点");
        }

        /// <summary>
        /// 使用新种子重新生成地图
        /// </summary>
        public void RegenerateMap(int newSeed = -1)
        {
            GenerateMap(newSeed);
        }
    }
}

