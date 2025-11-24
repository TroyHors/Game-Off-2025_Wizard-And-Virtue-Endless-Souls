using MapSystem;
using UnityEngine;

namespace GameFlow
{
    /// <summary>
    /// 节点事件流程基类
    /// 提供通用的初始化逻辑和事件管理
    /// </summary>
    public abstract class NodeEventFlowBase : MonoBehaviour, INodeEventFlow
    {
        /// <summary>
        /// 当前节点数据
        /// </summary>
        protected MapNode currentNodeData { get; private set; }

        /// <summary>
        /// 节点事件完成事件
        /// </summary>
        public System.Action<MapNode> OnFlowFinished { get; set; }

        /// <summary>
        /// 初始化节点事件流程
        /// </summary>
        /// <param name="nodeData">节点数据</param>
        public virtual void Initialize(MapNode nodeData)
        {
            currentNodeData = nodeData;
            Debug.Log($"[{GetType().Name}] 初始化节点事件流程: Node[{nodeData.NodeId}] Type:{nodeData.NodeType}");
        }

        /// <summary>
        /// 开始执行事件流程
        /// 子类必须实现此方法
        /// </summary>
        public abstract void StartFlow();

        /// <summary>
        /// 完成事件流程
        /// 子类在事件完成时调用此方法
        /// </summary>
        protected void FinishFlow()
        {
            if (currentNodeData == null)
            {
                Debug.LogError($"[{GetType().Name}] 尝试完成流程，但节点数据为空");
                return;
            }

            Debug.Log($"[{GetType().Name}] 节点事件流程完成: Node[{currentNodeData.NodeId}]");
            OnFlowFinished?.Invoke(currentNodeData);
        }
    }
}

