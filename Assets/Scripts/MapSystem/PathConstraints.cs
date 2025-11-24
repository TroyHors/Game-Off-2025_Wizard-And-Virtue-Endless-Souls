using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapSystem
{
    /// <summary>
    /// 路径合理性约束的示例实现
    /// 用户可以根据需要扩展或创建新的约束类
    /// </summary>

    /// <summary>
    /// 最少休息节点约束
    /// 确保每条路径上至少有指定数量的休息节点(如营火)
    /// </summary>
    public class MinRestNodesConstraint : ContentFiller.IPathConstraint
    {
        private string restNodeType;
        private int minCount;

        public MinRestNodesConstraint(string restNodeType, int minCount)
        {
            this.restNodeType = restNodeType;
            this.minCount = minCount;
        }

        public bool CheckPath(List<int> path, MapTopology topology)
        {
            int restNodeCount = 0;
            foreach (int nodeId in path)
            {
                MapNode node = topology.GetNode(nodeId);
                if (node != null && node.NodeType == restNodeType)
                {
                    restNodeCount++;
                }
            }

            return restNodeCount >= minCount;
        }

        public string GetDescription()
        {
            return $"最少休息节点约束: 路径上至少需要 {minCount} 个类型为 '{restNodeType}' 的节点";
        }
    }

    /// <summary>
    /// 最多连续精英节点约束
    /// 防止相邻层出现过多连续精英节点
    /// </summary>
    public class MaxConsecutiveEliteNodesConstraint : ContentFiller.IPathConstraint
    {
        private string eliteNodeType;
        private int maxConsecutive;

        public MaxConsecutiveEliteNodesConstraint(string eliteNodeType, int maxConsecutive)
        {
            this.eliteNodeType = eliteNodeType;
            this.maxConsecutive = maxConsecutive;
        }

        public bool CheckPath(List<int> path, MapTopology topology)
        {
            int consecutiveCount = 0;
            int maxFound = 0;

            foreach (int nodeId in path)
            {
                MapNode node = topology.GetNode(nodeId);
                if (node != null && node.NodeType == eliteNodeType)
                {
                    consecutiveCount++;
                    maxFound = Mathf.Max(maxFound, consecutiveCount);
                }
                else
                {
                    consecutiveCount = 0;
                }
            }

            return maxFound <= maxConsecutive;
        }

        public string GetDescription()
        {
            return $"最多连续精英节点约束: 路径上最多连续 {maxConsecutive} 个类型为 '{eliteNodeType}' 的节点";
        }
    }

    /// <summary>
    /// 重要功能点分布约束
    /// 确保重要功能点(如商店、营火)不会全挤在前期或后期
    /// </summary>
    public class ImportantNodesDistributionConstraint : ContentFiller.IPathConstraint
    {
        private string[] importantNodeTypes;
        private float minEarlyRatio; // 前期(前30%)最少比例
        private float minLateRatio;  // 后期(后30%)最少比例

        public ImportantNodesDistributionConstraint(string[] importantNodeTypes, float minEarlyRatio, float minLateRatio)
        {
            this.importantNodeTypes = importantNodeTypes;
            this.minEarlyRatio = minEarlyRatio;
            this.minLateRatio = minLateRatio;
        }

        public bool CheckPath(List<int> path, MapTopology topology)
        {
            if (path.Count == 0)
            {
                return true;
            }

            List<int> importantNodeIndices = new List<int>();
            for (int i = 0; i < path.Count; i++)
            {
                MapNode node = topology.GetNode(path[i]);
                if (node != null && importantNodeTypes.Contains(node.NodeType))
                {
                    importantNodeIndices.Add(i);
                }
            }

            if (importantNodeIndices.Count == 0)
            {
                return false; // 至少需要一个重要节点
            }

            int earlyThreshold = Mathf.FloorToInt(path.Count * 0.3f);
            int lateThreshold = Mathf.CeilToInt(path.Count * 0.7f);

            int earlyCount = importantNodeIndices.Count(idx => idx < earlyThreshold);
            int lateCount = importantNodeIndices.Count(idx => idx >= lateThreshold);

            float earlyRatio = (float)earlyCount / importantNodeIndices.Count;
            float lateRatio = (float)lateCount / importantNodeIndices.Count;

            return earlyRatio >= minEarlyRatio && lateRatio >= minLateRatio;
        }

        public string GetDescription()
        {
            return $"重要节点分布约束: 重要节点类型 {string.Join(",", importantNodeTypes)} 在前期和后期都需要有分布";
        }
    }
}

