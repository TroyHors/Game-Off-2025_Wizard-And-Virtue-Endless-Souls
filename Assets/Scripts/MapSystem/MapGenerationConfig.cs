using UnityEngine;

namespace MapSystem
{
    /// <summary>
    /// 地图生成配置
    /// ScriptableObject,可在Inspector中配置不同章节/难度的参数
    /// </summary>
    [CreateAssetMenu(fileName = "MapGenerationConfig", menuName = "Map System/Map Generation Config")]
    public class MapGenerationConfig : ScriptableObject
    {
        [Header("基础参数")]
        [Tooltip("地图高度(层数)")]
        public int height = 15;

        [Tooltip("地图宽度(每层最大节点数)")]
        public int width = 5;

        [Header("拓扑生成参数")]
        [Tooltip("每层最小节点数")]
        [Range(1, 10)]
        public int minNodesPerLayer = 2;

        [Tooltip("每层最大节点数")]
        [Range(1, 10)]
        public int maxNodesPerLayer = 4;

        [Tooltip("每个节点平均出度(向上连接数)")]
        [Range(1f, 5f)]
        public float avgOutDegree = 2.0f;

        [Tooltip("出度随机范围(实际出度 = avgOutDegree ± outDegreeVariance)")]
        [Range(0f, 2f)]
        public float outDegreeVariance = 0.5f;

        [Tooltip("连接跨度(节点可以连接到上层多少列范围内的节点)")]
        [Range(1, 5)]
        public int connectionSpan = 2;

        [Tooltip("分叉概率(节点连接到多个上层节点的概率)")]
        [Range(0f, 1f)]
        public float branchProbability = 0.6f;

        [Tooltip("汇合概率(多个下层节点连接到同一个上层节点的概率)")]
        [Range(0f, 1f)]
        public float mergeProbability = 0.4f;

        [Header("内容填充参数")]
        [Tooltip("是否启用内容填充")]
        public bool enableContentFilling = true;

        [Tooltip("内容填充配置(在Inspector中配置节点类型和权重)")]
        public NodeTypeConfig[] nodeTypeConfigs;

        [Header("路径合理性约束")]
        [Tooltip("是否启用路径合理性检查")]
        public bool enablePathValidation = true;

        [Tooltip("每条路径上最少营火节点数(由约束接口实现)")]
        public int minRestNodesPerPath = 2;

        [Tooltip("相邻层最多连续精英节点数(由约束接口实现)")]
        public int maxConsecutiveEliteNodes = 2;

        [Header("可视化配置")]
        [Tooltip("Boss节点图像")]
        public Sprite bossNodeSprite;

        [Tooltip("Boss节点大小")]
        public Vector2 bossNodeSize = new Vector2(100, 100);

        [Tooltip("默认节点图像(当节点类型未配置时使用)")]
        public Sprite defaultNodeSprite;

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool Validate()
        {
            if (height < 2)
            {
                Debug.LogError("[MapGenerationConfig] 地图高度必须至少为2");
                return false;
            }

            if (width < 1)
            {
                Debug.LogError("[MapGenerationConfig] 地图宽度必须至少为1");
                return false;
            }

            if (minNodesPerLayer > maxNodesPerLayer)
            {
                Debug.LogError("[MapGenerationConfig] 每层最小节点数不能大于最大节点数");
                return false;
            }

            if (maxNodesPerLayer > width)
            {
                Debug.LogError($"[MapGenerationConfig] 每层最大节点数不能超过地图宽度{width}");
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 节点类型配置
    /// 用于在Inspector中配置节点类型及其权重和可视化
    /// </summary>
    [System.Serializable]
    public class NodeTypeConfig
    {
        [Tooltip("节点类型标识(用于导向对应的事件系统)")]
        public string nodeType = "Combat";

        [Tooltip("全局权重(在所有层级中的基础权重)")]
        [Range(0f, 100f)]
        public float globalWeight = 50f;

        [Tooltip("层级权重曲线(按层数调整权重,0为底层,1为顶层)")]
        public AnimationCurve layerWeightCurve = AnimationCurve.Linear(0, 1, 1, 1);

        [Tooltip("最小出现次数(全图)")]
        public int minCount = 0;

        [Tooltip("最大出现次数(全图)")]
        public int maxCount = -1; // -1表示无限制

        [Tooltip("最小层间隔(同类型节点之间必须至少相隔多少层,0为无限制可以同层)")]
        [Range(0, 10)]
        public int minLayerInterval = 0;

        [Header("可视化")]
        [Tooltip("该节点类型对应的图像素材")]
        public Sprite sprite;

        [Tooltip("节点大小")]
        public Vector2 nodeSize = new Vector2(80, 80);

        [Header("事件流程")]
        [Tooltip("该节点类型对应的事件流程Prefab（必须包含实现 INodeEventFlow 的组件）")]
        public UnityEngine.GameObject flowPrefab;

        public NodeTypeConfig()
        {
            layerWeightCurve = AnimationCurve.Linear(0, 1, 1, 1);
        }
    }
}

