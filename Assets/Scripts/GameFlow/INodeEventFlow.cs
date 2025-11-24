using MapSystem;

namespace GameFlow
{
    /// <summary>
    /// 节点事件流程接口
    /// 所有节点事件类型都必须实现此接口
    /// </summary>
    public interface INodeEventFlow
    {
        /// <summary>
        /// 初始化节点事件流程
        /// </summary>
        /// <param name="nodeData">节点数据</param>
        void Initialize(MapNode nodeData);

        /// <summary>
        /// 开始执行事件流程
        /// 可能是协程或异步流程
        /// </summary>
        void StartFlow();

        /// <summary>
        /// 节点事件完成事件
        /// 当事件流程完成时，必须调用此事件通知 GameFlowManager
        /// </summary>
        System.Action<MapNode> OnFlowFinished { get; set; }
    }
}

