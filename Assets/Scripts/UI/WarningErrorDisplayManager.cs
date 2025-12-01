using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameFlow
{
    /// <summary>
    /// 警告/错误显示管理器
    /// 监听Unity日志系统，当有警告或错误时显示弹窗
    /// </summary>
    public class WarningErrorDisplayManager : MonoBehaviour
    {
        [Header("弹窗设置")]
        [Tooltip("警告/错误弹窗Prefab（必须包含Text组件和确认按钮）")]
        [SerializeField] private GameObject warningErrorDialogPrefab;

        [Tooltip("弹窗父容器（弹窗会作为此GameObject的子对象生成，如果为空则使用Canvas）")]
        [SerializeField] private Transform dialogContainer;

        // 注意：此组件已不再监听Unity日志系统，以下设置已无效
        // 所有警告/错误信息都通过GameErrorDisplaySystem显示

        [Header("运行时状态")]
        [Tooltip("当前显示的弹窗实例列表")]
        [SerializeField] private List<GameObject> currentDialogs = new List<GameObject>();

        private void Awake()
        {
            // 如果没有设置容器，尝试查找Canvas
            if (dialogContainer == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    dialogContainer = canvas.transform;
                    Debug.Log("[WarningErrorDisplayManager] 自动使用Canvas作为弹窗容器");
                }
            }

            // 如果没有容器，使用自身作为容器
            if (dialogContainer == null)
            {
                dialogContainer = transform;
                Debug.LogWarning("[WarningErrorDisplayManager] 未找到弹窗容器，使用自身作为容器");
            }
        }

        // 注意：此组件已不再监听Unity日志系统
        // 所有警告/错误信息都通过GameErrorDisplaySystem显示
        // 保留此组件仅用于向后兼容，可以禁用或删除

        /// <summary>
        /// 确认按钮点击事件
        /// 删除弹窗实例
        /// </summary>
        private void OnConfirmButtonClicked(GameObject dialogInstance)
        {
            if (dialogInstance != null)
            {
                // 从列表中移除
                currentDialogs.Remove(dialogInstance);

                // 销毁实例
                Destroy(dialogInstance);
                Debug.Log("[WarningErrorDisplayManager] 弹窗已关闭");
            }
        }

        /// <summary>
        /// 清除所有弹窗（供外部调用）
        /// </summary>
        public void ClearAllDialogs()
        {
            foreach (GameObject dialog in currentDialogs)
            {
                if (dialog != null)
                {
                    Destroy(dialog);
                }
            }
            currentDialogs.Clear();
            Debug.Log("[WarningErrorDisplayManager] 已清除所有弹窗");
        }

        private void OnDestroy()
        {
            // 清理所有弹窗
            ClearAllDialogs();
        }
    }
}

