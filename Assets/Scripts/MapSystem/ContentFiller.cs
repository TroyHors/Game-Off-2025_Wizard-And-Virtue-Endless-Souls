using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapSystem
{
    /// <summary>
    /// 内容填充器
    /// 在既定拓扑上为每个节点分配类型和事件
    /// </summary>
    public static class ContentFiller
    {
        /// <summary>
        /// 路径合理性约束接口
        /// 用于检查路径是否满足合理性要求
        /// </summary>
        public interface IPathConstraint
        {
            /// <summary>
            /// 检查路径是否满足约束
            /// </summary>
            /// <param name="path">节点ID路径</param>
            /// <param name="topology">地图拓扑</param>
            /// <returns>是否满足约束</returns>
            bool CheckPath(List<int> path, MapTopology topology);

            /// <summary>
            /// 获取约束描述(用于调试)
            /// </summary>
            string GetDescription();
        }

        /// <summary>
        /// 填充节点内容
        /// </summary>
        /// <param name="topology">地图拓扑</param>
        /// <param name="config">生成配置</param>
        /// <param name="seed">随机种子</param>
        /// <param name="constraints">路径合理性约束列表(可选)</param>
        /// <returns>是否填充成功</returns>
        public static bool FillContent(
            MapTopology topology, 
            MapGenerationConfig config, 
            int seed = -1,
            List<IPathConstraint> constraints = null)
        {
            if (!config.enableContentFilling)
            {
                Debug.Log("[ContentFiller] 内容填充已禁用,跳过");
                return true;
            }

            if (config.nodeTypeConfigs == null || config.nodeTypeConfigs.Length == 0)
            {
                Debug.LogWarning("[ContentFiller] 没有配置节点类型,跳过内容填充");
                return true;
            }

            // 设置随机种子
            System.Random random = seed >= 0 ? new System.Random(seed) : new System.Random();

            // 阶段1: 按权重随机分配节点类型
            AssignNodeTypesByWeight(topology, config, random);

            // 阶段2: 验证路径合理性并调整
            if (config.enablePathValidation && constraints != null && constraints.Count > 0)
            {
                ValidateAndAdjustPaths(topology, config, constraints, random);
            }

            Debug.Log("[ContentFiller] 内容填充完成");
            return true;
        }

        /// <summary>
        /// 按权重分配节点类型
        /// </summary>
        private static void AssignNodeTypesByWeight(
            MapTopology topology, 
            MapGenerationConfig config, 
            System.Random random)
        {
            // 统计已分配的数量
            Dictionary<string, int> assignedCounts = new Dictionary<string, int>();
            // 记录每个节点类型最后出现的层数
            Dictionary<string, int> lastLayerOfType = new Dictionary<string, int>();
            foreach (var typeConfig in config.nodeTypeConfigs)
            {
                assignedCounts[typeConfig.nodeType] = 0;
                lastLayerOfType[typeConfig.nodeType] = -1; // -1表示还未出现过
            }

            // 获取所有非Boss节点，按层分组，每层内随机打乱顺序，避免底层集中分配
            List<MapNode> nodesToAssign = topology.Nodes.Values
                .Where(node => !node.IsBoss)
                .GroupBy(node => node.Layer)
                .OrderBy(group => group.Key) // 按层排序
                .SelectMany(group => group.OrderBy(n => random.Next())) // 每层内随机打乱
                .ToList();

            // 为每个节点分配类型
            foreach (var node in nodesToAssign)
            {
                // 计算当前层级的权重
                float normalizedLayer = (float)node.Layer / (config.height - 1);
                Dictionary<string, float> weights = CalculateWeightsForLayer(
                    config, normalizedLayer, assignedCounts, node.Layer, lastLayerOfType);

                // 根据权重随机选择
                string selectedType = SelectTypeByWeight(weights, random);
                node.NodeType = selectedType;
                assignedCounts[selectedType]++;
                lastLayerOfType[selectedType] = node.Layer; // 更新最后出现的层数
            }

            Debug.Log($"[ContentFiller] 节点类型分配完成");
        }

        /// <summary>
        /// 计算指定层级的权重
        /// </summary>
        private static Dictionary<string, float> CalculateWeightsForLayer(
            MapGenerationConfig config, 
            float normalizedLayer, 
            Dictionary<string, int> assignedCounts,
            int currentLayer,
            Dictionary<string, int> lastLayerOfType)
        {
            Dictionary<string, float> weights = new Dictionary<string, float>();

            // 计算总节点数(用于计算剩余节点数)
            int totalNodes = config.nodeTypeConfigs.Sum(c => 
                assignedCounts.ContainsKey(c.nodeType) ? assignedCounts[c.nodeType] : 0);
            // 这里需要知道总节点数，但由于是随机分配，我们使用一个估算值
            // 实际应该从topology获取，但为了简化，我们使用一个合理的估算

            foreach (var typeConfig in config.nodeTypeConfigs)
            {
                // 基础权重
                float weight = typeConfig.globalWeight;

                // 应用层级权重曲线
                float layerMultiplier = typeConfig.layerWeightCurve.Evaluate(normalizedLayer);
                weight *= layerMultiplier;

                // 检查最小/最大数量限制
                int currentCount = assignedCounts.ContainsKey(typeConfig.nodeType) 
                    ? assignedCounts[typeConfig.nodeType] 
                    : 0;

                if (typeConfig.maxCount > 0 && currentCount >= typeConfig.maxCount)
                {
                    weight = 0; // 已达到最大数量
                }
                else if (typeConfig.minCount > 0 && currentCount < typeConfig.minCount)
                {
                    // 改进：根据剩余需要分配的数量和当前层级来调整权重
                    // 如果当前层级权重曲线值较高，则增加权重；如果较低，则适度增加
                    int remainingNeeded = typeConfig.minCount - currentCount;
                    float urgencyMultiplier = 1f + (remainingNeeded * 0.5f); // 根据剩余数量调整
                    
                    // 如果当前层级的权重曲线值较低，说明这个类型不应该在这个层级出现
                    // 但仍然需要满足最小数量，所以适度增加权重
                    if (layerMultiplier < 0.5f)
                    {
                        urgencyMultiplier *= 0.5f; // 降低权重，避免在低权重层级过度分配
                    }
                    
                    weight *= urgencyMultiplier;
                }

                // 检查最小间隔层数限制
                if (typeConfig.minLayerInterval > 0)
                {
                    int lastLayer = lastLayerOfType.ContainsKey(typeConfig.nodeType) 
                        ? lastLayerOfType[typeConfig.nodeType] 
                        : -1;
                    
                    if (lastLayer >= 0)
                    {
                        int layerDistance = currentLayer - lastLayer;
                        if (layerDistance < typeConfig.minLayerInterval)
                        {
                            // 违反最小间隔要求，权重设为0
                            weight = 0;
                        }
                    }
                }

                weights[typeConfig.nodeType] = weight;
            }

            return weights;
        }

        /// <summary>
        /// 根据权重随机选择类型
        /// </summary>
        private static string SelectTypeByWeight(Dictionary<string, float> weights, System.Random random)
        {
            float totalWeight = weights.Values.Sum();
            if (totalWeight <= 0)
            {
                // 如果所有权重都为0,随机选择一个
                List<string> types = weights.Keys.ToList();
                return types[random.Next(types.Count)];
            }

            float randomValue = (float)(random.NextDouble() * totalWeight);
            float cumulative = 0f;

            foreach (var kvp in weights)
            {
                cumulative += kvp.Value;
                if (randomValue <= cumulative)
                {
                    return kvp.Key;
                }
            }

            // 兜底:返回最后一个类型
            return weights.Keys.Last();
        }

        /// <summary>
        /// 验证和调整路径
        /// </summary>
        private static void ValidateAndAdjustPaths(
            MapTopology topology, 
            MapGenerationConfig config, 
            List<IPathConstraint> constraints, 
            System.Random random)
        {
            // 获取所有从起点到Boss的路径
            List<List<int>> allPaths = topology.GetAllPathsToBoss();

            if (allPaths.Count == 0)
            {
                Debug.LogWarning("[ContentFiller] 没有找到从起点到Boss的路径");
                return;
            }

            // 随机选择若干条路径进行验证
            int sampleCount = Mathf.Min(5, allPaths.Count);
            List<List<int>> samplePaths = allPaths
                .OrderBy(x => random.Next())
                .Take(sampleCount)
                .ToList();

            int adjustmentAttempts = 0;
            const int maxAttempts = 10;

            foreach (var path in samplePaths)
            {
                bool pathValid = true;
                foreach (var constraint in constraints)
                {
                    if (!constraint.CheckPath(path, topology))
                    {
                        pathValid = false;
                        Debug.LogWarning($"[ContentFiller] 路径不满足约束: {constraint.GetDescription()}");
                        break;
                    }
                }

                if (!pathValid && adjustmentAttempts < maxAttempts)
                {
                    // 尝试调整路径上的节点类型
                    AdjustPathNodes(path, topology, config, constraints, random);
                    adjustmentAttempts++;
                }
            }
        }

        /// <summary>
        /// 调整路径上的节点类型
        /// </summary>
        private static void AdjustPathNodes(
            List<int> path, 
            MapTopology topology, 
            MapGenerationConfig config, 
            List<IPathConstraint> constraints, 
            System.Random random)
        {
            // 简单的调整策略:随机替换路径上的某些节点类型
            // 更复杂的策略可以在具体约束中实现

            foreach (int nodeId in path)
            {
                MapNode node = topology.GetNode(nodeId);
                if (node == null || node.IsBoss)
                {
                    continue;
                }

                // 随机决定是否调整这个节点
                if (random.NextDouble() < 0.3f) // 30%概率调整
                {
                    // 重新分配类型
                    float normalizedLayer = (float)node.Layer / (config.height - 1);
                    Dictionary<string, int> dummyCounts = new Dictionary<string, int>();
                    Dictionary<string, int> dummyLastLayers = new Dictionary<string, int>();
                    // 在调整阶段，暂时不考虑最小间隔限制（使用-1表示未出现过）
                    foreach (var typeConfig in config.nodeTypeConfigs)
                    {
                        dummyLastLayers[typeConfig.nodeType] = -1;
                    }
                    Dictionary<string, float> weights = CalculateWeightsForLayer(
                        config, normalizedLayer, dummyCounts, node.Layer, dummyLastLayers);
                    string newType = SelectTypeByWeight(weights, random);
                    node.NodeType = newType;
                }
            }
        }
    }
}

