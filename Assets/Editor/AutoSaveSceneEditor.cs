using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 自动保存场景编辑器插件
/// 功能：定期自动保存当前场景，防止意外丢失工作
/// </summary>
[InitializeOnLoad]
public class AutoSaveSceneEditor
{
    // 配置键名（使用统一前缀）
    private const string PREF_ENABLED = "AutoSaveScene_Enabled";
    private const string PREF_INTERVAL = "AutoSaveScene_Interval";
    private const string PREF_LAST_SAVE_TIME = "AutoSaveScene_LastSaveTime";
    
    // 默认配置
    private const bool DEFAULT_ENABLED = true;
    private const float DEFAULT_INTERVAL = 300f; // 默认5分钟（300秒）
    
    // 调试日志前缀
    private const string LOG_PREFIX = "[AutoSaveScene]";
    
    private static float lastSaveTime = 0f;
    
    /// <summary>
    /// 静态构造函数，在Unity编辑器启动时自动执行
    /// </summary>
    static AutoSaveSceneEditor()
    {
        // 初始化上次保存时间
        lastSaveTime = EditorPrefs.GetFloat(PREF_LAST_SAVE_TIME, 0f);
        
        // 注册编辑器更新回调
        EditorApplication.update += OnEditorUpdate;
        
        // 监听场景变化
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }
    
    /// <summary>
    /// 编辑器更新回调，定期检查是否需要保存
    /// </summary>
    private static void OnEditorUpdate()
    {
        // 检查是否启用自动保存
        if (!IsEnabled())
        {
            return;
        }
        
        // 检查是否在播放模式（播放模式下不自动保存）
        if (EditorApplication.isPlaying)
        {
            return;
        }
        
        // 检查是否有场景打开
        if (EditorSceneManager.GetActiveScene().path == null || 
            string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
        {
            return;
        }
        
        // 检查是否到了保存时间
        float currentTime = (float)EditorApplication.timeSinceStartup;
        float interval = GetInterval();
        
        if (currentTime - lastSaveTime >= interval)
        {
            SaveCurrentScene();
            lastSaveTime = currentTime;
            EditorPrefs.SetFloat(PREF_LAST_SAVE_TIME, currentTime);
        }
    }
    
    /// <summary>
    /// 场景打开时的回调，重置保存时间
    /// </summary>
    private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        // 场景打开时重置保存时间，避免立即保存
        lastSaveTime = (float)EditorApplication.timeSinceStartup;
        EditorPrefs.SetFloat(PREF_LAST_SAVE_TIME, lastSaveTime);
    }
    
    /// <summary>
    /// 保存当前场景
    /// </summary>
    private static void SaveCurrentScene()
    {
        try
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            
            // 检查场景是否有路径（已保存过的场景）
            if (string.IsNullOrEmpty(activeScene.path))
            {
                // 未保存的场景，尝试保存为新场景
                string scenePath = EditorUtility.SaveFilePanelInProject(
                    "保存场景",
                    "NewScene",
                    "unity",
                    "请选择保存位置");
                
                if (string.IsNullOrEmpty(scenePath))
                {
                    // 用户取消了保存，不记录错误
                    return;
                }
                
                EditorSceneManager.SaveScene(activeScene, scenePath);
                Debug.Log($"{LOG_PREFIX} 场景已保存为新文件: {scenePath}");
            }
            else
            {
                // 已保存的场景，直接保存
                bool saved = EditorSceneManager.SaveScene(activeScene);
                
                if (saved)
                {
                    Debug.Log($"{LOG_PREFIX} 场景已自动保存: {activeScene.path}");
                }
                else
                {
                    Debug.LogWarning($"{LOG_PREFIX} 场景保存失败: {activeScene.path}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{LOG_PREFIX} 自动保存场景时发生错误: {e.Message}");
        }
    }
    
    /// <summary>
    /// 检查是否启用自动保存
    /// </summary>
    public static bool IsEnabled()
    {
        return EditorPrefs.GetBool(PREF_ENABLED, DEFAULT_ENABLED);
    }
    
    /// <summary>
    /// 设置是否启用自动保存
    /// </summary>
    public static void SetEnabled(bool enabled)
    {
        EditorPrefs.SetBool(PREF_ENABLED, enabled);
        Debug.Log($"{LOG_PREFIX} 自动保存已{(enabled ? "启用" : "禁用")}");
    }
    
    /// <summary>
    /// 获取保存间隔（秒）
    /// </summary>
    public static float GetInterval()
    {
        return EditorPrefs.GetFloat(PREF_INTERVAL, DEFAULT_INTERVAL);
    }
    
    /// <summary>
    /// 设置保存间隔（秒）
    /// </summary>
    public static void SetInterval(float interval)
    {
        if (interval < 10f)
        {
            Debug.LogWarning($"{LOG_PREFIX} 保存间隔不能小于10秒，已设置为10秒");
            interval = 10f;
        }
        
        EditorPrefs.SetFloat(PREF_INTERVAL, interval);
        Debug.Log($"{LOG_PREFIX} 保存间隔已设置为: {interval}秒");
    }
    
    /// <summary>
    /// 手动触发保存（菜单项）
    /// </summary>
    [MenuItem("Tools/自动保存场景/立即保存当前场景", false, 1)]
    public static void SaveSceneNow()
    {
        SaveCurrentScene();
    }
    
    /// <summary>
    /// 切换自动保存开关（菜单项）
    /// </summary>
    [MenuItem("Tools/自动保存场景/启用自动保存", false, 11)]
    public static void EnableAutoSave()
    {
        SetEnabled(true);
    }
    
    [MenuItem("Tools/自动保存场景/启用自动保存", true)]
    public static bool EnableAutoSaveValidate()
    {
        Menu.SetChecked("Tools/自动保存场景/启用自动保存", IsEnabled());
        return !IsEnabled();
    }
    
    [MenuItem("Tools/自动保存场景/禁用自动保存", false, 12)]
    public static void DisableAutoSave()
    {
        SetEnabled(false);
    }
    
    [MenuItem("Tools/自动保存场景/禁用自动保存", true)]
    public static bool DisableAutoSaveValidate()
    {
        return IsEnabled();
    }
    
    /// <summary>
    /// 打开设置窗口（菜单项）
    /// </summary>
    [MenuItem("Tools/自动保存场景/设置", false, 21)]
    public static void OpenSettings()
    {
        AutoSaveSceneSettingsWindow.ShowWindow();
    }
}

/// <summary>
/// 自动保存场景设置窗口
/// </summary>
public class AutoSaveSceneSettingsWindow : EditorWindow
{
    private bool enabled;
    private float interval;
    private float lastSaveTime;
    
    private const string LOG_PREFIX = "[AutoSaveScene]";
    
    [MenuItem("Tools/自动保存场景/设置", false, 21)]
    public static void ShowWindow()
    {
        AutoSaveSceneSettingsWindow window = GetWindow<AutoSaveSceneSettingsWindow>("自动保存场景设置");
        window.minSize = new Vector2(300, 150);
        window.Show();
    }
    
    private void OnEnable()
    {
        LoadSettings();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        
        // 标题
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        EditorGUILayout.LabelField("自动保存场景设置", titleStyle);
        
        EditorGUILayout.Space(10);
        
        // 启用开关
        enabled = EditorGUILayout.Toggle("启用自动保存", enabled);
        
        EditorGUILayout.Space(5);
        
        // 保存间隔
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("保存间隔（秒）", GUILayout.Width(120));
        interval = EditorGUILayout.Slider(interval, 10f, 1800f);
        EditorGUILayout.EndHorizontal();
        
        // 显示间隔时间（分钟）
        float minutes = interval / 60f;
        EditorGUILayout.LabelField($"  ({minutes:F1} 分钟)", EditorStyles.miniLabel);
        
        EditorGUILayout.Space(10);
        
        // 状态信息
        EditorGUILayout.LabelField("状态信息", EditorStyles.boldLabel);
        
        if (enabled)
        {
            EditorGUILayout.LabelField("状态:", "已启用", EditorStyles.helpBox);
            
            // 计算下次保存时间
            float currentTime = (float)EditorApplication.timeSinceStartup;
            float timeSinceLastSave = currentTime - lastSaveTime;
            float timeUntilNextSave = interval - timeSinceLastSave;
            
            if (timeUntilNextSave > 0)
            {
                EditorGUILayout.LabelField("距离下次保存:", $"{timeUntilNextSave:F0} 秒", EditorStyles.helpBox);
            }
            else
            {
                EditorGUILayout.LabelField("距离下次保存:", "即将保存", EditorStyles.helpBox);
            }
        }
        else
        {
            EditorGUILayout.LabelField("状态:", "已禁用", EditorStyles.helpBox);
        }
        
        EditorGUILayout.Space(10);
        
        // 按钮
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("应用设置"))
        {
            ApplySettings();
        }
        
        if (GUILayout.Button("立即保存场景"))
        {
            AutoSaveSceneEditor.SaveSceneNow();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("重置为默认值"))
        {
            ResetToDefault();
        }
    }
    
    private void LoadSettings()
    {
        enabled = AutoSaveSceneEditor.IsEnabled();
        interval = AutoSaveSceneEditor.GetInterval();
        lastSaveTime = EditorPrefs.GetFloat("AutoSaveScene_LastSaveTime", 0f);
    }
    
    private void ApplySettings()
    {
        AutoSaveSceneEditor.SetEnabled(enabled);
        AutoSaveSceneEditor.SetInterval(interval);
        lastSaveTime = EditorPrefs.GetFloat("AutoSaveScene_LastSaveTime", 0f);
        
        Debug.Log($"{LOG_PREFIX} 设置已应用: 启用={enabled}, 间隔={interval}秒");
        
        // 刷新窗口
        Repaint();
    }
    
    private void ResetToDefault()
    {
        enabled = true;
        interval = 300f; // 5分钟
        ApplySettings();
    }
    
    private void Update()
    {
        // 定期刷新窗口以更新倒计时
        if (enabled)
        {
            Repaint();
        }
    }
}

