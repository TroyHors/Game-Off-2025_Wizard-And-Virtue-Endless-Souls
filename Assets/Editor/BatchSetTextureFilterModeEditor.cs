using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 批量设置纹理Filter Mode为Point的编辑器插件
/// 功能：批量将所有纹理资源的Filter Mode设置为Point（无过滤模式）
/// </summary>
public class BatchSetTextureFilterModeEditor : EditorWindow
{
    // 调试日志前缀
    private const string LOG_PREFIX = "[BatchSetTextureFilterMode]";
    
    // 支持的纹理文件扩展名
    private static readonly string[] TEXTURE_EXTENSIONS = { ".png", ".jpg", ".jpeg", ".tga", ".tiff", ".bmp", ".psd", ".gif", ".exr", ".hdr" };
    
    // 搜索路径（默认搜索Assets目录）
    private string searchPath = "Assets";
    
    // 是否包含子目录
    private bool includeSubdirectories = true;
    
    // 处理进度
    private int processedCount = 0;
    private int totalCount = 0;
    private bool isProcessing = false;
    
    /// <summary>
    /// 打开窗口
    /// </summary>
    [MenuItem("Tools/批量设置纹理Filter Mode/设置为Point", false, 1)]
    public static void ShowWindow()
    {
        BatchSetTextureFilterModeEditor window = GetWindow<BatchSetTextureFilterModeEditor>("批量设置Filter Mode");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }
    
    /// <summary>
    /// 直接执行批量设置（菜单项）
    /// </summary>
    [MenuItem("Tools/批量设置纹理Filter Mode/快速设置所有纹理为Point", false, 11)]
    public static void QuickSetAllTexturesToPoint()
    {
        if (EditorUtility.DisplayDialog(
            "批量设置Filter Mode",
            "确定要将项目中所有纹理的Filter Mode设置为Point吗？\n\n此操作可能需要一些时间。",
            "确定",
            "取消"))
        {
            SetAllTexturesFilterModeToPoint("Assets", true);
        }
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        
        // 标题
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        EditorGUILayout.LabelField("批量设置纹理Filter Mode为Point", titleStyle);
        
        EditorGUILayout.Space(10);
        
        // 说明
        EditorGUILayout.HelpBox(
            "此工具将批量设置纹理的Filter Mode为Point（无过滤模式）。\n" +
            "Point模式适合像素艺术风格的2D游戏，可以保持像素的清晰度。",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // 搜索路径
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("搜索路径:", GUILayout.Width(100));
        searchPath = EditorGUILayout.TextField(searchPath);
        if (GUILayout.Button("选择文件夹", GUILayout.Width(100)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择文件夹", searchPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 转换为相对于Assets的路径
                string assetsPath = "Assets";
                if (selectedPath.Contains(Application.dataPath))
                {
                    searchPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择Assets目录下的文件夹", "确定");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // 包含子目录
        includeSubdirectories = EditorGUILayout.Toggle("包含子目录", includeSubdirectories);
        
        EditorGUILayout.Space(10);
        
        // 统计信息
        if (totalCount > 0)
        {
            EditorGUILayout.LabelField("统计信息", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"找到纹理数量: {totalCount}");
            EditorGUILayout.LabelField($"已处理数量: {processedCount}");
            
            if (isProcessing)
            {
                float progress = (float)processedCount / totalCount;
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, $"处理中... {processedCount}/{totalCount}");
            }
            
            EditorGUILayout.Space(10);
        }
        
        // 按钮
        EditorGUI.BeginDisabledGroup(isProcessing);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("扫描纹理", GUILayout.Height(30)))
        {
            ScanTextures();
        }
        
        if (GUILayout.Button("设置所有纹理为Point", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(
                "确认操作",
                $"确定要将找到的 {totalCount} 个纹理的Filter Mode设置为Point吗？",
                "确定",
                "取消"))
            {
                SetAllTexturesFilterModeToPoint(searchPath, includeSubdirectories);
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("设置整个项目为Point", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog(
                "确认操作",
                "确定要将整个项目中所有纹理的Filter Mode设置为Point吗？\n\n此操作可能需要较长时间。",
                "确定",
                "取消"))
            {
                SetAllTexturesFilterModeToPoint("Assets", true);
            }
        }
        
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(10);
        
        // 说明
        EditorGUILayout.HelpBox(
            "提示：\n" +
            "• 建议先使用\"扫描纹理\"查看会处理哪些文件\n" +
            "• 操作会修改纹理的导入设置，可能需要重新导入\n" +
            "• 建议在操作前备份项目",
            MessageType.Warning);
    }
    
    /// <summary>
    /// 扫描纹理文件
    /// </summary>
    private void ScanTextures()
    {
        totalCount = 0;
        processedCount = 0;
        
        List<string> texturePaths = FindTextureFiles(searchPath, includeSubdirectories);
        totalCount = texturePaths.Count;
        
        Debug.Log($"{LOG_PREFIX} 扫描完成，找到 {totalCount} 个纹理文件");
        
        // 刷新窗口
        Repaint();
    }
    
    /// <summary>
    /// 设置所有纹理的Filter Mode为Point
    /// </summary>
    private static void SetAllTexturesFilterModeToPoint(string path, bool includeSubdirectories)
    {
        List<string> texturePaths = FindTextureFiles(path, includeSubdirectories);
        int totalCount = texturePaths.Count;
        int processedCount = 0;
        int modifiedCount = 0;
        
        Debug.Log($"{LOG_PREFIX} 开始处理 {totalCount} 个纹理文件...");
        
        try
        {
            for (int i = 0; i < texturePaths.Count; i++)
            {
                string texturePath = texturePaths[i];
                
                // 显示进度条
                if (EditorUtility.DisplayCancelableProgressBar(
                    "批量设置Filter Mode",
                    $"正在处理: {Path.GetFileName(texturePath)} ({i + 1}/{totalCount})",
                    (float)(i + 1) / totalCount))
                {
                    Debug.LogWarning($"{LOG_PREFIX} 操作已取消");
                    break;
                }
                
                // 获取纹理导入器
                TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                
                if (textureImporter != null)
                {
                    // 检查是否需要修改
                    if (textureImporter.filterMode != FilterMode.Point)
                    {
                        textureImporter.filterMode = FilterMode.Point;
                        textureImporter.SaveAndReimport();
                        modifiedCount++;
                    }
                    
                    processedCount++;
                }
                else
                {
                    Debug.LogWarning($"{LOG_PREFIX} 无法获取纹理导入器: {texturePath}");
                }
            }
            
            EditorUtility.ClearProgressBar();
            
            Debug.Log($"{LOG_PREFIX} 处理完成！共处理 {processedCount} 个文件，修改了 {modifiedCount} 个文件");
            
            // 刷新资源数据库
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "完成",
                $"处理完成！\n\n共处理: {processedCount} 个文件\n修改了: {modifiedCount} 个文件",
                "确定");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"{LOG_PREFIX} 处理过程中发生错误: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"处理过程中发生错误:\n{e.Message}", "确定");
        }
    }
    
    /// <summary>
    /// 查找所有纹理文件
    /// </summary>
    private static List<string> FindTextureFiles(string searchPath, bool includeSubdirectories)
    {
        List<string> texturePaths = new List<string>();
        
        // 确保路径存在
        string fullPath = Path.Combine(Application.dataPath, searchPath.Replace("Assets/", "").Replace("Assets", ""));
        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning($"{LOG_PREFIX} 路径不存在: {searchPath}");
            return texturePaths;
        }
        
        // 获取所有文件
        SearchOption searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        string[] allFiles = Directory.GetFiles(fullPath, "*.*", searchOption);
        
        // 筛选纹理文件
        foreach (string filePath in allFiles)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            
            // 检查是否是支持的纹理格式
            bool isTexture = false;
            foreach (string texExt in TEXTURE_EXTENSIONS)
            {
                if (extension == texExt)
                {
                    isTexture = true;
                    break;
                }
            }
            
            if (isTexture)
            {
                // 转换为相对于Assets的路径
                string relativePath = "Assets" + filePath.Substring(Application.dataPath.Length).Replace('\\', '/');
                texturePaths.Add(relativePath);
            }
        }
        
        return texturePaths;
    }
}

