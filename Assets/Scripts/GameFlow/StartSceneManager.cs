using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameFlow
{
    /// <summary>
    /// 开始场景管理器
    /// 处理开始场景的UI交互和场景切换
    /// </summary>
    public class StartSceneManager : MonoBehaviour
    {
        [Header("UI按钮")]
        [Tooltip("开始游戏按钮（如果为空，会自动查找）")]
        [SerializeField] private Button startButton;

        [Tooltip("退出游戏按钮（如果为空，会自动查找）")]
        [SerializeField] private Button quitButton;

        [Header("场景设置")]
        [Tooltip("主游戏场景名称（切换到Main场景）")]
        [SerializeField] private string mainSceneName = "Main";

        private void Awake()
        {
            // 如果没有设置按钮，尝试自动查找
            if (startButton == null)
            {
                // 查找名称包含"Start"的按钮
                Button[] buttons = FindObjectsOfType<Button>();
                foreach (Button btn in buttons)
                {
                    if (btn.name.Contains("Start") || btn.name.Contains("start"))
                    {
                        startButton = btn;
                        Debug.Log($"[StartSceneManager] 自动找到开始按钮: {btn.name}");
                        break;
                    }
                }
            }

            if (quitButton == null)
            {
                // 查找名称包含"Quit"的按钮
                Button[] buttons = FindObjectsOfType<Button>();
                foreach (Button btn in buttons)
                {
                    if (btn.name.Contains("Quit") || btn.name.Contains("quit"))
                    {
                        quitButton = btn;
                        Debug.Log($"[StartSceneManager] 自动找到退出按钮: {btn.name}");
                        break;
                    }
                }
            }
        }

        private void Start()
        {
            // 订阅按钮事件
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
                Debug.Log("[StartSceneManager] 开始按钮已订阅");
            }
            else
            {
                Debug.LogWarning("[StartSceneManager] 开始按钮未找到，无法开始游戏");
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
                Debug.Log("[StartSceneManager] 退出按钮已订阅");
            }
            else
            {
                Debug.LogWarning("[StartSceneManager] 退出按钮未找到，无法退出游戏");
            }
        }

        private void OnDestroy()
        {
            // 取消订阅按钮事件
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(OnStartButtonClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            }
        }

        /// <summary>
        /// 开始按钮点击事件处理
        /// 切换到主游戏场景
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("[StartSceneManager] 开始按钮被点击，准备切换到主游戏场景");
            
            if (string.IsNullOrEmpty(mainSceneName))
            {
                Debug.LogError("[StartSceneManager] 主游戏场景名称未设置，无法切换场景");
                return;
            }

            // 加载主游戏场景
            SceneManager.LoadScene(mainSceneName);
            Debug.Log($"[StartSceneManager] 正在加载场景: {mainSceneName}");
        }

        /// <summary>
        /// 退出按钮点击事件处理
        /// 直接结束游戏进程
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("[StartSceneManager] 退出按钮被点击，准备退出游戏");
            
            // 在Unity编辑器中，Application.Quit()不会真正退出，所以使用不同的处理方式
            #if UNITY_EDITOR
                Debug.Log("[StartSceneManager] 退出游戏（编辑器模式，不会真正退出）");
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Debug.Log("[StartSceneManager] 退出游戏");
                Application.Quit();
            #endif
        }
    }
}

