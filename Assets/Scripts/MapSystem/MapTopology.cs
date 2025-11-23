using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapSystem
{
    /// <summary>
    /// 地图拓扑结构
    /// 表示地图的节点和连接关系(DAG)
    /// </summary>
    [System.Serializable]
    public class MapTopology
    {
        /// <summary>
        /// 所有节点的字典,key为节点ID
        /// </summary>
        public Dictionary<int, MapNode> Nodes { get; private set; }

        /// <summary>
        /// 按层组织的节点列表,key为层数
        /// </summary>
        public Dictionary<int, List<MapNode>> NodesByLayer { get; private set; }

        /// <summary>
        /// 地图高度(层数)
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// 地图宽度(每层最大列数)
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Boss节点ID
        /// </summary>
        public int BossNodeId { get; set; }

        /// <summary>
        /// 下一个可用的节点ID
        /// </summary>
        private int nextNodeId = 0;

        public MapTopology(int height, int width)
        {
            Height = height;
            Width = width;
            Nodes = new Dictionary<int, MapNode>();
            NodesByLayer = new Dictionary<int, List<MapNode>>();
            BossNodeId = -1;

            // 初始化层级字典
            for (int i = 0; i < height; i++)
            {
                NodesByLayer[i] = new List<MapNode>();
            }
        }

        /// <summary>
        /// 创建新节点
        /// </summary>
        public MapNode CreateNode(int layer, int column)
        {
            int nodeId = nextNodeId++;
            MapNode node = new MapNode(nodeId, layer, column);
            Nodes[nodeId] = node;
            NodesByLayer[layer].Add(node);
            return node;
        }

        /// <summary>
        /// 添加节点(用于从数据恢复)
        /// </summary>
        public void AddNode(MapNode node)
        {
            if (Nodes.ContainsKey(node.NodeId))
            {
                Debug.LogWarning($"[MapTopology] 节点ID {node.NodeId} 已存在,跳过添加");
                return;
            }

            Nodes[node.NodeId] = node;
            if (!NodesByLayer.ContainsKey(node.Layer))
            {
                NodesByLayer[node.Layer] = new List<MapNode>();
            }
            NodesByLayer[node.Layer].Add(node);

            if (node.IsBoss)
            {
                BossNodeId = node.NodeId;
            }

            // 更新nextNodeId
            if (node.NodeId >= nextNodeId)
            {
                nextNodeId = node.NodeId + 1;
            }
        }

        /// <summary>
        /// 删除节点及其所有连接
        /// </summary>
        public void RemoveNode(int nodeId)
        {
            if (!Nodes.ContainsKey(nodeId))
            {
                return;
            }

            MapNode node = Nodes[nodeId];

            // 移除所有上层邻居的连接
            foreach (int upperNeighborId in node.UpperNeighbors.ToList())
            {
                MapNode upperNeighbor = GetNode(upperNeighborId);
                if (upperNeighbor != null)
                {
                    upperNeighbor.LowerNeighbors.Remove(nodeId);
                }
            }

            // 移除所有下层邻居的连接
            foreach (int lowerNeighborId in node.LowerNeighbors.ToList())
            {
                MapNode lowerNeighbor = GetNode(lowerNeighborId);
                if (lowerNeighbor != null)
                {
                    lowerNeighbor.UpperNeighbors.Remove(nodeId);
                }
            }

            // 从层级字典中移除
            if (NodesByLayer.ContainsKey(node.Layer))
            {
                NodesByLayer[node.Layer].Remove(node);
            }

            // 如果是Boss节点，清除Boss节点ID
            if (node.IsBoss)
            {
                BossNodeId = -1;
            }

            // 从节点字典中移除
            Nodes.Remove(nodeId);
        }

        /// <summary>
        /// 添加有向边(从下层节点到上层节点)
        /// </summary>
        public void AddEdge(int fromNodeId, int toNodeId)
        {
            if (!Nodes.ContainsKey(fromNodeId) || !Nodes.ContainsKey(toNodeId))
            {
                Debug.LogWarning($"[MapTopology] 添加边失败: 节点不存在 from:{fromNodeId} to:{toNodeId}");
                return;
            }

            MapNode fromNode = Nodes[fromNodeId];
            MapNode toNode = Nodes[toNodeId];

            // 检查层数关系
            if (fromNode.Layer >= toNode.Layer)
            {
                Debug.LogWarning($"[MapTopology] 添加边失败: 只能从下层连接到上层 from:L{fromNode.Layer} to:L{toNode.Layer}");
                return;
            }

            fromNode.AddUpperNeighbor(toNodeId);
            toNode.AddLowerNeighbor(fromNodeId);
        }

        /// <summary>
        /// 获取指定层的所有节点
        /// </summary>
        public List<MapNode> GetNodesAtLayer(int layer)
        {
            if (NodesByLayer.ContainsKey(layer))
            {
                return new List<MapNode>(NodesByLayer[layer]);
            }
            return new List<MapNode>();
        }

        /// <summary>
        /// 获取指定节点
        /// </summary>
        public MapNode GetNode(int nodeId)
        {
            Nodes.TryGetValue(nodeId, out MapNode node);
            return node;
        }

        /// <summary>
        /// 检查从底层到Boss的连通性
        /// </summary>
        public bool CheckConnectivity()
        {
            if (BossNodeId == -1)
            {
                Debug.LogWarning("[MapTopology] 没有Boss节点");
                return false;
            }

            // 获取底层(第0层)的所有节点作为起点
            List<MapNode> bottomLayerNodes = GetNodesAtLayer(0);
            if (bottomLayerNodes.Count == 0)
            {
                Debug.LogWarning("[MapTopology] 底层没有节点");
                return false;
            }

            // 使用BFS检查从任意底层节点是否能到达Boss
            HashSet<int> visited = new HashSet<int>();
            Queue<int> queue = new Queue<int>();

            // 将所有底层节点加入队列
            foreach (MapNode node in bottomLayerNodes)
            {
                queue.Enqueue(node.NodeId);
                visited.Add(node.NodeId);
            }

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                MapNode currentNode = Nodes[currentId];

                if (currentId == BossNodeId)
                {
                    return true; // 找到Boss
                }

                // 遍历上层邻居
                foreach (int neighborId in currentNode.UpperNeighbors)
                {
                    if (!visited.Contains(neighborId))
                    {
                        visited.Add(neighborId);
                        queue.Enqueue(neighborId);
                    }
                }
            }

            return false; // 无法到达Boss
        }

        /// <summary>
        /// 检查是否有死节点(无法继续向上)
        /// </summary>
        public List<int> FindDeadEndNodes()
        {
            List<int> deadEnds = new List<int>();

            foreach (var node in Nodes.Values)
            {
                // Boss节点不算死节点
                if (node.IsBoss)
                {
                    continue;
                }

                // 如果没有上层邻居,且不是Boss,则是死节点
                if (node.OutDegree == 0)
                {
                    deadEnds.Add(node.NodeId);
                }
            }

            return deadEnds;
        }

        /// <summary>
        /// 获取从底层到Boss的所有路径
        /// </summary>
        public List<List<int>> GetAllPathsToBoss()
        {
            List<List<int>> allPaths = new List<List<int>>();

            if (BossNodeId == -1)
            {
                return allPaths;
            }

            // 获取底层(第0层)的所有节点作为起点
            List<MapNode> bottomLayerNodes = GetNodesAtLayer(0);
            if (bottomLayerNodes.Count == 0)
            {
                return allPaths;
            }

            foreach (MapNode startNode in bottomLayerNodes)
            {
                List<List<int>> pathsFromStart = FindPaths(startNode.NodeId, BossNodeId);
                allPaths.AddRange(pathsFromStart);
            }

            return allPaths;
        }

        /// <summary>
        /// 获取从指定节点到Boss的所有路径
        /// </summary>
        public List<List<int>> GetPathsFromNodeToBoss(int startNodeId)
        {
            if (BossNodeId == -1)
            {
                return new List<List<int>>();
            }

            return FindPaths(startNodeId, BossNodeId);
        }

        /// <summary>
        /// 使用DFS查找从起点到终点的所有路径
        /// </summary>
        private List<List<int>> FindPaths(int startId, int endId)
        {
            List<List<int>> paths = new List<List<int>>();
            List<int> currentPath = new List<int>();
            HashSet<int> visited = new HashSet<int>();

            DFS(startId, endId, currentPath, visited, paths);
            return paths;
        }

        private void DFS(int currentNodeId, int endId, List<int> currentPath, HashSet<int> visited, List<List<int>> allPaths)
        {
            if (currentNodeId == endId)
            {
                List<int> path = new List<int>(currentPath) { endId };
                allPaths.Add(path);
                return;
            }

            if (visited.Contains(currentNodeId))
            {
                return; // 避免循环(虽然DAG理论上不应该有循环)
            }

            visited.Add(currentNodeId);
            currentPath.Add(currentNodeId);

            MapNode currentNode = Nodes[currentNodeId];
            foreach (int neighborId in currentNode.UpperNeighbors)
            {
                DFS(neighborId, endId, currentPath, visited, allPaths);
            }

            currentPath.RemoveAt(currentPath.Count - 1);
            visited.Remove(currentNodeId);
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public string GetStatistics()
        {
            int totalNodes = Nodes.Count;
            int totalEdges = Nodes.Values.Sum(n => n.OutDegree);
            int avgOutDegree = totalNodes > 0 ? totalEdges / totalNodes : 0;
            int maxOutDegree = Nodes.Values.Max(n => n.OutDegree);
            int minOutDegree = Nodes.Values.Min(n => n.OutDegree);

            int bottomLayerNodeCount = GetNodesAtLayer(0).Count;
            string stats = $"[MapTopology统计] 总节点数:{totalNodes} 总边数:{totalEdges} " +
                          $"平均出度:{avgOutDegree} 最大出度:{maxOutDegree} 最小出度:{minOutDegree} " +
                          $"底层节点数:{bottomLayerNodeCount} Boss节点:{BossNodeId}";

            return stats;
        }
    }
}

