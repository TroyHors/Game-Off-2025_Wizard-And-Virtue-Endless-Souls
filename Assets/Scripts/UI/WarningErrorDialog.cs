using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GameFlow
{
    /// <summary>
    /// 警告/错误弹窗组件
    /// 挂载在弹窗Prefab上，直接引用Text和Button组件
    /// </summary>
    public class WarningErrorDialog : MonoBehaviour
    {
        [Header("组件引用")]
        [Tooltip("消息文本组件（TextMeshPro，用于显示警告/错误信息）")]
        [SerializeField] private TextMeshProUGUI messageText;

        [Tooltip("确认按钮（点击后关闭弹窗）")]
        [SerializeField] private Button confirmButton;

        /// <summary>
        /// 消息文本组件
        /// </summary>
        public TextMeshProUGUI MessageText => messageText;

        /// <summary>
        /// 确认按钮
        /// </summary>
        public Button ConfirmButton => confirmButton;

        /// <summary>
        /// 设置消息内容
        /// </summary>
        /// <param name="message">消息内容</param>
        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
            else
            {
                Debug.LogError("[WarningErrorDialog] MessageText未设置，无法显示消息");
            }
        }

        /// <summary>
        /// 设置确认按钮点击事件
        /// </summary>
        /// <param name="onClick">点击回调</param>
        public void SetConfirmCallback(UnityEngine.Events.UnityAction onClick)
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(onClick);
            }
            else
            {
                Debug.LogError("[WarningErrorDialog] ConfirmButton未设置，无法绑定点击事件");
            }
        }

        private void Reset()
        {
            // 在Inspector中自动查找组件（如果未设置）
            if (messageText == null)
            {
                messageText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (confirmButton == null)
            {
                confirmButton = GetComponentInChildren<Button>();
            }
        }
    }
}

