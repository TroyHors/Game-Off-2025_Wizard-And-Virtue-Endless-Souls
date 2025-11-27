using System.Collections.Generic;
using UnityEngine;
using WaveSystem;
using GameFlow;
using MapSystem;

namespace CharacterSystem
{
    /// <summary>
    /// 敌人配置生成器
    /// 在游戏开始时根据EnemyGenerationConfig为每种战斗类型生成EnemyConfig
    /// </summary>
    public class EnemyConfigGenerator : MonoBehaviour
    {
        [Header("生成配置")]
        [Tooltip("敌人生成配置（包含各战斗类型的生成参数）")]
        [SerializeField] private EnemyGenerationConfig generationConfig;

        [Header("生成的配置")]
        [Tooltip("生成的敌人配置字典（运行时使用，key为战斗类型，value为EnemyConfig）")]
        [SerializeField] private Dictionary<string, EnemyConfig> generatedConfigs = new Dictionary<string, EnemyConfig>();

        [Header("敌人实体设置")]
        [Tooltip("敌人实体Prefab（必须包含 HealthComponent 组件）")]
        [SerializeField] private GameObject enemyEntityPrefab;

        [Tooltip("敌人Tag（用于查找敌人）")]
        [SerializeField] private string enemyTag = "Enemy";

        /// <summary>
        /// 获取指定战斗类型的敌人配置
        /// </summary>
        /// <param name="nodeType">战斗类型</param>
        /// <returns>敌人配置，如果不存在则返回null</returns>
        public EnemyConfig GetEnemyConfig(string nodeType)
        {
            if (generatedConfigs.TryGetValue(nodeType, out EnemyConfig config))
            {
                return config;
            }
            return null;
        }

        private void Awake()
        {
            // 订阅游戏开始事件
            GameFlowManager gameFlowManager = FindObjectOfType<GameFlowManager>();
            if (gameFlowManager != null)
            {
                gameFlowManager.OnGameStart.AddListener(GenerateEnemyConfigs);
            }
            else
            {
                Debug.LogWarning("[EnemyConfigGenerator] 未找到 GameFlowManager，无法订阅游戏开始事件");
            }
        }

        private void OnDestroy()
        {
            // 取消订阅
            GameFlowManager gameFlowManager = FindObjectOfType<GameFlowManager>();
            if (gameFlowManager != null)
            {
                gameFlowManager.OnGameStart.RemoveListener(GenerateEnemyConfigs);
            }
        }

        /// <summary>
        /// 生成所有战斗类型的敌人配置（在游戏开始时调用）
        /// </summary>
        public void GenerateEnemyConfigs()
        {
            Debug.Log("[EnemyConfigGenerator] 开始生成敌人配置");

            if (generationConfig == null)
            {
                Debug.LogError("[EnemyConfigGenerator] 敌人生成配置未设置，无法生成敌人配置");
                return;
            }

            // 注意：enemyEntityPrefab可以为空，如果为空则会在EnemySpawner中动态创建敌人实体
            if (enemyEntityPrefab == null)
            {
                Debug.LogWarning("[EnemyConfigGenerator] 敌人实体Prefab未设置，将在生成敌人时动态创建实体（白模）");
            }

            // 获取地图的最大层数
            int maxLayerCount = GetMapMaxLayerCount();
            Debug.Log($"[EnemyConfigGenerator] 地图最大层数: {maxLayerCount}，将为此数量生成敌人配置");

            // 清空之前的配置
            generatedConfigs.Clear();

            // 为每种战斗类型生成配置
            foreach (var typeConfig in generationConfig.TypeConfigs)
            {
                if (typeConfig == null)
                {
                    continue;
                }

                string nodeType = typeConfig.nodeType;
                if (string.IsNullOrEmpty(nodeType))
                {
                    Debug.LogWarning("[EnemyConfigGenerator] 跳过空的战斗类型配置");
                    continue;
                }

                Debug.Log($"[EnemyConfigGenerator] 为战斗类型 '{nodeType}' 生成敌人配置");

                // 创建EnemyConfig（运行时创建，不保存为资源）
                EnemyConfig enemyConfig = ScriptableObject.CreateInstance<EnemyConfig>();
                enemyConfig.name = $"GeneratedEnemyConfig_{nodeType}";

                // 设置敌人实体Prefab和Tag（通过反射设置私有字段）
                SetEnemyConfigPrefab(enemyConfig, enemyEntityPrefab);
                SetEnemyConfigTag(enemyConfig, enemyTag);

                // 生成敌人配置数据（使用地图最大层数）
                List<EnemyConfigData> configDataList = GenerateEnemyConfigDataList(typeConfig, maxLayerCount);
                SetEnemyConfigDataList(enemyConfig, configDataList);

                generatedConfigs[nodeType] = enemyConfig;

                Debug.Log($"[EnemyConfigGenerator] 为战斗类型 '{nodeType}' 生成了 {configDataList.Count} 个敌人配置");
            }

            Debug.Log($"[EnemyConfigGenerator] 完成生成敌人配置，共 {generatedConfigs.Count} 种战斗类型");
        }

        /// <summary>
        /// 获取地图的最大层数
        /// </summary>
        private int GetMapMaxLayerCount()
        {
            // 尝试从MapManager获取地图拓扑
            MapManager mapManager = FindObjectOfType<MapManager>();
            if (mapManager != null)
            {
                // 优先使用已生成的地图拓扑的层数
                if (mapManager.CurrentTopology != null)
                {
                    int height = mapManager.CurrentTopology.Height;
                    Debug.Log($"[EnemyConfigGenerator] 从MapTopology获取地图层数: {height}");
                    return height;
                }
                
                // 如果地图还未生成，使用配置中的层数
                if (mapManager.Config != null)
                {
                    int height = mapManager.Config.height;
                    Debug.Log($"[EnemyConfigGenerator] 从MapGenerationConfig获取地图层数: {height}");
                    return height;
                }
            }
            
            // 如果找不到MapManager，使用默认值
            Debug.LogWarning("[EnemyConfigGenerator] 未找到MapManager，使用默认层数10");
            return 10;
        }

        /// <summary>
        /// 生成敌人配置数据列表
        /// 为整局游戏生成足够多的敌人配置（每个战斗类型生成多个敌人配置）
        /// </summary>
        /// <param name="typeConfig">战斗类型配置</param>
        /// <param name="maxLayerCount">地图最大层数</param>
        private List<EnemyConfigData> GenerateEnemyConfigDataList(EnemyTypeGenerationConfig typeConfig, int maxLayerCount)
        {
            List<EnemyConfigData> configDataList = new List<EnemyConfigData>();

            // 确定要生成的敌人数量（使用地图最大层数）
            // 如果Figure列表为空，生成默认配置；否则根据Figure数量生成，但至少生成maxLayerCount个配置
            int enemyCount = maxLayerCount; // 使用地图最大层数
            if (typeConfig.enemyFigures != null && typeConfig.enemyFigures.Count > 0)
            {
                // 如果Figure数量少于maxLayerCount，循环使用；如果多于maxLayerCount，使用所有Figure
                enemyCount = Mathf.Max(maxLayerCount, typeConfig.enemyFigures.Count);
            }

            // 生成指定数量的敌人配置
            for (int i = 0; i < enemyCount; i++)
            {
                // 从Figure列表中循环选择（如果列表为空则为null）
                Sprite figure = null;
                if (typeConfig.enemyFigures != null && typeConfig.enemyFigures.Count > 0)
                {
                    figure = typeConfig.enemyFigures[i % typeConfig.enemyFigures.Count];
                }

                EnemyConfigData configData = CreateEnemyConfigData(typeConfig, figure, i);
                configDataList.Add(configData);
            }

            Debug.Log($"[EnemyConfigGenerator] 为战斗类型 '{typeConfig.nodeType}' 生成了 {configDataList.Count} 个敌人配置");
            return configDataList;
        }

        /// <summary>
        /// 创建单个敌人配置数据
        /// </summary>
        private EnemyConfigData CreateEnemyConfigData(EnemyTypeGenerationConfig typeConfig, Sprite figure, int index)
        {
            EnemyConfigData configData = new EnemyConfigData();

            // 设置敌人名称（使用Figure名称，如果没有则使用默认名称）
            if (figure != null)
            {
                configData.enemyName = figure.name;
            }
            else
            {
                configData.enemyName = $"Enemy_{typeConfig.nodeType}_{index}";
            }

            // 设置敌人外观图片
            configData.enemyFigure = figure;

            // 随机生成生命值
            int health = Random.Range(typeConfig.healthRange.x, typeConfig.healthRange.y + 1);
            configData.maxHealth = health;

            // 生成初始波数据（回合开始时会重新生成，这里只是占位）
            WaveData initialWaveData = GenerateRandomWaveData(typeConfig);
            configData.waveData = initialWaveData;
            configData.presetWaveIndex = -1; // 使用自定义波数据

            // 保存波生成配置（用于回合开始时生成随机波）
            configData.waveGenerationConfig = typeConfig;

            return configData;
        }

        /// <summary>
        /// 生成随机波数据（根据配置的方法和参数）
        /// </summary>
        private WaveData GenerateRandomWaveData(EnemyTypeGenerationConfig typeConfig)
        {
            // 使用独立的波生成器
            return EnemyWaveGenerator.GenerateRandomWaveData(typeConfig);
        }

        /// <summary>
        /// 设置EnemyConfig的Prefab（通过反射）
        /// </summary>
        private void SetEnemyConfigPrefab(EnemyConfig config, GameObject prefab)
        {
            var field = typeof(EnemyConfig).GetField("enemyEntityPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(config, prefab);
            }
        }

        /// <summary>
        /// 设置EnemyConfig的Tag（通过反射）
        /// </summary>
        private void SetEnemyConfigTag(EnemyConfig config, string tag)
        {
            var field = typeof(EnemyConfig).GetField("enemyTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(config, tag);
            }
        }

        /// <summary>
        /// 设置EnemyConfig的配置数据列表（通过反射）
        /// </summary>
        private void SetEnemyConfigDataList(EnemyConfig config, List<EnemyConfigData> dataList)
        {
            var field = typeof(EnemyConfig).GetField("enemyConfigs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(config, dataList);
            }
        }
    }
}

