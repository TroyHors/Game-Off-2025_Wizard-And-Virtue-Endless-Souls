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

            int maxRegenerationAttempts = 10;
            int regenerationAttempts = 0;

            while (regenerationAttempts < maxRegenerationAttempts)
            {
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

                // 阶段5: 地图验证检查
                if (ValidateMap(topology, config))
                {
                    Debug.Log($"[TopologyGenerator] 拓扑生成完成 {topology.GetStatistics()}");
                    return topology;
                }
                else
                {
                    regenerationAttempts++;
                    Debug.LogWarning($"[TopologyGenerator] 地图验证失败，重新生成 (尝试 {regenerationAttempts}/{maxRegenerationAttempts})");
                    // 使用新的随机种子重新生成
                    random = new System.Random(random.Next());
                }
            }

            Debug.LogError("[TopologyGenerator] 达到最大重生成次数，返回最后一次生成的地图");
            // 即使验证失败，也返回最后一次生成的地图
            MapTopology finalTopology = new MapTopology(config.height, config.width);
            GenerateBottomLayer(finalTopology, config, random);
            for (int layer = 0; layer < config.height - 1; layer++)
            {
                GenerateLayerConnections(finalTopology, layer, config, random);
            }
            GenerateBossNode(finalTopology, config, random);
            ValidateAndFix(finalTopology, config, random);
            return finalTopology;
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

            // 按column值从小到大排序当前层节点
            currentLayerNodes = currentLayerNodes.OrderBy(n => n.Column).ToList();
            // 按column值排序下一层节点
            nextLayerNodes = nextLayerNodes.OrderBy(n => n.Column).ToList();

            // 记录前一个节点连接的column最大值
            int previousMaxColumn = -1;

            // 按顺序为当前层的每个节点生成到下一层的连接
            for (int i = 0; i < currentLayerNodes.Count; i++)
            {
                MapNode currentNode = currentLayerNodes[i];
                
                int outDegree = CalculateOutDegree(config, random);
                outDegree = Mathf.Max(1, outDegree); // 至少连接1个节点
                outDegree = Mathf.Min(outDegree, nextLayerNodes.Count); // 不超过下一层节点数

                // 选择要连接的下一层节点(遵循新的连接规则)
                List<MapNode> targetNodes = SelectTargetNodesWithRules(
                    currentNode, 
                    nextLayerNodes, 
                    outDegree, 
                    previousMaxColumn,
                    random
                );

                // 创建连接
                int currentMaxColumn = -1;
                foreach (MapNode targetNode in targetNodes)
                {
                    topology.AddEdge(currentNode.NodeId, targetNode.NodeId);
                    // 更新当前节点连接的column最大值
                    if (targetNode.Column > currentMaxColumn)
                    {
                        currentMaxColumn = targetNode.Column;
                    }
                }

                // 更新previousMaxColumn为当前节点连接的column最大值
                if (currentMaxColumn >= 0)
                {
                    previousMaxColumn = currentMaxColumn;
                }
            }
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
        /// 选择目标节点(遵循新的连接规则)
        /// 规则1: column差绝对值为1之内的node
        /// 规则2: 连接的column值 >= 前一个节点连接的column最大值
        /// </summary>
        private static List<MapNode> SelectTargetNodesWithRules(
            MapNode sourceNode, 
            List<MapNode> candidates, 
            int count, 
            int previousMaxColumn,
            System.Random random)
        {
            // 规则1: column差绝对值为1之内的node
            List<MapNode> validCandidates = candidates.Where(node =>
            {
                int columnDistance = Mathf.Abs(node.Column - sourceNode.Column);
                return columnDistance <= 1;
            }).ToList();

            // 规则2: 连接的column值 >= 前一个节点连接的column最大值
            if (previousMaxColumn >= 0)
            {
                validCandidates = validCandidates.Where(node => node.Column >= previousMaxColumn).ToList();
            }

            // 如果有效候选节点不足,至少保证有一个连接
            if (validCandidates.Count == 0)
            {
                // 如果规则1无法满足，放宽到column差<=1
                validCandidates = candidates.Where(node =>
                {
                    int columnDistance = Mathf.Abs(node.Column - sourceNode.Column);
                    return columnDistance <= 1;
                }).ToList();
                
                // 如果还是为空，选择最近的节点
                if (validCandidates.Count == 0)
                {
                    validCandidates = candidates.OrderBy(node => Mathf.Abs(node.Column - sourceNode.Column)).Take(1).ToList();
                }
            }

            // 从有效候选节点中选择
            List<MapNode> selected = new List<MapNode>();
            List<MapNode> available = new List<MapNode>(validCandidates);

            // 至少选择一个节点
            int selectCount = Mathf.Min(count, available.Count);
            for (int i = 0; i < selectCount && available.Count > 0; i++)
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
        /// 确保最高层只包含一个Boss节点
        /// </summary>
        private static void GenerateBossNode(MapTopology topology, MapGenerationConfig config, System.Random random)
        {
            int topLayer = config.height - 1;
            List<MapNode> topLayerNodes = topology.GetNodesAtLayer(topLayer);

            MapNode bossNode;

            if (topLayerNodes.Count == 0)
            {
                // 如果顶层没有节点,创建一个Boss节点
                int bossColumn = random.Next(config.width);
                bossNode = topology.CreateNode(topLayer, bossColumn);
            }
            else if (topLayerNodes.Count == 1)
            {
                // 如果顶层只有一个节点,将其标记为Boss
                bossNode = topLayerNodes[0];
            }
            else
            {
                // 如果顶层有多个节点,选择中间位置的节点作为Boss,删除其他节点
                bossNode = topLayerNodes.OrderBy(n => Mathf.Abs(n.Column - config.width / 2)).First();
                
                // 删除其他顶层节点
                foreach (MapNode node in topLayerNodes)
                {
                    if (node.NodeId != bossNode.NodeId)
                    {
                        RemoveNode(topology, node.NodeId);
                    }
                }
            }

            // 标记为Boss节点
            bossNode.IsBoss = true;
            topology.BossNodeId = bossNode.NodeId;

            // 确保所有倒数第二层的节点都连接到Boss节点
            int secondLastLayer = topLayer - 1;
            List<MapNode> secondLastNodes = topology.GetNodesAtLayer(secondLastLayer);
            foreach (MapNode node in secondLastNodes)
            {
                // 如果还没有连接,则添加连接
                if (!node.UpperNeighbors.Contains(bossNode.NodeId))
                {
                    topology.AddEdge(node.NodeId, bossNode.NodeId);
                }
            }

            Debug.Log($"[TopologyGenerator] Boss节点生成: Node[{topology.BossNodeId}]");
        }

        /// <summary>
        /// 验证和修复拓扑结构
        /// </summary>
        private static void ValidateAndFix(MapTopology topology, MapGenerationConfig config, System.Random random)
        {
            int maxFixAttempts = 10; // 增加最大尝试次数
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
                    Debug.Log("[TopologyGenerator] 地图验证通过，所有节点都有效");
                    break;
                }

                // 修复连通性
                if (!isConnected)
                {
                    Debug.LogWarning("[TopologyGenerator] 连通性检查失败,尝试修复...");
                    FixConnectivity(topology, config, random);
                }

                // 修复死节点和不可达节点(循环修复直到地图有效)
                if (deadEnds.Count > 0 || unreachableNodes.Count > 0)
                {
                    Debug.LogWarning($"[TopologyGenerator] 发现 {deadEnds.Count} 个死节点, {unreachableNodes.Count} 个不可达节点,开始循环修复...");
                    bool stillHasProblems = FixDeadEnds(topology, deadEnds, config, random);
                    
                    // FixDeadEnds内部已经循环修复，如果返回true说明还有问题，继续外层循环
                    if (stillHasProblems)
                    {
                        attempts++;
                        continue; // 继续循环，重新检查
                    }
                }

                attempts++;
            }

            // 确保顶层只有Boss节点
            EnsureTopLayerOnlyBoss(topology, config);

            // 最终验证
            bool finalConnected = topology.CheckConnectivity();
            List<int> finalDeadEnds = topology.FindDeadEndNodes();
            List<MapNode> finalBottomNodes = topology.GetNodesAtLayer(0);
            HashSet<int> finalReachable = new HashSet<int>();
            if (finalBottomNodes.Count > 0)
            {
                foreach (MapNode bottomNode in finalBottomNodes)
                {
                    HashSet<int> reachable = GetReachableNodes(topology, bottomNode.NodeId);
                    finalReachable.UnionWith(reachable);
                }
            }
            List<MapNode> finalUnreachable = topology.Nodes.Values
                .Where(node => !node.IsBoss && !finalReachable.Contains(node.NodeId))
                .ToList();

            if (!finalConnected)
            {
                Debug.LogError("[TopologyGenerator] 修复后连通性仍然失败");
            }
            if (finalDeadEnds.Count > 0 || finalUnreachable.Count > 0)
            {
                Debug.LogError($"[TopologyGenerator] 修复后仍有 {finalDeadEnds.Count} 个死节点和 {finalUnreachable.Count} 个不可达节点");
            }
        }

        /// <summary>
        /// 验证地图是否符合要求
        /// </summary>
        /// <returns>是否通过验证</returns>
        private static bool ValidateMap(MapTopology topology, MapGenerationConfig config)
        {
            // 检查1: 是否有超过连续三层（包括三层）只有一个node
            if (!ValidateLayerNodeCount(topology, config))
            {
                return false;
            }

            // 待添加其他验证规则

            return true;
        }

        /// <summary>
        /// 验证每层节点数量
        /// 检查是否有连续三层（包括三层）只有一个节点
        /// </summary>
        private static bool ValidateLayerNodeCount(MapTopology topology, MapGenerationConfig config)
        {
            int consecutiveSingleNodeLayers = 0;
            int maxConsecutiveSingleNodeLayers = 0;

            for (int layer = 0; layer < config.height; layer++)
            {
                List<MapNode> layerNodes = topology.GetNodesAtLayer(layer);
                int nodeCount = layerNodes.Count;

                if (nodeCount == 1)
                {
                    consecutiveSingleNodeLayers++;
                    maxConsecutiveSingleNodeLayers = Mathf.Max(maxConsecutiveSingleNodeLayers, consecutiveSingleNodeLayers);
                }
                else
                {
                    consecutiveSingleNodeLayers = 0;
                }
            }

            // 如果连续三层或以上只有一个节点，验证失败
            if (maxConsecutiveSingleNodeLayers >= 3)
            {
                Debug.LogWarning($"[TopologyGenerator] 地图验证失败: 有连续 {maxConsecutiveSingleNodeLayers} 层只有一个节点");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 确保顶层只有Boss节点
        /// </summary>
        private static void EnsureTopLayerOnlyBoss(MapTopology topology, MapGenerationConfig config)
        {
            int topLayer = config.height - 1;
            List<MapNode> topLayerNodes = topology.GetNodesAtLayer(topLayer);

            if (topLayerNodes.Count == 0)
            {
                Debug.LogWarning("[TopologyGenerator] 顶层没有节点，这不应该发生");
                return;
            }

            // 找到Boss节点
            MapNode bossNode = topLayerNodes.FirstOrDefault(n => n.IsBoss);
            
            if (bossNode == null)
            {
                // 如果没有Boss节点，将第一个节点标记为Boss
                bossNode = topLayerNodes[0];
                bossNode.IsBoss = true;
                topology.BossNodeId = bossNode.NodeId;
                Debug.LogWarning("[TopologyGenerator] 顶层没有Boss节点，已将第一个节点标记为Boss");
            }

            // 删除所有非Boss节点
            foreach (MapNode node in topLayerNodes)
            {
                if (node.NodeId != bossNode.NodeId)
                {
                    RemoveNode(topology, node.NodeId);
                }
            }

            // 确保所有倒数第二层的节点都连接到Boss节点
            int secondLastLayer = topLayer - 1;
            List<MapNode> secondLastNodes = topology.GetNodesAtLayer(secondLastLayer);
            foreach (MapNode node in secondLastNodes)
            {
                if (!node.UpperNeighbors.Contains(bossNode.NodeId))
                {
                    topology.AddEdge(node.NodeId, bossNode.NodeId);
                }
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
        /// 循环修复直到整个地图都有效
        /// </summary>
        /// <returns>是否还有问题节点需要修复</returns>
        private static bool FixDeadEnds(MapTopology topology, List<int> deadEndIds, MapGenerationConfig config, System.Random random)
        {
            int maxIterations = 50; // 防止无限循环
            int iterations = 0;

            while (iterations < maxIterations)
            {
                // 获取所有底层节点
                List<MapNode> bottomLayerNodes = topology.GetNodesAtLayer(0);
                if (bottomLayerNodes.Count == 0)
                {
                    Debug.LogError("[TopologyGenerator] 没有底层节点，无法修复");
                    return true; // 返回true表示有问题
                }

                // 确保Boss节点存在
                if (topology.BossNodeId == -1)
                {
                    Debug.LogError("[TopologyGenerator] Boss节点不存在，无法修复");
                    return true; // 返回true表示有问题
                }

                MapNode bossNode = topology.GetNode(topology.BossNodeId);
                if (bossNode == null)
                {
                    Debug.LogError("[TopologyGenerator] Boss节点已被删除，无法修复");
                    return true; // 返回true表示有问题
                }

                // 计算从所有底层节点可达的节点集合
                HashSet<int> reachableFromBottom = new HashSet<int>();
                foreach (MapNode bottomNode in bottomLayerNodes)
                {
                    HashSet<int> reachable = GetReachableNodes(topology, bottomNode.NodeId);
                    reachableFromBottom.UnionWith(reachable);
                }

                // 检查Boss节点是否可达
                bool bossReachable = reachableFromBottom.Contains(topology.BossNodeId);
                
                // 如果Boss节点不可达，先尝试修复连通性
                if (!bossReachable)
                {
                    Debug.LogWarning("[TopologyGenerator] Boss节点不可达，尝试修复连通性");
                    FixConnectivity(topology, config, random);
                    // 重新计算可达性
                    reachableFromBottom.Clear();
                    foreach (MapNode bottomNode in bottomLayerNodes)
                    {
                        HashSet<int> reachable = GetReachableNodes(topology, bottomNode.NodeId);
                        reachableFromBottom.UnionWith(reachable);
                    }
                    bossReachable = reachableFromBottom.Contains(topology.BossNodeId);
                }

                // 找到所有不可达的节点（排除Boss节点）
                List<MapNode> unreachableNodes = topology.Nodes.Values
                    .Where(node => !node.IsBoss && !reachableFromBottom.Contains(node.NodeId))
                    .ToList();

                // 重新检查死节点（因为可能已经修复了一些）
                List<int> currentDeadEnds = topology.FindDeadEndNodes();
                
                // 找到所有无法通往Boss的节点（从这些节点无法到达Boss）
                // 注意：只有当Boss可达时，才检查其他节点是否能到达Boss
                List<int> nodesCannotReachBoss = new List<int>();
                if (bossReachable)
                {
                    foreach (var node in topology.Nodes.Values)
                    {
                        if (node.IsBoss)
                        {
                            continue; // Boss节点本身不算问题节点
                        }
                        HashSet<int> reachableFromNode = GetReachableNodes(topology, node.NodeId);
                        if (!reachableFromNode.Contains(topology.BossNodeId))
                        {
                            nodesCannotReachBoss.Add(node.NodeId);
                        }
                    }
                }

                // 合并死节点、不可达节点和无法通往Boss的节点
                // 确保Boss节点永远不会被标记为问题节点
                List<int> allProblemNodes = new List<int>(currentDeadEnds);
                allProblemNodes.AddRange(unreachableNodes.Select(n => n.NodeId));
                allProblemNodes.AddRange(nodesCannotReachBoss);
                allProblemNodes = allProblemNodes
                    .Where(id => id != topology.BossNodeId) // 确保Boss节点不会被标记为问题节点
                    .Distinct()
                    .ToList();

                int topLayer = config.height - 1;
                
                // 检查顶层是否有非Boss节点，如果有则删除
                List<MapNode> topLayerNodes = topology.GetNodesAtLayer(topLayer);
                foreach (MapNode node in topLayerNodes)
                {
                    if (!node.IsBoss && !allProblemNodes.Contains(node.NodeId))
                    {
                        allProblemNodes.Add(node.NodeId);
                    }
                }
                allProblemNodes = allProblemNodes.Distinct().ToList();

                // 如果没有问题节点，检查顶层是否只有Boss节点
                if (allProblemNodes.Count == 0)
                {
                    // 确保顶层只有Boss节点
                    EnsureTopLayerOnlyBoss(topology, config);
                    // 再次验证Boss节点可达
                    reachableFromBottom.Clear();
                    foreach (MapNode bottomNode in bottomLayerNodes)
                    {
                        HashSet<int> reachable = GetReachableNodes(topology, bottomNode.NodeId);
                        reachableFromBottom.UnionWith(reachable);
                    }
                    if (!reachableFromBottom.Contains(topology.BossNodeId))
                    {
                        Debug.LogWarning("[TopologyGenerator] 修复后Boss节点仍不可达，需要继续修复");
                        iterations++;
                        continue;
                    }
                    return false; // 没有问题节点，修复完成
                }
                
                List<int> nodesToDelete = new List<int>();
                bool hasFixed = false;

                foreach (int problemNodeId in allProblemNodes)
                {
                    MapNode problemNode = topology.GetNode(problemNodeId);
                    if (problemNode == null)
                    {
                        continue; // 节点已被删除，跳过
                    }

                    // 确保Boss节点永远不会被处理
                    if (problemNode.IsBoss || problemNodeId == topology.BossNodeId)
                    {
                        continue; // 跳过Boss节点
                    }

                    // 如果问题节点在顶层且不是Boss，直接删除
                    if (problemNode.Layer == topLayer && !problemNode.IsBoss)
                    {
                        nodesToDelete.Add(problemNodeId);
                        continue;
                    }

                    bool canFix = false;

                    // 检查节点是否无法通往Boss（只有当Boss可达时才检查）
                    bool cannotReachBoss = false;
                    if (bossReachable)
                    {
                        HashSet<int> reachableFromNode = GetReachableNodes(topology, problemNodeId);
                        cannotReachBoss = !reachableFromNode.Contains(topology.BossNodeId);
                    }

                    // 如果是死节点(没有向上连接)或无法通往Boss，优先尝试向上连接
                    if ((problemNode.OutDegree == 0 || cannotReachBoss) && problemNode.Layer < topLayer)
                    {
                        int nextLayer = problemNode.Layer + 1;
                        List<MapNode> nextLayerNodes = topology.GetNodesAtLayer(nextLayer)
                            .OrderBy(n => n.Column)
                            .ToList();
                        
                        if (nextLayerNodes.Count > 0)
                        {
                            // 尝试找到符合规则的连接
                            foreach (MapNode targetNode in nextLayerNodes)
                            {
                                if (CheckConnectionRules(topology, problemNodeId, targetNode.NodeId))
                                {
                                    topology.AddEdge(problemNodeId, targetNode.NodeId);
                                    canFix = true;
                                    hasFixed = true;
                                    Debug.Log($"[TopologyGenerator] 修复节点 Node[{problemNodeId}] L{problemNode.Layer}C{problemNode.Column} -> Node[{targetNode.NodeId}] L{targetNode.Layer}C{targetNode.Column}");
                                    break; // 找到一个连接就够了，跳出循环继续下一个问题节点
                                }
                            }
                        }
                    }

                    // 如果节点不可达且向上连接失败，尝试向下连接
                    if (!canFix && !reachableFromBottom.Contains(problemNodeId) && problemNode.Layer > 0)
                    {
                        int prevLayer = problemNode.Layer - 1;
                        List<MapNode> prevLayerNodes = topology.GetNodesAtLayer(prevLayer)
                            .OrderBy(n => n.Column)
                            .ToList();
                        
                        if (prevLayerNodes.Count > 0)
                        {
                            // 尝试找到符合规则的连接
                            foreach (MapNode sourceNode in prevLayerNodes)
                            {
                                if (CheckConnectionRules(topology, sourceNode.NodeId, problemNodeId))
                                {
                                    topology.AddEdge(sourceNode.NodeId, problemNodeId);
                                    canFix = true;
                                    hasFixed = true;
                                    Debug.Log($"[TopologyGenerator] 修复节点 Node[{sourceNode.NodeId}] L{sourceNode.Layer}C{sourceNode.Column} -> Node[{problemNodeId}] L{problemNode.Layer}C{problemNode.Column}");
                                    break; // 找到一个连接就够了，跳出循环继续下一个问题节点
                                }
                            }
                        }
                    }

                    // 如果无法修复，标记为删除
                    if (!canFix)
                    {
                        nodesToDelete.Add(problemNodeId);
                    }
                }

                // 删除无法修复的节点（确保不会删除Boss节点）
                if (nodesToDelete.Count > 0)
                {
                    // 再次确认不会删除Boss节点
                    nodesToDelete = nodesToDelete
                        .Where(id => id != topology.BossNodeId)
                        .ToList();
                    
                    if (nodesToDelete.Count > 0)
                    {
                        Debug.LogWarning($"[TopologyGenerator] 删除 {nodesToDelete.Count} 个无法修复的节点");
                        foreach (int nodeId in nodesToDelete)
                        {
                            MapNode nodeToDelete = topology.GetNode(nodeId);
                            if (nodeToDelete != null && !nodeToDelete.IsBoss)
                            {
                                RemoveNode(topology, nodeId);
                                Debug.Log($"[TopologyGenerator] 已删除节点 Node[{nodeId}] L{nodeToDelete.Layer}C{nodeToDelete.Column}");
                            }
                        }
                        hasFixed = true; // 删除节点也算是一种修复
                        
                        // 删除节点后，检查Boss节点是否仍然存在
                        if (topology.BossNodeId != -1)
                        {
                            MapNode bossAfterDelete = topology.GetNode(topology.BossNodeId);
                            if (bossAfterDelete == null)
                            {
                                // Boss节点被意外删除，需要重新创建
                                Debug.LogError("[TopologyGenerator] Boss节点被意外删除，尝试重新创建");
                                List<MapNode> topLayerNodesAfterDelete = topology.GetNodesAtLayer(topLayer);
                                if (topLayerNodesAfterDelete.Count > 0)
                                {
                                    MapNode newBoss = topLayerNodesAfterDelete[0];
                                    newBoss.IsBoss = true;
                                    topology.BossNodeId = newBoss.NodeId;
                                    Debug.LogWarning($"[TopologyGenerator] 已将节点 Node[{newBoss.NodeId}] 重新标记为Boss");
                                }
                                else
                                {
                                    Debug.LogError("[TopologyGenerator] 顶层没有节点，无法重新创建Boss节点");
                                    return true; // 返回true表示有问题
                                }
                            }
                        }
                        
                        // 删除节点后，需要重新检查，因为删除可能影响其他节点
                        // 所以继续循环，重新检查所有问题
                        iterations++;
                        continue; // 重新开始循环，重新检查所有问题节点
                    }
                }

                // 如果这一轮没有修复任何问题，说明无法继续修复，返回true表示还有问题
                if (!hasFixed)
                {
                    Debug.LogWarning("[TopologyGenerator] 无法继续修复死节点和不可达节点");
                    return true; // 还有问题节点
                }

                iterations++;
            }

            // 如果达到最大迭代次数，直接删除所有死节点和不可达节点
            Debug.LogWarning("[TopologyGenerator] 达到最大迭代次数，开始强制删除所有死节点和不可达节点");
            
            List<int> finalDeadEnds = topology.FindDeadEndNodes();
            List<MapNode> bottomNodes = topology.GetNodesAtLayer(0);
            HashSet<int> finalReachable = new HashSet<int>();
            if (bottomNodes.Count > 0)
            {
                foreach (MapNode bottomNode in bottomNodes)
                {
                    HashSet<int> reachable = GetReachableNodes(topology, bottomNode.NodeId);
                    finalReachable.UnionWith(reachable);
                }
            }
            List<MapNode> finalUnreachable = topology.Nodes.Values
                .Where(node => !node.IsBoss && !finalReachable.Contains(node.NodeId))
                .ToList();

            // 合并所有需要删除的节点（排除Boss节点）
            List<int> nodesToForceDelete = new List<int>(finalDeadEnds);
            nodesToForceDelete.AddRange(finalUnreachable.Select(n => n.NodeId));
            nodesToForceDelete = nodesToForceDelete
                .Where(id => id != topology.BossNodeId)
                .Distinct()
                .ToList();

            // 删除所有问题节点
            if (nodesToForceDelete.Count > 0)
            {
                Debug.LogWarning($"[TopologyGenerator] 强制删除 {nodesToForceDelete.Count} 个死节点和不可达节点");
                foreach (int nodeId in nodesToForceDelete)
                {
                    MapNode nodeToDelete = topology.GetNode(nodeId);
                    if (nodeToDelete != null && !nodeToDelete.IsBoss)
                    {
                        RemoveNode(topology, nodeId);
                        Debug.Log($"[TopologyGenerator] 强制删除节点 Node[{nodeId}] L{nodeToDelete.Layer}C{nodeToDelete.Column}");
                    }
                }

                // 删除节点后，检查Boss节点是否仍然存在
                if (topology.BossNodeId != -1)
                {
                    MapNode bossAfterDelete = topology.GetNode(topology.BossNodeId);
                    if (bossAfterDelete == null)
                    {
                        // Boss节点被意外删除，需要重新创建
                        Debug.LogError("[TopologyGenerator] Boss节点被意外删除，尝试重新创建");
                        int topLayer = config.height - 1;
                        List<MapNode> topLayerNodes = topology.GetNodesAtLayer(topLayer);
                        if (topLayerNodes.Count > 0)
                        {
                            MapNode newBoss = topLayerNodes[0];
                            newBoss.IsBoss = true;
                            topology.BossNodeId = newBoss.NodeId;
                            Debug.LogWarning($"[TopologyGenerator] 已将节点 Node[{newBoss.NodeId}] 重新标记为Boss");
                        }
                        else
                        {
                            Debug.LogError("[TopologyGenerator] 顶层没有节点，无法重新创建Boss节点");
                            return true; // 返回true表示有问题
                        }
                    }
                }

                // 确保顶层只有Boss节点
                EnsureTopLayerOnlyBoss(topology, config);
            }

            // 最终检查是否还有问题
            List<int> remainingDeadEnds = topology.FindDeadEndNodes();
            bottomNodes = topology.GetNodesAtLayer(0);
            finalReachable.Clear();
            if (bottomNodes.Count > 0)
            {
                foreach (MapNode bottomNode in bottomNodes)
                {
                    HashSet<int> reachable = GetReachableNodes(topology, bottomNode.NodeId);
                    finalReachable.UnionWith(reachable);
                }
            }
            List<MapNode> remainingUnreachable = topology.Nodes.Values
                .Where(node => !node.IsBoss && !finalReachable.Contains(node.NodeId))
                .ToList();

            bool stillHasProblems = remainingDeadEnds.Count > 0 || remainingUnreachable.Count > 0;
            
            if (stillHasProblems)
            {
                Debug.LogWarning($"[TopologyGenerator] 强制删除后，仍有 {remainingDeadEnds.Count} 个死节点和 {remainingUnreachable.Count} 个不可达节点");
            }
            else
            {
                Debug.Log("[TopologyGenerator] 强制删除后，所有问题节点已清除");
            }

            return stillHasProblems;
        }

        /// <summary>
        /// 检查连接是否符合规则
        /// 规则1: column差绝对值为1之内的node
        /// 规则2: 连接的column值 >= 前一个节点连接的column最大值(需要检查同层前一个节点)
        /// </summary>
        private static bool CheckConnectionRules(MapTopology topology, int fromNodeId, int toNodeId)
        {
            MapNode fromNode = topology.GetNode(fromNodeId);
            MapNode toNode = topology.GetNode(toNodeId);

            if (fromNode == null || toNode == null)
            {
                return false;
            }

            // 规则1: column差绝对值为1之内的node
            int columnDistance = Mathf.Abs(toNode.Column - fromNode.Column);
            if (columnDistance > 1)
            {
                return false;
            }

            // 规则2: 检查同层前一个节点连接的column最大值
            List<MapNode> sameLayerNodes = topology.GetNodesAtLayer(fromNode.Layer)
                .OrderBy(n => n.Column)
                .ToList();

            int currentIndex = sameLayerNodes.FindIndex(n => n.NodeId == fromNodeId);
            if (currentIndex > 0)
            {
                // 找到前一个节点
                MapNode previousNode = sameLayerNodes[currentIndex - 1];
                
                // 获取前一个节点连接的column最大值
                int previousMaxColumn = -1;
                foreach (int neighborId in previousNode.UpperNeighbors)
                {
                    MapNode neighbor = topology.GetNode(neighborId);
                    if (neighbor != null && neighbor.Column > previousMaxColumn)
                    {
                        previousMaxColumn = neighbor.Column;
                    }
                }

                // 如果前一个节点有连接，当前节点连接的column值必须 >= 前一个节点的最大值
                if (previousMaxColumn >= 0 && toNode.Column < previousMaxColumn)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 移除节点及其所有连接
        /// </summary>
        private static void RemoveNode(MapTopology topology, int nodeId)
        {
            MapNode node = topology.GetNode(nodeId);
            if (node == null)
            {
                return;
            }

            // 移除所有向上连接
            foreach (int upperId in node.UpperNeighbors.ToList())
            {
                MapNode upperNode = topology.GetNode(upperId);
                if (upperNode != null)
                {
                    upperNode.LowerNeighbors.Remove(nodeId);
                }
            }

            // 移除所有向下连接
            foreach (int lowerId in node.LowerNeighbors.ToList())
            {
                MapNode lowerNode = topology.GetNode(lowerId);
                if (lowerNode != null)
                {
                    lowerNode.UpperNeighbors.Remove(nodeId);
                }
            }

            // 从拓扑中移除节点
            topology.Nodes.Remove(nodeId);
            if (topology.NodesByLayer.ContainsKey(node.Layer))
            {
                topology.NodesByLayer[node.Layer].Remove(node);
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
    }
}

