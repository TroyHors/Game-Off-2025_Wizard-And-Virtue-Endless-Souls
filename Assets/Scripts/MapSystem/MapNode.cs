using System.Collections.Generic;
using UnityEngine;

namespace MapSystem
{
    /// <summary>
    /// 地图节点
    /// 表示地图上的一个房间节点
    /// </summary>
    [System.Serializable]
    public class MapNode
    {
        /// <summary>
        /// 节点唯一ID
        /// </summary>
        public int NodeId { get; set; }

        /// <summary>
        /// 所在层数(从0开始,0为底层)
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// 列位置(在层级网格中的列索引)
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// 节点类型标记(由内容填充系统分配,用于导向对应的事件)
        /// </summary>
        public string NodeType { get; set; }

        /// <summary>
        /// 邻接的上层节点ID列表(可以通往的节点)
        /// </summary>
        public List<int> UpperNeighbors { get; set; }

        /// <summary>
        /// 邻接的下层节点ID列表(可以来自的节点)
        /// </summary>
        public List<int> LowerNeighbors { get; set; }

        /// <summary>
        /// 是否为Boss节点
        /// </summary>
        public bool IsBoss { get; set; }

        public MapNode(int nodeId, int layer, int column)
        {
            NodeId = nodeId;
            Layer = layer;
            Column = column;
            NodeType = string.Empty;
            UpperNeighbors = new List<int>();
            LowerNeighbors = new List<int>();
            IsBoss = false;
        }

        /// <summary>
        /// 添加上层邻居
        /// </summary>
        public void AddUpperNeighbor(int neighborId)
        {
            if (!UpperNeighbors.Contains(neighborId))
            {
                UpperNeighbors.Add(neighborId);
            }
        }

        /// <summary>
        /// 添加下层邻居
        /// </summary>
        public void AddLowerNeighbor(int neighborId)
        {
            if (!LowerNeighbors.Contains(neighborId))
            {
                LowerNeighbors.Add(neighborId);
            }
        }

        /// <summary>
        /// 获取出度(向上连接的节点数)
        /// </summary>
        public int OutDegree => UpperNeighbors.Count;

        /// <summary>
        /// 获取入度(向下连接的节点数)
        /// </summary>
        public int InDegree => LowerNeighbors.Count;

        public override string ToString()
        {
            return $"Node[{NodeId}] L{Layer}C{Column} Type:{NodeType} Out:{OutDegree} In:{InDegree}";
        }
    }
}

