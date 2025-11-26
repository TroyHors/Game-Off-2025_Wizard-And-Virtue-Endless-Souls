using MapSystem;
using UnityEngine;

namespace GameFlow
{
    /// <summary>
    /// 商店节点事件流程
    /// 处理商店节点的商品展示和购买
    /// </summary>
    public class ShopNodeFlow : NodeEventFlowBase
    {
        [Header("商店系统")]
        [Tooltip("商店管理器（如果为空，会自动查找）")]
        [SerializeField] private ShopManager shopManager;

        [Header("UI设置")]
        [Tooltip("商店容器（如果为空，会自动查找）")]
        [SerializeField] private Transform shopContainer;

        /// <summary>
        /// 当前节点类型（从节点数据获取）
        /// </summary>
        public string CurrentNodeType => currentNodeData != null ? currentNodeData.NodeType : string.Empty;

        private void Awake()
        {
            // 自动查找商店管理器
            if (shopManager == null)
            {
                shopManager = GetComponent<ShopManager>();
            }

            if (shopManager == null)
            {
                shopManager = FindObjectOfType<ShopManager>();
            }

            // 自动查找商店容器（在子对象中查找）
            if (shopContainer == null)
            {
                shopContainer = transform.Find("ShopContainer");
            }

            // 如果找到了容器，设置到ShopManager
            if (shopContainer != null && shopManager != null)
            {
                shopManager.SetShopContainer(shopContainer);
            }
        }

        /// <summary>
        /// 开始执行商店流程
        /// </summary>
        public override void StartFlow()
        {
            if (currentNodeData == null)
            {
                Debug.LogError("[ShopNodeFlow] 无法开始商店流程: 节点数据为空");
                return;
            }

            Debug.Log($"[ShopNodeFlow] 开始商店流程: Node[{currentNodeData.NodeId}] Type:{CurrentNodeType}");

            // 确保找到商店管理器
            if (shopManager == null)
            {
                shopManager = GetComponent<ShopManager>();
            }

            if (shopManager == null)
            {
                shopManager = FindObjectOfType<ShopManager>();
            }

            if (shopManager == null)
            {
                Debug.LogError("[ShopNodeFlow] 商店管理器未找到，无法初始化商店");
                return;
            }

            // 如果找到了商店容器，设置到ShopManager
            if (shopContainer == null)
            {
                shopContainer = transform.Find("ShopContainer");
            }

            if (shopContainer != null && shopManager != null)
            {
                shopManager.SetShopContainer(shopContainer);
            }

            // 初始化商店
            shopManager.InitializeShop();
        }

        /// <summary>
        /// 完成商店并结束流程（供按钮调用）
        /// 玩家通过按钮确认后调用此方法
        /// </summary>
        public void FinishShopAndFlow()
        {
            Debug.Log("[ShopNodeFlow] 玩家确认，结束商店流程");

            // 清理商店
            if (shopManager != null)
            {
                shopManager.ClearShop();
            }

            // 完成流程
            FinishFlow();
        }
    }
}

