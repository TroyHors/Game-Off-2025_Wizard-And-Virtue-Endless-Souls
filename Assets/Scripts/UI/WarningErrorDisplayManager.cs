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

        [Header("设置")]
        [Tooltip("是否显示警告（Warning）")]
        [SerializeField] private bool showWarnings = true;

        [Tooltip("是否显示错误（Error）")]
        [SerializeField] private bool showErrors = true;

        [Tooltip("是否显示异常（Exception）")]
        [SerializeField] private bool showExceptions = true;

        [Tooltip("是否显示断言失败（Assert）")]
        [SerializeField] private bool showAsserts = true;

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

        private void OnEnable()
        {
            // 订阅Unity日志系统
            Application.logMessageReceived += OnLogMessageReceived;
            Debug.Log("[WarningErrorDisplayManager] 已订阅Unity日志系统");
        }

        private void OnDisable()
        {
            // 取消订阅Unity日志系统
            Application.logMessageReceived -= OnLogMessageReceived;
            Debug.Log("[WarningErrorDisplayManager] 已取消订阅Unity日志系统");
        }

        /// <summary>
        /// Unity日志消息接收回调
        /// </summary>
        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            // 根据日志类型决定是否显示
            bool shouldShow = false;
            switch (type)
            {
                case LogType.Warning:
                    shouldShow = showWarnings;
                    break;
                case LogType.Error:
                    shouldShow = showErrors;
                    break;
                case LogType.Exception:
                    shouldShow = showExceptions;
                    break;
                case LogType.Assert:
                    shouldShow = showAsserts;
                    break;
                case LogType.Log:
                default:
                    // 普通日志不显示
                    return;
            }

            if (!shouldShow)
            {
                return;
            }

            // 显示警告/错误弹窗
            ShowWarningErrorDialog(logString, stackTrace, type);
        }

        /// <summary>
        /// 显示警告/错误弹窗
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="stackTrace">堆栈跟踪（可选）</param>
        /// <param name="type">日志类型</param>
        private void ShowWarningErrorDialog(string message, string stackTrace, LogType type)
        {
            if (warningErrorDialogPrefab == null)
            {
                Debug.LogError("[WarningErrorDisplayManager] 警告/错误弹窗Prefab未设置，无法显示弹窗");
                return;
            }

            if (dialogContainer == null)
            {
                Debug.LogError("[WarningErrorDisplayManager] 弹窗容器未设置，无法显示弹窗");
                return;
            }

            // 实例化弹窗Prefab
            GameObject dialogInstance = Instantiate(warningErrorDialogPrefab, dialogContainer);
            dialogInstance.name = $"{type}Dialog_{System.DateTime.Now:HHmmss}";

            // 查找Text组件并设置消息内容
            Text messageText = dialogInstance.GetComponentInChildren<Text>();
            if (messageText == null)
            {
                // 尝试通过名称查找
                Transform textTransform = dialogInstance.transform.Find("Text") ?? dialogInstance.transform.Find("MessageText");
                if (textTransform != null)
                {
                    messageText = textTransform.GetComponent<Text>();
                }
            }

            if (messageText != null)
            {
                // 根据类型设置标题和内容
                string title = GetLogTypeTitle(type);
                string fullMessage = $"{title}\n\n{message}";
                
                // 如果有堆栈跟踪且不是警告，添加堆栈跟踪（可选，因为可能很长）
                if (!string.IsNullOrEmpty(stackTrace) && type != LogType.Warning)
                {
                    // 只显示堆栈跟踪的前几行，避免弹窗过长
                    string[] stackLines = stackTrace.Split('\n');
                    int maxStackLines = 5;
                    string shortStackTrace = string.Join("\n", stackLines, 0, Mathf.Min(maxStackLines, stackLines.Length));
                    fullMessage += $"\n\n堆栈跟踪:\n{shortStackTrace}";
                }

                messageText.text = fullMessage;
            }
            else
            {
                Debug.LogError("[WarningErrorDisplayManager] 弹窗Prefab中未找到Text组件，无法显示消息");
            }

            // 查找确认按钮并绑定点击事件
            Button confirmButton = dialogInstance.GetComponentInChildren<Button>();
            if (confirmButton == null)
            {
                // 尝试通过名称查找
                Transform buttonTransform = dialogInstance.transform.Find("Button") ?? 
                                           dialogInstance.transform.Find("ConfirmButton") ?? 
                                           dialogInstance.transform.Find("OKButton");
                if (buttonTransform != null)
                {
                    confirmButton = buttonTransform.GetComponent<Button>();
                }
            }

            if (confirmButton != null)
            {
                // 绑定按钮点击事件：删除弹窗实例
                confirmButton.onClick.AddListener(() => OnConfirmButtonClicked(dialogInstance));
            }
            else
            {
                Debug.LogError("[WarningErrorDisplayManager] 弹窗Prefab中未找到Button组件，无法绑定确认事件");
            }

            // 添加到当前弹窗列表
            currentDialogs.Add(dialogInstance);

            Debug.Log($"[WarningErrorDisplayManager] 显示{type}弹窗: {message.Substring(0, Mathf.Min(50, message.Length))}...");
        }

        /// <summary>
        /// 获取日志类型的标题
        /// </summary>
        private string GetLogTypeTitle(LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    return "⚠️ 警告";
                case LogType.Error:
                    return "❌ 错误";
                case LogType.Exception:
                    return "💥 异常";
                case LogType.Assert:
                    return "⚠️ 断言失败";
                default:
                    return "信息";
            }
        }

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

