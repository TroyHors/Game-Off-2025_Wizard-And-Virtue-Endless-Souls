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
            System.Random random = seed >= 0 ? new System.Random(seed) : new System.Random();

            MapTopology topology = new MapTopology(config.height, config.width);

            // 阶段1: 生成底层节点
            GenerateBottomLayer(topology, config, random);

            // 阶段2: 从下往上逐层生成节点和连接
            for (int layer = 0; layer < config.height - 1; layer++)
            {
                GenerateLayerConnections(topology, layer, config, random);
            }

            // 阶段3: 生成顶层Boss节点
            GenerateBossNode(topology, config, random);

            // 阶段4: 验证和修复
            ValidateAndFix(topology, config, random);

            // 阶段5: 优化节点列位置，最小化连接距离
            OptimizeNodeColumns(topology, config);

            Debug.Log($"[TopologyGenerator] 拓扑生成完成 {topology.GetStatistics()}");
            return topology;
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
        /// </summary>
        private static void GenerateBossNode(MapTopology topology, MapGenerationConfig config, System.Random random)
        {
            int topLayer = config.height - 1;
            List<MapNode> topLayerNodes = topology.GetNodesAtLayer(topLayer);

            if (topLayerNodes.Count == 0)
            {
                // 如果顶层没有节点,创建一个Boss节点
                int bossColumn = random.Next(config.width);
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
            }
            else
            {
                // 将顶层的一个节点标记为Boss(选择中间位置的节点)
                MapNode bossNode = topLayerNodes.OrderBy(n => Mathf.Abs(n.Column - config.width / 2)).First();
                bossNode.IsBoss = true;
                topology.BossNodeId = bossNode.NodeId;
            }

            Debug.Log($"[TopologyGenerator] Boss节点生成: Node[{topology.BossNodeId}]");
        }

        /// <summary>
        /// 验证和修复拓扑结构
        /// </summary>
        private static void ValidateAndFix(MapTopology topology, MapGenerationConfig config, System.Random random)
        {
            int maxFixAttempts = 5;
            int attempts = 0;

            while (attempts < maxFixAttempts)
            {
                // 检查连通性
                bool isConnected = topology.CheckConnectivity();
                
                // 检查死节点和不可达节点
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
                
                List<MapNode> unreachableNodes = topology.Nodes.Values
                    .Where(node => !node.IsBoss && !reachableFromBottom.Contains(node.NodeId))
                    .ToList();

                // 如果连通性正常且没有死节点和不可达节点，退出循环
                if (isConnected && deadEnds.Count == 0 && unreachableNodes.Count == 0)
                {
                    break;
                }

                // 修复连通性
                if (!isConnected)
                {
                    Debug.LogWarning("[TopologyGenerator] 连通性检查失败,尝试修复...");
                    FixConnectivity(topology, config, random);
                }

                // 修复死节点和不可达节点
                if (deadEnds.Count > 0 || unreachableNodes.Count > 0)
                {
                    Debug.LogWarning($"[TopologyGenerator] 发现 {deadEnds.Count} 个死节点, {unreachableNodes.Count} 个不可达节点,尝试修复...");
                    FixDeadEnds(topology, deadEnds, config, random);
                }

                attempts++;
            }

            // 最终验证
            if (!topology.CheckConnectivity())
            {
                Debug.LogError("[TopologyGenerator] 修复后连通性仍然失败");
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
                    // 找到最接近Boss的可达节点
                    MapNode closestNode = FindClosestReachableNode(topology, reachable, bossNode);
                    if (closestNode != null && closestNode.Layer < bossNode.Layer)
                    {
                        topology.AddEdge(closestNode.NodeId, bossNode.NodeId);
                    }
                }
            }
        }

        /// <summary>
        /// 修复死节点和不可达节点
        /// </summary>
        private static void FixDeadEnds(MapTopology topology, List<int> deadEndIds, MapGenerationConfig config, System.Random random)
        {
            // 获取所有底层节点
            List<MapNode> bottomLayerNodes = topology.GetNodesAtLayer(0);
            if (bottomLayerNodes.Count == 0)
            {
                return;
            }

            // 计算从所有底层节点可达的节点集合
            HashSet<int> reachableFromBottom = new HashSet<int>();
            foreach (MapNode bottomNode in bottomLayerNodes)
            {
                HashSet<int> reachable = GetReachableNodes(topology, bottomNode.NodeId);
                reachableFromBottom.UnionWith(reachable);
            }

            // 找到所有不可达的节点
            List<MapNode> unreachableNodes = topology.Nodes.Values
                .Where(node => !node.IsBoss && !reachableFromBottom.Contains(node.NodeId))
                .ToList();

            // 修复死节点和不可达节点
            List<int> allProblemNodes = new List<int>(deadEndIds);
            allProblemNodes.AddRange(unreachableNodes.Select(n => n.NodeId));
            allProblemNodes = allProblemNodes.Distinct().ToList();

            int topLayer = config.height - 1;

            foreach (int problemNodeId in allProblemNodes)
            {
                MapNode problemNode = topology.GetNode(problemNodeId);
                if (problemNode == null)
                {
                    continue;
                }

                // 如果是死节点(没有向上连接)，先修复向上连接
                if (problemNode.OutDegree == 0 && problemNode.Layer < topLayer)
                {
                    int nextLayer = problemNode.Layer + 1;
                    List<MapNode> nextLayerNodes = topology.GetNodesAtLayer(nextLayer);
                    if (nextLayerNodes.Count > 0)
                    {
                        // 随机选择一个上层节点连接
                        MapNode targetNode = nextLayerNodes[random.Next(nextLayerNodes.Count)];
                        topology.AddEdge(problemNodeId, targetNode.NodeId);
                    }
                }

                // 如果节点不可达，随机连接到下层节点，确保能被到达
                if (!reachableFromBottom.Contains(problemNodeId) && problemNode.Layer > 0)
                {
                    int prevLayer = problemNode.Layer - 1;
                    List<MapNode> prevLayerNodes = topology.GetNodesAtLayer(prevLayer);
                    
                    if (prevLayerNodes.Count > 0)
                    {
                        // 随机选择一个下层节点，让它连接到当前节点
                        MapNode sourceNode = prevLayerNodes[random.Next(prevLayerNodes.Count)];
                        topology.AddEdge(sourceNode.NodeId, problemNodeId);
                        
                        // 更新可达集合
                        reachableFromBottom.Add(problemNodeId);
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
        /// 找到最接近目标节点的可达节点
        /// </summary>
        private static MapNode FindClosestReachableNode(MapTopology topology, HashSet<int> reachable, MapNode target)
        {
            MapNode closest = null;
            int minDistance = int.MaxValue;

            foreach (int nodeId in reachable)
            {
                MapNode node = topology.GetNode(nodeId);
                if (node == null || node.Layer >= target.Layer)
                {
                    continue;
                }

                int distance = Mathf.Abs(node.Column - target.Column) + (target.Layer - node.Layer);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = node;
                }
            }

            return closest;
        }

        /// <summary>
        /// 优化节点列位置，最小化相邻两层之间连接的列值差绝对值之和
        /// </summary>
        private static void OptimizeNodeColumns(MapTopology topology, MapGenerationConfig config)
        {
            // 从最底层往上，依次对每两层进行优化
            for (int layer = 0; layer < config.height - 1; layer++)
            {
                OptimizeTwoLayers(topology, layer, layer + 1, config);
            }
        }

        /// <summary>
        /// 优化相邻两层的节点列位置
        /// 使用回溯算法找到全局最优解
        /// </summary>
        private static void OptimizeTwoLayers(MapTopology topology, int lowerLayer, int upperLayer, MapGenerationConfig config)
        {
            List<MapNode> lowerNodes = topology.GetNodesAtLayer(lowerLayer);
            List<MapNode> upperNodes = topology.GetNodesAtLayer(upperLayer);

            if (lowerNodes.Count == 0 || upperNodes.Count == 0)
            {
                return;
            }

            // 计算当前连接的列值差绝对值之和
            int currentTotalDistance = CalculateTotalColumnDistance(topology, lowerNodes, upperNodes);

            // 如果上层节点数太多，给出警告但继续尝试（可能较慢）
            const int WARNING_THRESHOLD = 10;
            if (upperNodes.Count > WARNING_THRESHOLD)
            {
                Debug.LogWarning($"[TopologyGenerator] 上层节点数较多({upperNodes.Count})，优化可能较慢");
            }

            // 获取下层节点已使用的列位置
            Dictionary<int, int> lowerNodeColumns = new Dictionary<int, int>();
            HashSet<int> usedColumns = new HashSet<int>();
            foreach (var lowerNode in lowerNodes)
            {
                lowerNodeColumns[lowerNode.NodeId] = lowerNode.Column;
                usedColumns.Add(lowerNode.Column);
            }

            // 使用回溯算法找到最优排列
            Dictionary<int, int> bestAssignment = new Dictionary<int, int>();
            int minTotalDistance = int.MaxValue;

            // 为每个上层节点准备可用的列位置
            List<int> availableColumns = Enumerable.Range(0, config.width).ToList();
            
            // 使用回溯算法尝试所有可能的排列
            BacktrackOptimize(upperNodes, 0, availableColumns, usedColumns, lowerNodeColumns, topology, 
                lowerLayer, new Dictionary<int, int>(), ref bestAssignment, ref minTotalDistance);

            // 应用最优解
            if (bestAssignment.Count > 0 && minTotalDistance < currentTotalDistance)
            {
                foreach (var upperNode in upperNodes)
                {
                    if (bestAssignment.ContainsKey(upperNode.NodeId))
                    {
                        upperNode.Column = bestAssignment[upperNode.NodeId];
                    }
                }

                Debug.Log($"[TopologyGenerator] 优化层 {lowerLayer}-{upperLayer}: 距离从 {currentTotalDistance} 减少到 {minTotalDistance}");
            }
            else if (bestAssignment.Count > 0)
            {
                Debug.Log($"[TopologyGenerator] 层 {lowerLayer}-{upperLayer}: 当前已是最优解，距离为 {currentTotalDistance}");
            }
        }

        /// <summary>
        /// 回溯算法：尝试所有可能的列位置分配，找到全局最优解
        /// </summary>
        private static void BacktrackOptimize(
            List<MapNode> upperNodes,
            int nodeIndex,
            List<int> availableColumns,
            HashSet<int> usedColumns,
            Dictionary<int, int> lowerNodeColumns,
            MapTopology topology,
            int lowerLayer,
            Dictionary<int, int> currentAssignment,
            ref Dictionary<int, int> bestAssignment,
            ref int minTotalDistance)
        {
            // 如果已经处理完所有节点，计算总距离
            if (nodeIndex >= upperNodes.Count)
            {
                int totalDistance = CalculateTotalDistanceWithAssignment(topology, lowerNodeColumns, currentAssignment, upperNodes, lowerLayer);
                if (totalDistance < minTotalDistance)
                {
                    minTotalDistance = totalDistance;
                    bestAssignment = new Dictionary<int, int>(currentAssignment);
                }
                return;
            }

            MapNode currentNode = upperNodes[nodeIndex];

            // 获取该节点的所有下层连接节点
            List<MapNode> connectedLowerNodes = currentNode.LowerNeighbors
                .Select(id => topology.GetNode(id))
                .Where(n => n != null && n.Layer == lowerLayer)
                .ToList();

            // 计算每个列位置的候选值，按距离排序（剪枝优化）
            List<(int column, int distance)> columnDistances = new List<(int, int)>();
            
            foreach (int col in availableColumns)
            {
                int distance = 0;
                if (connectedLowerNodes.Count > 0)
                {
                    // 计算该列位置与所有连接的下层节点的距离
                    foreach (var lowerNode in connectedLowerNodes)
                    {
                        if (lowerNodeColumns.ContainsKey(lowerNode.NodeId))
                        {
                            distance += Mathf.Abs(col - lowerNodeColumns[lowerNode.NodeId]);
                        }
                    }
                }
                columnDistances.Add((col, distance));
            }

            // 按距离排序，优先尝试距离小的列（剪枝优化）
            columnDistances = columnDistances.OrderBy(x => x.distance).ToList();

            // 尝试每个候选列位置
            foreach (var (col, distance) in columnDistances)
            {
                // 计算当前部分分配的最小可能距离（剪枝）
                int currentPartialDistance = CalculatePartialDistance(topology, lowerNodeColumns, currentAssignment, 
                    currentNode, col, connectedLowerNodes);
                
                // 如果当前部分距离已经大于等于已知最优解，剪枝
                if (currentPartialDistance >= minTotalDistance)
                {
                    continue;
                }

                // 检查是否可以使用该列（允许重复使用列，因为可能有多个节点）
                bool columnWasUsed = usedColumns.Contains(col);
                
                // 分配列位置
                currentAssignment[currentNode.NodeId] = col;
                if (!columnWasUsed)
                {
                    usedColumns.Add(col);
                }

                // 递归处理下一个节点
                BacktrackOptimize(upperNodes, nodeIndex + 1, availableColumns, usedColumns, 
                    lowerNodeColumns, topology, lowerLayer, currentAssignment, 
                    ref bestAssignment, ref minTotalDistance);

                // 回溯：恢复状态
                currentAssignment.Remove(currentNode.NodeId);
                if (!columnWasUsed)
                {
                    usedColumns.Remove(col);
                }

                // 如果已经找到最优解（距离为0），可以提前退出
                if (minTotalDistance == 0)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 计算部分分配的距离（用于剪枝）
        /// </summary>
        private static int CalculatePartialDistance(
            MapTopology topology,
            Dictionary<int, int> lowerNodeColumns,
            Dictionary<int, int> currentAssignment,
            MapNode currentNode,
            int candidateColumn,
            List<MapNode> connectedLowerNodes)
        {
            int distance = 0;

            // 计算当前节点与连接的下层节点的距离
            foreach (var lowerNode in connectedLowerNodes)
            {
                if (lowerNodeColumns.ContainsKey(lowerNode.NodeId))
                {
                    distance += Mathf.Abs(candidateColumn - lowerNodeColumns[lowerNode.NodeId]);
                }
            }

            // 计算已分配节点与下层节点的距离
            foreach (var kvp in currentAssignment)
            {
                MapNode assignedNode = topology.GetNode(kvp.Key);
                if (assignedNode == null)
                {
                    continue;
                }

                foreach (int lowerNeighborId in assignedNode.LowerNeighbors)
                {
                    if (lowerNodeColumns.ContainsKey(lowerNeighborId))
                    {
                        distance += Mathf.Abs(kvp.Value - lowerNodeColumns[lowerNeighborId]);
                    }
                }
            }

            return distance;
        }

        /// <summary>
        /// 计算给定分配方案下的总距离
        /// </summary>
        private static int CalculateTotalDistanceWithAssignment(
            MapTopology topology,
            Dictionary<int, int> lowerNodeColumns,
            Dictionary<int, int> upperNodeAssignment,
            List<MapNode> upperNodes,
            int lowerLayer)
        {
            int totalDistance = 0;

            foreach (var lowerNodeId in lowerNodeColumns.Keys)
            {
                MapNode lowerNode = topology.GetNode(lowerNodeId);
                if (lowerNode == null)
                {
                    continue;
                }

                int lowerCol = lowerNodeColumns[lowerNodeId];

                foreach (int upperNeighborId in lowerNode.UpperNeighbors)
                {
                    if (upperNodeAssignment.ContainsKey(upperNeighborId))
                    {
                        int upperCol = upperNodeAssignment[upperNeighborId];
                        totalDistance += Mathf.Abs(lowerCol - upperCol);
                    }
                }
            }

            return totalDistance;
        }

        /// <summary>
        /// 计算两层之间所有连接的列值差绝对值之和
        /// </summary>
        private static int CalculateTotalColumnDistance(MapTopology topology, List<MapNode> lowerNodes, List<MapNode> upperNodes)
        {
            int totalDistance = 0;

            foreach (var lowerNode in lowerNodes)
            {
                foreach (int upperNeighborId in lowerNode.UpperNeighbors)
                {
                    MapNode upperNode = topology.GetNode(upperNeighborId);
                    if (upperNode != null && upperNodes.Contains(upperNode))
                    {
                        totalDistance += Mathf.Abs(lowerNode.Column - upperNode.Column);
                    }
                }
            }

            return totalDistance;
        }

        /// <summary>
        /// 找到最优列位置，使得与所有连接节点的列值差绝对值之和最小
        /// </summary>
        private static int FindOptimalColumn(List<MapNode> connectedNodes, Dictionary<int, int> nodeToColumn, List<int> usedColumns, int maxWidth)
        {
            if (connectedNodes.Count == 0)
            {
                return FindUnusedColumn(usedColumns, maxWidth);
            }

            // 获取所有连接节点的列位置
            List<int> connectedColumns = connectedNodes
                .Where(n => nodeToColumn.ContainsKey(n.NodeId))
                .Select(n => nodeToColumn[n.NodeId])
                .ToList();

            if (connectedColumns.Count == 0)
            {
                return FindUnusedColumn(usedColumns, maxWidth);
            }

            // 计算所有可能的列位置的总距离
            int bestColumn = 0;
            int minTotalDistance = int.MaxValue;

            // 遍历所有可能的列位置
            for (int col = 0; col < maxWidth; col++)
            {
                // 如果该列已被使用，跳过
                if (usedColumns.Contains(col))
                {
                    continue;
                }

                // 计算该位置的总距离
                int totalDistance = 0;
                foreach (int connectedCol in connectedColumns)
                {
                    totalDistance += Mathf.Abs(col - connectedCol);
                }

                if (totalDistance < minTotalDistance)
                {
                    minTotalDistance = totalDistance;
                    bestColumn = col;
                }
            }

            // 如果所有列都被使用，选择距离最小的已使用列
            if (minTotalDistance == int.MaxValue)
            {
                // 所有列都被使用，选择距离最小的列
                for (int col = 0; col < maxWidth; col++)
                {
                    int totalDistance = 0;
                    foreach (int connectedCol in connectedColumns)
                    {
                        totalDistance += Mathf.Abs(col - connectedCol);
                    }

                    if (totalDistance < minTotalDistance)
                    {
                        minTotalDistance = totalDistance;
                        bestColumn = col;
                    }
                }
            }

            return bestColumn;
        }

        /// <summary>
        /// 找到一个未使用的列位置
        /// </summary>
        private static int FindUnusedColumn(List<int> usedColumns, int maxWidth)
        {
            for (int col = 0; col < maxWidth; col++)
            {
                if (!usedColumns.Contains(col))
                {
                    return col;
                }
            }

            // 如果所有列都被使用，返回第一个列
            return 0;
        }
    }
}

