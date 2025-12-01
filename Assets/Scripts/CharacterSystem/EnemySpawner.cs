using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 添加UI命名空间，用于Image组件
using DamageSystem;
using WaveSystem;

namespace CharacterSystem
{
    /// <summary>
    /// 敌人生成器
    /// 根据敌人配置动态生成敌人实体
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("敌人配置")]
        [Tooltip("敌人配置（包含敌人数据和Prefab，如果为空则从EnemyConfigGenerator获取）")]
        [SerializeField] private EnemyConfig enemyConfig;

        [Tooltip("敌人配置生成器（用于获取生成的敌人配置）")]
        [SerializeField] private EnemyConfigGenerator enemyConfigGenerator;

        [Header("生成设置")]
        [Tooltip("敌人生成位置列表（按顺序生成）")]
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

        [Tooltip("如果没有设置生成位置，使用默认偏移")]
        [SerializeField] private Vector3 defaultOffset = Vector3.zero;

        [Tooltip("默认生成间距（当没有设置生成位置时使用）")]
        [SerializeField] private float defaultSpacing = 2f;

        [Header("运行时状态")]
        [Tooltip("当前生成的敌人实体列表")]
        [SerializeField] private List<GameObject> currentEnemies = new List<GameObject>();

        [Tooltip("战斗计数器字典（key为战斗类型，value为该类型的战斗计数）")]
        [SerializeField] private Dictionary<string, int> combatCounters = new Dictionary<string, int>();

        /// <summary>
        /// 敌人配置
        /// </summary>
        public EnemyConfig EnemyConfig
        {
            get => enemyConfig;
            set => enemyConfig = value;
        }

        /// <summary>
        /// 当前生成的敌人实体列表
        /// </summary>
        public List<GameObject> CurrentEnemies => new List<GameObject>(currentEnemies);

        /// <summary>
        /// 当前敌人数量
        /// </summary>
        public int EnemyCount => currentEnemies.Count;

        /// <summary>
        /// 获取指定战斗类型的战斗计数
        /// </summary>
        /// <param name="nodeType">战斗类型</param>
        /// <returns>该战斗类型的战斗计数</returns>
        public int GetCombatCounter(string nodeType)
        {
            if (string.IsNullOrEmpty(nodeType))
            {
                return 0;
            }
            return combatCounters.TryGetValue(nodeType, out int count) ? count : 0;
        }

        /// <summary>
        /// 根据配置生成敌人（战斗开始时调用）
        /// 敌人依次生成：第一次战斗生成第一个配置的敌人，第二次战斗生成第二个配置的敌人，以此类推
        /// 所有敌人都生成在同一个位置（使用第一个生成位置或默认位置）
        /// </summary>
        /// <param name="configIndices">要使用的配置索引列表（如果为null，则按战斗计数依次生成单个敌人）</param>
        /// <param name="nodeType">战斗类型（用于从EnemyConfigGenerator获取配置）</param>
        /// <returns>生成的敌人实体列表</returns>
        public List<GameObject> SpawnEnemies(List<int> configIndices = null, string nodeType = null)
        {
            // 先清除现有敌人
            ClearAllEnemies();

            // 优先从EnemyConfigGenerator获取对应nodeType的配置
            // 如果提供了nodeType，必须从Generator获取匹配的配置，不能使用Inspector中可能不匹配的配置
            if (!string.IsNullOrEmpty(nodeType))
            {
                if (enemyConfigGenerator == null)
                {
                    enemyConfigGenerator = FindObjectOfType<EnemyConfigGenerator>();
                }

                if (enemyConfigGenerator != null)
                {
                    Debug.Log($"[EnemySpawner] 尝试从EnemyConfigGenerator获取战斗类型 '{nodeType}' 的敌人配置");
                    EnemyConfig targetConfig = enemyConfigGenerator.GetEnemyConfig(nodeType);
                    if (targetConfig != null)
                    {
                        Debug.Log($"[EnemySpawner] 成功获取到战斗类型 '{nodeType}' 的敌人配置，包含 {targetConfig.ConfigCount} 个配置数据");
                        enemyConfig = targetConfig; // 使用获取到的配置
                    }
                    else
                    {
                        // 获取已生成的配置类型用于错误信息
                    var generatedTypes = new System.Collections.Generic.List<string>();
                    if (enemyConfigGenerator != null)
                    {
                        // 通过反射或直接访问字典来获取类型列表（如果GetAllGeneratedNodeTypes不存在）
                        try
                        {
                            var method = enemyConfigGenerator.GetType().GetMethod("GetAllGeneratedNodeTypes");
                            if (method != null)
                            {
                                generatedTypes = (System.Collections.Generic.List<string>)method.Invoke(enemyConfigGenerator, null);
                            }
                        }
                        catch { }
                    }
                    string typesStr = generatedTypes.Count > 0 ? string.Join(", ", generatedTypes) : "未知";
                    Debug.LogError($"[EnemySpawner] 无法获取战斗类型 '{nodeType}' 的敌人配置！已生成的配置类型: {typesStr}。可能原因：1) EnemyGenerationConfig中未配置此类型 2) 节点类型字符串不匹配（如'精英'vs'Elite'）");
                        return new List<GameObject>(); // 找不到匹配配置，不生成敌人
                    }
                }
                else
                {
                    Debug.LogError($"[EnemySpawner] EnemyConfigGenerator未找到，无法获取战斗类型 '{nodeType}' 的敌人配置");
                    return new List<GameObject>();
                }
            }
            // 如果没有提供nodeType，尝试使用Inspector中设置的enemyConfig（向后兼容）
            else if (enemyConfig == null)
            {
                Debug.LogWarning("[EnemySpawner] 未提供nodeType且Inspector中未设置enemyConfig，无法生成敌人");
                return new List<GameObject>();
            }
            
            if (enemyConfig == null)
            {
                Debug.LogError("[EnemySpawner] 敌人配置未设置，无法生成敌人");
                return new List<GameObject>();
            }
            
            Debug.Log($"[EnemySpawner] 使用敌人配置: ConfigCount={enemyConfig.ConfigCount}");

            // 注意：enemyEntityPrefab可以为空，如果为空则动态创建敌人实体（白模）
            if (enemyConfig.EnemyEntityPrefab == null)
            {
                Debug.LogWarning("[EnemySpawner] 敌人实体Prefab未设置，将动态创建敌人实体（白模）");
            }

            List<GameObject> spawnedEnemies = new List<GameObject>();

            // 确定要使用的配置索引
            List<int> indicesToUse = configIndices;
            if (indicesToUse == null || indicesToUse.Count == 0)
            {
                // 获取当前战斗类型的计数器（每个战斗类型有独立的计数器）
                if (string.IsNullOrEmpty(nodeType))
                {
                    Debug.LogWarning("[EnemySpawner] 战斗类型为空，无法获取正确的计数器，使用默认值0");
                    nodeType = "Unknown";
                }
                
                // 获取或初始化该战斗类型的计数器
                if (!combatCounters.ContainsKey(nodeType))
                {
                    combatCounters[nodeType] = 0;
                }
                
                int combatCounter = combatCounters[nodeType];
                
                // 如果没有指定，按战斗计数依次生成单个敌人
                // 第一次战斗生成第一个配置（索引0），第二次战斗生成第二个配置（索引1），以此类推
                if (combatCounter >= enemyConfig.ConfigCount)
                {
                    Debug.LogWarning($"[EnemySpawner] 战斗类型 '{nodeType}' 的战斗计数 {combatCounter} 超出配置数量 {enemyConfig.ConfigCount}，循环使用配置");
                    // 循环使用配置
                    indicesToUse = new List<int> { combatCounter % enemyConfig.ConfigCount };
                }
                else
                {
                    indicesToUse = new List<int> { combatCounter };
                }
                
                // 增加该战斗类型的战斗计数（用于下次战斗）
                combatCounters[nodeType] = combatCounter + 1;
                
                Debug.Log($"[EnemySpawner] 战斗类型 '{nodeType}' 使用配置索引 {indicesToUse[0]}（计数器：{combatCounter} -> {combatCounters[nodeType]}）");
            }

            // 生成敌人（所有敌人都生成在同一个位置）
            for (int i = 0; i < indicesToUse.Count; i++)
            {
                int configIndex = indicesToUse[i];
                EnemyConfigData configData = enemyConfig.GetEnemyConfig(configIndex);
                if (configData == null)
                {
                    Debug.LogWarning($"[EnemySpawner] 配置索引 {configIndex} 无效，跳过");
                    continue;
                }

                // 确定生成位置（所有敌人都生成在同一个位置）
                Vector3 spawnPosition;
                Quaternion spawnRotation = Quaternion.identity;

                // 使用第一个生成位置（如果存在），否则使用默认位置
                if (spawnPoints.Count > 0 && spawnPoints[0] != null)
                {
                    spawnPosition = spawnPoints[0].position;
                    spawnRotation = spawnPoints[0].rotation;
                }
                else
                {
                    // 使用默认位置（所有敌人都生成在同一个位置）
                    spawnPosition = transform.position + defaultOffset;
                }

                // 确定父对象（使用第一个spawn point作为父对象）
                Transform parentTransform = (spawnPoints.Count > 0 && spawnPoints[0] != null) ? spawnPoints[0] : null;

                // 创建敌人实体（如果Prefab存在则使用Prefab，否则动态创建）
                GameObject enemyEntity;
                if (enemyConfig.EnemyEntityPrefab != null)
                {
                    // 作为spawn point的子对象生成
                    enemyEntity = Instantiate(enemyConfig.EnemyEntityPrefab, spawnPosition, spawnRotation, parentTransform);
                }
                else
                {
                    // 动态创建敌人实体（白模），作为spawn point的子对象
                    enemyEntity = new GameObject(configData.enemyName);
                    enemyEntity.transform.SetParent(parentTransform, false); // 使用false保持世界位置
                    enemyEntity.transform.position = spawnPosition;
                    enemyEntity.transform.rotation = spawnRotation;
                    
                    // 添加HealthComponent
                    enemyEntity.AddComponent<HealthComponent>();
                    
                    // 添加EnemyWaveManager（必需，用于管理敌人波数据）
                    enemyEntity.AddComponent<EnemyWaveManager>();
                    
                    // 如果配置中有外观图片，添加Image组件
                    if (configData.enemyFigure != null)
                    {
                        Image image = enemyEntity.AddComponent<Image>();
                        image.sprite = configData.enemyFigure;
                    }
                }
                
                enemyEntity.name = $"{configData.enemyName}_{i}";
                enemyEntity.tag = enemyConfig.EnemyTag;

                // 获取 HealthComponent 并应用配置
                HealthComponent healthComponent = enemyEntity.GetComponent<HealthComponent>();
                if (healthComponent == null)
                {
                    Debug.LogError($"[EnemySpawner] 敌人实体缺少 HealthComponent 组件，已销毁：{enemyEntity.name}");
                    Destroy(enemyEntity);
                    continue;
                }
                
                // 如果配置中有外观图片，设置Image组件
                if (configData.enemyFigure != null)
                {
                    Image image = enemyEntity.GetComponent<Image>();
                    if (image == null)
                    {
                        image = enemyEntity.AddComponent<Image>();
                    }
                    image.sprite = configData.enemyFigure;
                }

                // 设置生命值
                healthComponent.SetMaxHealth(configData.maxHealth);
                healthComponent.ResetHealth();

                // 确保有EnemyWaveManager组件（如果没有则添加）
                EnemyWaveManager waveManager = enemyEntity.GetComponent<EnemyWaveManager>();
                if (waveManager == null)
                {
                    Debug.LogWarning($"[EnemySpawner] 敌人实体 {enemyEntity.name} 缺少 EnemyWaveManager 组件，自动添加");
                    waveManager = enemyEntity.AddComponent<EnemyWaveManager>();
                }
                
                if (waveManager != null)
                {
                    if (configData.presetWaveIndex >= 0)
                    {
                        // 使用预设波索引
                        waveManager.LoadPresetWave(configData.presetWaveIndex);
                    }
                    else if (configData.waveData != null)
                    {
                        // 使用自定义波数据
                        Wave wave = Wave.FromData(configData.waveData);
                        waveManager.SetEnemyWave(wave);
                    }
                    else
                    {
                        // 如果没有配置波数据，更新显示（显示空波）
                        waveManager.UpdateWaveDisplay();
                    }
                }

                currentEnemies.Add(enemyEntity);
                spawnedEnemies.Add(enemyEntity);

                Debug.Log($"[EnemySpawner] 生成敌人：{enemyEntity.name}，生命值：{healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");
            }

            Debug.Log($"[EnemySpawner] 共生成 {spawnedEnemies.Count} 个敌人");

            // 通知 TargetManager 刷新敌人引用
            RefreshTargetManager();

            return spawnedEnemies;
        }

        /// <summary>
        /// 清除所有敌人（战斗结束时调用）
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (GameObject enemy in currentEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }

            currentEnemies.Clear();
            Debug.Log("[EnemySpawner] 已清除所有敌人");
        }

        /// <summary>
        /// 重置战斗计数（用于重新开始游戏等场景）
        /// </summary>
        /// <param name="nodeType">要重置的战斗类型（如果为null则重置所有）</param>
        public void ResetCombatCounter(string nodeType = null)
        {
            if (string.IsNullOrEmpty(nodeType))
            {
                // 重置所有战斗类型的计数器
                combatCounters.Clear();
                Debug.Log("[EnemySpawner] 所有战斗类型的战斗计数已重置");
            }
            else
            {
                // 重置指定战斗类型的计数器
                if (combatCounters.ContainsKey(nodeType))
                {
                    combatCounters[nodeType] = 0;
                    Debug.Log($"[EnemySpawner] 战斗类型 '{nodeType}' 的战斗计数已重置");
                }
            }
        }

        /// <summary>
        /// 清除指定敌人
        /// </summary>
        /// <param name="enemy">要清除的敌人实体</param>
        public void RemoveEnemy(GameObject enemy)
        {
            if (enemy != null && currentEnemies.Contains(enemy))
            {
                currentEnemies.Remove(enemy);
                Destroy(enemy);
                Debug.Log($"[EnemySpawner] 已清除敌人：{enemy.name}");
            }
        }

        /// <summary>
        /// 通知 TargetManager 刷新敌人引用
        /// </summary>
        private void RefreshTargetManager()
        {
            TargetManager targetManager = FindObjectOfType<TargetManager>();
            if (targetManager != null)
            {
                targetManager.RefreshEnemy();
            }
        }
    }
}

