using MapSystem;
using UnityEngine;

namespace GameFlow
{
    /// <summary>
    /// 酒馆节点事件流程
    /// 处理酒馆节点的成员展示和购买
    /// </summary>
    public class TavernNodeFlow : NodeEventFlowBase
    {
        [Header("酒馆系统")]
        [Tooltip("酒馆管理器（如果为空，会自动查找）")]
        [SerializeField] private TavernManager tavernManager;

        [Header("UI设置")]
        [Tooltip("酒馆容器（如果为空，会自动查找）")]
        [SerializeField] private Transform tavernContainer;

        /// <summary>
        /// 当前节点类型（从节点数据获取）
        /// </summary>
        public string CurrentNodeType => currentNodeData != null ? currentNodeData.NodeType : string.Empty;

        private void Awake()
        {
            // 自动查找酒馆管理器
            if (tavernManager == null)
            {
                tavernManager = GetComponent<TavernManager>();
            }

            if (tavernManager == null)
            {
                tavernManager = FindObjectOfType<TavernManager>();
            }

            // 自动查找酒馆容器（在子对象中查找）
            if (tavernContainer == null)
            {
                tavernContainer = transform.Find("TavernContainer");
            }

            // 如果找到了容器，设置到TavernManager
            if (tavernContainer != null && tavernManager != null)
            {
                tavernManager.SetTavernContainer(tavernContainer);
            }
        }

        /// <summary>
        /// 开始执行酒馆流程
        /// </summary>
        public override void StartFlow()
        {
            if (currentNodeData == null)
            {
                Debug.LogError("[TavernNodeFlow] 无法开始酒馆流程: 节点数据为空");
                return;
            }

            Debug.Log($"[TavernNodeFlow] 开始酒馆流程: Node[{currentNodeData.NodeId}] Type:{CurrentNodeType}");

            // 确保找到酒馆管理器
            if (tavernManager == null)
            {
                tavernManager = GetComponent<TavernManager>();
            }

            if (tavernManager == null)
            {
                tavernManager = FindObjectOfType<TavernManager>();
            }

            if (tavernManager == null)
            {
                Debug.LogError("[TavernNodeFlow] 酒馆管理器未找到，无法初始化酒馆");
                return;
            }

            // 如果找到了酒馆容器，设置到TavernManager
            if (tavernContainer == null)
            {
                tavernContainer = transform.Find("TavernContainer");
            }

            if (tavernContainer != null && tavernManager != null)
            {
                tavernManager.SetTavernContainer(tavernContainer);
            }

            // 初始化酒馆
            tavernManager.InitializeTavern();
        }

        /// <summary>
        /// 完成酒馆并结束流程（供按钮调用）
        /// 玩家通过按钮确认后调用此方法
        /// </summary>
        public void FinishTavernAndFlow()
        {
            Debug.Log("[TavernNodeFlow] 玩家确认，结束酒馆流程");

            // 清理酒馆
            if (tavernManager != null)
            {
                tavernManager.ClearTavern();
            }

            // 完成流程
            FinishFlow();
        }
    }
}

