using System.Collections;
using UnityEngine;

namespace GameFlow
{
    /// <summary>
    /// 游戏报错显示系统
    /// 用于显示游戏内的警告和错误信息（如金币不足等）
    /// 不监听Unity引擎日志，只显示通过此系统发送的游戏内错误信息
    /// </summary>
    public class GameErrorDisplaySystem : MonoBehaviour
    {
        [Header("弹窗设置")]
        [Tooltip("警告/错误弹窗Prefab（必须包含WarningErrorDialog组件）")]
        [SerializeField] private GameObject warningErrorDialogPrefab;

        [Tooltip("弹窗父容器（弹窗会作为此GameObject的子对象生成，如果为空则使用Canvas）")]
        [SerializeField] private Transform dialogContainer;

        [Header("运行时状态")]
        [Tooltip("当前显示的弹窗实例列表")]
        [SerializeField] private System.Collections.Generic.List<GameObject> currentDialogs = new System.Collections.Generic.List<GameObject>();

        private static GameErrorDisplaySystem instance;

        /// <summary>
        /// 单例实例（用于静态调用）
        /// </summary>
        public static GameErrorDisplaySystem Instance => instance;

        private void Awake()
        {
            // 设置单例
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.LogWarning("[GameErrorDisplaySystem] 检测到多个实例，销毁重复实例");
                Destroy(gameObject);
                return;
            }

            // 如果没有设置容器，尝试查找Canvas
            if (dialogContainer == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    dialogContainer = canvas.transform;
                }
            }

            // 如果没有容器，使用自身作为容器
            if (dialogContainer == null)
            {
                dialogContainer = transform;
            }

            // 如果没有设置弹窗Prefab，尝试从WarningErrorDisplayManager获取
            if (warningErrorDialogPrefab == null)
            {
                WarningErrorDisplayManager existingManager = FindObjectOfType<WarningErrorDisplayManager>();
                if (existingManager != null)
                {
                    // 通过反射获取warningErrorDialogPrefab字段（因为它是private的）
                    var prefabField = typeof(WarningErrorDisplayManager).GetField("warningErrorDialogPrefab", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (prefabField != null)
                    {
                        warningErrorDialogPrefab = prefabField.GetValue(existingManager) as GameObject;
                    }
                }
            }
        }

        /// <summary>
        /// 确保实例存在，如果不存在则自动创建
        /// </summary>
        private static void EnsureInstance()
        {
            if (instance == null)
            {
                // 尝试在场景中查找现有实例
                instance = FindObjectOfType<GameErrorDisplaySystem>();
                
                // 如果仍然不存在，自动创建新实例
                if (instance == null)
                {
                    GameObject systemObject = new GameObject("GameErrorDisplaySystem");
                    instance = systemObject.AddComponent<GameErrorDisplaySystem>();
                    DontDestroyOnLoad(systemObject); // 确保跨场景持久化
                    
                    // 自动创建后，Awake会被调用，但为了确保Prefab和容器被正确设置，
                    // 我们需要等待一帧让Awake完成，或者直接在这里初始化
                    // 由于Awake会在AddComponent后立即调用，所以Prefab和容器应该已经通过Awake中的逻辑获取了
                }
            }
        }

        /// <summary>
        /// 显示游戏警告信息（静态方法，供其他系统调用）
        /// 注意：警告消息必须只使用英文字母（alphabet characters），不能使用中文或其他非ASCII字符
        /// </summary>
        /// <param name="message">警告消息（必须只包含英文字母、数字、空格和基本标点符号）</param>
        public static void ShowWarning(string message)
        {
            EnsureInstance();
            if (instance != null)
            {
                instance.ShowGameError(message, ErrorType.Warning);
            }
        }

        /// <summary>
        /// 显示游戏错误信息（静态方法，供其他系统调用）
        /// 注意：错误消息必须只使用英文字母（alphabet characters），不能使用中文或其他非ASCII字符
        /// </summary>
        /// <param name="message">错误消息（必须只包含英文字母、数字、空格和基本标点符号）</param>
        public static void ShowError(string message)
        {
            EnsureInstance();
            if (instance != null)
            {
                instance.ShowGameError(message, ErrorType.Error);
            }
        }

        /// <summary>
        /// 显示游戏信息（静态方法，供其他系统调用）
        /// 注意：信息消息必须只使用英文字母（alphabet characters），不能使用中文或其他非ASCII字符
        /// </summary>
        /// <param name="message">信息消息（必须只包含英文字母、数字、空格和基本标点符号）</param>
        public static void ShowInfo(string message)
        {
            EnsureInstance();
            if (instance != null)
            {
                instance.ShowGameError(message, ErrorType.Info);
            }
        }

        /// <summary>
        /// 显示游戏错误信息（内部方法）
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="errorType">错误类型</param>
        private void ShowGameError(string message, ErrorType errorType)
        {
            if (warningErrorDialogPrefab == null)
            {
                Debug.LogError("[GameErrorDisplaySystem] 警告/错误弹窗Prefab未设置，无法显示弹窗");
                return;
            }

            if (dialogContainer == null)
            {
                Debug.LogError("[GameErrorDisplaySystem] 弹窗容器未设置，无法显示弹窗");
                return;
            }

            // 延迟显示，避免在图形重建循环中实例化UI对象
            StartCoroutine(ShowGameErrorDelayed(message, errorType));
        }

        /// <summary>
        /// 延迟显示游戏错误（协程）
        /// </summary>
        private IEnumerator ShowGameErrorDelayed(string message, ErrorType errorType)
        {
            // 等待一帧，确保不在图形重建循环中
            yield return null;

            // 实例化弹窗Prefab
            GameObject dialogInstance = Instantiate(warningErrorDialogPrefab, dialogContainer);
            dialogInstance.name = $"{errorType}Dialog_{System.DateTime.Now:HHmmss}";

            // 获取WarningErrorDialog组件（应该挂载在Prefab上）
            WarningErrorDialog dialogComponent = dialogInstance.GetComponent<WarningErrorDialog>();
            if (dialogComponent == null)
            {
                Debug.LogError("[GameErrorDisplaySystem] 弹窗Prefab缺少WarningErrorDialog组件！请在Prefab上挂载WarningErrorDialog组件并设置Text和Button引用");
                Destroy(dialogInstance);
                yield break;
            }

            // 构建完整消息内容
            string title = GetErrorTypeTitle(errorType);
            string fullMessage = $"{title}\n\n{message}";

            // 设置消息内容
            dialogComponent.SetMessage(fullMessage);

            // 绑定确认按钮点击事件：删除弹窗实例
            dialogComponent.SetConfirmCallback(() => OnConfirmButtonClicked(dialogInstance));

            // 添加到当前弹窗列表
            currentDialogs.Add(dialogInstance);
        }

        /// <summary>
        /// 获取错误类型的标题
        /// </summary>
        private string GetErrorTypeTitle(ErrorType type)
        {
            switch (type)
            {
                case ErrorType.Warning:
                    return "⚠️ 警告";
                case ErrorType.Error:
                    return "❌ 错误";
                case ErrorType.Info:
                    return "ℹ️ 信息";
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
        }

        private void OnDestroy()
        {
            // 清理所有弹窗
            ClearAllDialogs();

            // 清除单例引用
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 错误类型枚举
        /// </summary>
        private enum ErrorType
        {
            Warning,
            Error,
            Info
        }
    }
}

