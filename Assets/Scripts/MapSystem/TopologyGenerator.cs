using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapSystem
{
    /// <summary>
    /// 拓扑生成器
    /// 负责生成地图的节点和路径结构(DAG)
    /// </summary>
    public static class TopologyGenerator
    {
        /// <summary>
        /// 生成地图拓扑结构
        /// </summary>
        /// <param name="config">生成配置</param>
        /// <param name="seed">随机种子</param>
        /// <returns>生成的地图拓扑</returns>
        public static MapTopology Generate(MapGenerationConfig config, int seed = -1)
        {
            if (!config.Validate())
            {
                Debug.LogError("[TopologyGenerator] 配置验证失败");
                return null;
            }

            // 设置随机种子
            int baseSeed = seed >= 0 ? seed : System.Environment.TickCount;
            System.Random random = new System.Random(baseSeed);

            const int maxRetries = 50; // 最多重试50次
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                // 每次重试使用不同的种子
                int currentSeed = baseSeed + retryCount;
                random = new System.Random(currentSeed);

                MapTopology topology = new MapTopology(config.height, config.width);

                // 阶段1: 生成底层节点
                GenerateBottomLayer(topology, config, random);

                // 阶段2: 从下往上逐层生成节点和连接(不包括顶层，顶层只包含Boss)
                for (int layer = 0; layer < config.height - 2; layer++)
                {
                    GenerateLayerConnections(topology, layer, config, random);
                }

                // 阶段3: 生成顶层Boss节点(连接到倒数第二层)
                GenerateBossNode(topology, config, random);

                // 阶段4: 验证和修复
                ValidateAndFix(topology, config, random);

                // 阶段5: 检查路径多样性
                if (CheckPathDiversity(topology))
                {
                    Debug.Log($"[TopologyGenerator] 拓扑生成完成（重试 {retryCount} 次） {topology.GetStatistics()}");
                    return topology;
                }
                else
                {
                    Debug.LogWarning($"[TopologyGenerator] 路径多样性不足，重新生成（重试 {retryCount + 1}/{maxRetries}）");
                    retryCount++;
                }
            }

            Debug.LogError($"[TopologyGenerator] 达到最大重试次数({maxRetries})，无法生成满足路径多样性要求的地图");
            return null;
        }

        /// <summary>
        /// 生成底层节点
        /// </summary>
        private static void GenerateBottomLayer(MapTopology topology, MapGenerationConfig config, System.Random random)
        {
            // 确定底层节点数量
            int bottomLayerNodeCount = random.Next(config.minNodesPerLayer, config.maxNodesPerLayer + 1);
            bottomLayerNodeCount = Mathf.Min(bottomLayerNodeCount, config.width);

            List<int> usedColumns = new List<int>();

            for (int i = 0; i < bottomLayerNodeCount; i++)
            {
                int column = SelectColumnForNextLayer(usedColumns, config.width, random);
                usedColumns.Add(column);
                topology.CreateNode(0, column);
            }

            Debug.Log($"[TopologyGenerator] 生成了 {bottomLayerNodeCount} 个底层节点");
        }

        /// <summary>
        /// 生成指定层到上一层的连接
        /// </summary>
        private static void GenerateLayerConnections(MapTopology topology, int currentLayer, MapGenerationConfig config, System.Random random)
        {
            List<MapNode> currentLayerNodes = topology.GetNodesAtLayer(currentLayer);
            int nextLayer = currentLayer + 1;

            if (currentLayerNodes.Count == 0)
            {
                Debug.LogWarning($"[TopologyGenerator] 第{currentLayer}层没有节点,无法生成连接");
                return;
            }

            // 确定下一层的节点数量
            int nextLayerNodeCount = random.Next(config.minNodesPerLayer, config.maxNodesPerLayer + 1);
            nextLayerNodeCount = Mathf.Min(nextLayerNodeCount, config.width);

            // 生成下一层节点
            List<MapNode> nextLayerNodes = new List<MapNode>();
            List<int> usedColumns = new List<int>();

            for (int i = 0; i < nextLayerNodeCount; i++)
            {
                int column = SelectColumnForNextLayer(usedColumns, config.width, random);
                usedColumns.Add(column);
                MapNode nextNode = topology.CreateNode(nextLayer, column);
                nextLayerNodes.Add(nextNode);
            }

            // 为当前层的每个节点生成到下一层的连接
            foreach (MapNode currentNode in currentLayerNodes)
            {
                int outDegree = CalculateOutDegree(config, random);
                outDegree = Mathf.Max(1, outDegree); // 至少连接1个节点
                outDegree = Mathf.Min(outDegree, nextLayerNodes.Count); // 不超过下一层节点数

                // 选择要连接的下一层节点
                List<MapNode> targetNodes = SelectTargetNodes(
                    currentNode, 
                    nextLayerNodes, 
                    outDegree, 
                    config.connectionSpan, 
                    random
                );

                // 创建连接
                foreach (MapNode targetNode in targetNodes)
                {
                    topology.AddEdge(currentNode.NodeId, targetNode.NodeId);
                }
            }

            // 应用汇合逻辑: 确保某些节点被多个下层节点连接
            ApplyMergeLogic(topology, currentLayerNodes, nextLayerNodes, config, random);
        }

        /// <summary>
        /// 为下一层选择列位置
        /// </summary>
        private static int SelectColumnForNextLayer(List<int> usedColumns, int width, System.Random random)
        {
            List<int> availableColumns = Enumerable.Range(0, width)
                .Where(col => !usedColumns.Contains(col))
                .ToList();

            if (availableColumns.Count == 0)
            {
                // 如果所有列都被使用,随机选择一个
                return random.Next(width);
            }

            return availableColumns[random.Next(availableColumns.Count)];
        }

        /// <summary>
        /// 计算节点的出度
        /// </summary>
        private static int CalculateOutDegree(MapGenerationConfig config, System.Random random)
        {
            float baseDegree = config.avgOutDegree;
            float variance = (float)(random.NextDouble() * 2 - 1) * config.outDegreeVariance;
            float degree = baseDegree + variance;
            return Mathf.RoundToInt(degree);
        }

        /// <summary>
        /// 选择目标节点(考虑连接跨度)
        /// </summary>
        private static List<MapNode> SelectTargetNodes(
            MapNode sourceNode, 
            List<MapNode> candidates, 
            int count, 
            int connectionSpan, 
            System.Random random)
        {
            // 根据连接跨度筛选候选节点
            List<MapNode> validCandidates = candidates.Where(node =>
            {
                int columnDistance = Mathf.Abs(node.Column - sourceNode.Column);
                return columnDistance <= connectionSpan;
            }).ToList();

            // 如果有效候选节点不足,使用所有候选节点
            if (validCandidates.Count == 0)
            {
                validCandidates = new List<MapNode>(candidates);
            }

            // 随机选择
            List<MapNode> selected = new List<MapNode>();
            List<MapNode> available = new List<MapNode>(validCandidates);

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = random.Next(available.Count);
                selected.Add(available[index]);
                available.RemoveAt(index);
            }

            return selected;
        }

        /// <summary>
        /// 应用汇合逻辑
        /// </summary>
        private static void ApplyMergeLogic(
            MapTopology topology, 
            List<MapNode> currentLayerNodes, 
            List<MapNode> nextLayerNodes, 
            MapGenerationConfig config, 
            System.Random random)
        {
            if (nextLayerNodes.Count == 0 || currentLayerNodes.Count <= 1)
            {
                return;
            }

            // 根据汇合概率,让某些下一层节点被多个当前层节点连接
            foreach (MapNode nextNode in nextLayerNodes)
            {
                if (random.NextDouble() < config.mergeProbability)
                {
                    // 找到还没有连接到这个节点的当前层节点
                    List<MapNode> unconnectedNodes = currentLayerNodes
                        .Where(node => !node.UpperNeighbors.Contains(nextNode.NodeId))
                        .ToList();

                    if (unconnectedNodes.Count > 0)
                    {
                        // 随机选择一个未连接的节点进行连接
                        MapNode sourceNode = unconnectedNodes[random.Next(unconnectedNodes.Count)];
                        int columnDistance = Mathf.Abs(sourceNode.Column - nextNode.Column);
                        
                        // 检查连接跨度
                        if (columnDistance <= config.connectionSpan)
                        {
                            topology.AddEdge(sourceNode.NodeId, nextNode.NodeId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 生成顶层Boss节点
        /// 确保顶层只包含Boss节点
        /// </summary>
        private static void GenerateBossNode(MapTopology topology, MapGenerationConfig config, System.Random random)
        {
            int topLayer = config.height - 1;
            
            // 确保顶层没有任何节点(除了Boss)
            // 由于我们修改了生成逻辑，顶层不应该有节点，但为了安全起见检查一下
            List<MapNode> topLayerNodes = topology.GetNodesAtLayer(topLayer);
            if (topLayerNodes.Count > 0)
            {
                // 如果顶层已经有节点，删除它们(不应该发生，但为了安全)
                Debug.LogWarning($"[TopologyGenerator] 顶层已有 {topLayerNodes.Count} 个节点，将被删除，只保留Boss节点");
                foreach (var node in topLayerNodes.ToList())
                {
                    // 移除所有连接
                    foreach (int lowerId in node.LowerNeighbors.ToList())
                    {
                        MapNode lowerNode = topology.GetNode(lowerId);
                        if (lowerNode != null)
                        {
                            lowerNode.UpperNeighbors.Remove(node.NodeId);
                        }
                    }
                    // 从拓扑中移除节点
                    topology.Nodes.Remove(node.NodeId);
                    topology.NodesByLayer[topLayer].Remove(node);
                }
            }

            // 创建Boss节点
            int bossColumn = config.width / 2; // Boss节点放在中间位置
            MapNode bossNode = topology.CreateNode(topLayer, bossColumn);
            bossNode.IsBoss = true;
            topology.BossNodeId = bossNode.NodeId;

            // 连接到所有倒数第二层的节点
            int secondLastLayer = topLayer - 1;
            List<MapNode> secondLastNodes = topology.GetNodesAtLayer(secondLastLayer);
            foreach (MapNode node in secondLastNodes)
            {
                topology.AddEdge(node.NodeId, bossNode.NodeId);
            }

            Debug.Log($"[TopologyGenerator] Boss节点生成: Node[{topology.BossNodeId}] L{topLayer}C{bossColumn}");
        }

        /// <summary>
        /// 验证和修复拓扑结构
        /// </summary>
        private static void ValidateAndFix(MapTopology topology, MapGenerationConfig config, System.Random random)
        {
            int maxIterations = 100; // 防止无限循环
            int iterations = 0;

            while (iterations < maxIterations)
            {
                // 检查连通性
                bool isConnected = topology.CheckConnectivity();
                
                // 检查死节点
                List<int> deadEnds = topology.FindDeadEndNodes();
                
                // 获取所有底层节点
                List<MapNode> bottomLayerNodes = topology.GetNodesAtLayer(0);
                HashSet<int> reachableFromBottom = new HashSet<int>();
                if (bottomLayerNodes.Count > 0)
                {
                    foreach (MapNode bottomNode in bottomLayerNodes)
                    {
                        HashSet<int> reachable = GetReachableNodes(topology, bottomNode.NodeId);
                        reachableFromBottom.UnionWith(reachable);
                    }
                }
                
                // 找到所有不可达的节点（不包括Boss）
                List<int> unreachableNodeIds = topology.Nodes.Values
                    .Where(node => !node.IsBoss && !reachableFromBottom.Contains(node.NodeId))
                    .Select(node => node.NodeId)
                    .ToList();

                // 如果连通性正常且没有死节点和不可达节点，退出循环
                if (isConnected && deadEnds.Count == 0 && unreachableNodeIds.Count == 0)
                {
                    Debug.Log($"[TopologyGenerator] 地图验证通过，共检查 {iterations} 次");
                    break;
                }

                // 删除所有死节点和不可达节点
                List<int> nodesToDelete = new List<int>();
                nodesToDelete.AddRange(deadEnds);
                nodesToDelete.AddRange(unreachableNodeIds);
                nodesToDelete = nodesToDelete.Distinct().ToList();

                if (nodesToDelete.Count > 0)
                {
                    Debug.Log($"[TopologyGenerator] 删除 {nodesToDelete.Count} 个问题节点（死节点: {deadEnds.Count}, 不可达节点: {unreachableNodeIds.Count}）");
                    foreach (int nodeId in nodesToDelete)
                    {
                        topology.RemoveNode(nodeId);
                    }
                }

                // 修复连通性（如果删除节点后导致连通性问题）
                if (!topology.CheckConnectivity())
                {
                    Debug.LogWarning("[TopologyGenerator] 删除节点后连通性检查失败,尝试修复...");
                    FixConnectivity(topology, config, random);
                }

                iterations++;
            }

            if (iterations >= maxIterations)
            {
                Debug.LogError("[TopologyGenerator] 达到最大迭代次数，地图可能仍有问题");
            }

            // 最终验证
            if (!topology.CheckConnectivity())
            {
                Debug.LogError("[TopologyGenerator] 最终验证：连通性仍然失败");
            }

            // 最终检查死节点
            List<int> finalDeadEnds = topology.FindDeadEndNodes();
            if (finalDeadEnds.Count > 0)
            {
                Debug.LogError($"[TopologyGenerator] 最终验证：仍有 {finalDeadEnds.Count} 个死节点");
            }
        }

        /// <summary>
        /// 修复连通性问题
        /// </summary>
        private static void FixConnectivity(MapTopology topology, MapGenerationConfig config, System.Random random)
        {
            // 确保所有底层节点都能到达Boss
            int topLayer = config.height - 1;
            List<MapNode> topLayerNodes = topology.GetNodesAtLayer(topLayer);
            MapNode bossNode = topology.GetNode(topology.BossNodeId);

            if (bossNode == null || topLayerNodes.Count == 0)
            {
                return;
            }

            // 获取底层(第0层)的所有节点
            List<MapNode> bottomLayerNodes = topology.GetNodesAtLayer(0);
            if (bottomLayerNodes.Count == 0)
            {
                return;
            }

            // 找到所有无法到达Boss的底层节点,并建立连接
            foreach (MapNode bottomNode in bottomLayerNodes)
            {
                HashSet<int> reachable = GetReachableNodes(topology, bottomNode.NodeId);
                if (!reachable.Contains(topology.BossNodeId))
                {
                    // 找到最接近Boss的可达节点，且符合连接跨度限制
                    MapNode closestNode = FindClosestReachableNode(topology, reachable, bossNode, config.connectionSpan);
                    if (closestNode != null && closestNode.Layer < bossNode.Layer)
                    {
                        // 检查连接跨度（Boss节点特殊处理，如果不符合跨度也允许连接以确保连通性）
                        int columnDistance = Mathf.Abs(closestNode.Column - bossNode.Column);
                        if (columnDistance <= config.connectionSpan || bossNode.Layer - closestNode.Layer == 1)
                        {
                            topology.AddEdge(closestNode.NodeId, bossNode.NodeId);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 获取从指定节点可达的所有节点
        /// </summary>
        private static HashSet<int> GetReachableNodes(MapTopology topology, int startId)
        {
            HashSet<int> reachable = new HashSet<int>();
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(startId);
            reachable.Add(startId);

            while (queue.Count > 0)
            {
                int currentId = queue.Dequeue();
                MapNode currentNode = topology.GetNode(currentId);

                foreach (int neighborId in currentNode.UpperNeighbors)
                {
                    if (!reachable.Contains(neighborId))
                    {
                        reachable.Add(neighborId);
                        queue.Enqueue(neighborId);
                    }
                }
            }

            return reachable;
        }

        /// <summary>
        /// 找到最接近目标节点的可达节点（优先选择符合连接跨度限制的节点）
        /// </summary>
        private static MapNode FindClosestReachableNode(MapTopology topology, HashSet<int> reachable, MapNode target, int connectionSpan)
        {
            MapNode closestWithinSpan = null;
            MapNode closestOverall = null;
            int minDistanceWithinSpan = int.MaxValue;
            int minDistanceOverall = int.MaxValue;

            foreach (int nodeId in reachable)
            {
                MapNode node = topology.GetNode(nodeId);
                if (node == null || node.Layer >= target.Layer)
                {
                    continue;
                }

                int columnDistance = Mathf.Abs(node.Column - target.Column);
                int layerDistance = target.Layer - node.Layer;
                int totalDistance = columnDistance + layerDistance * 10; // 层数距离权重更大

                // 检查是否符合连接跨度限制
                bool withinSpan = columnDistance <= connectionSpan;

                if (withinSpan && totalDistance < minDistanceWithinSpan)
                {
                    minDistanceWithinSpan = totalDistance;
                    closestWithinSpan = node;
                }

                if (totalDistance < minDistanceOverall)
                {
                    minDistanceOverall = totalDistance;
                    closestOverall = node;
                }
            }

            // 优先返回符合连接跨度限制的节点，如果没有则返回最近的节点
            return closestWithinSpan ?? closestOverall;
        }

        /// <summary>
        /// 检查路径多样性（每个底层节点到Boss至少要有2条路径）
        /// </summary>
        private static bool CheckPathDiversity(MapTopology topology)
        {
            if (topology.BossNodeId == -1)
            {
                return false;
            }

            // 获取所有底层节点
            List<MapNode> bottomLayerNodes = topology.GetNodesAtLayer(0);
            if (bottomLayerNodes.Count == 0)
            {
                return false;
            }

            // 检查每个底层节点到Boss的路径数量
            foreach (MapNode bottomNode in bottomLayerNodes)
            {
                List<List<int>> paths = topology.GetPathsFromNodeToBoss(bottomNode.NodeId);
                if (paths.Count < 2)
                {
                    Debug.LogWarning($"[TopologyGenerator] 底层节点 {bottomNode.NodeId} 到Boss只有 {paths.Count} 条路径（需要至少2条）");
                    return false;
                }
            }

            return true;
        }

    }
}

