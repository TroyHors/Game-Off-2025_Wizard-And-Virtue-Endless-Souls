using System.Collections.Generic;
using UnityEngine;
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
        [Tooltip("敌人配置（包含敌人数据和Prefab）")]
        [SerializeField] private EnemyConfig enemyConfig;

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

        [Tooltip("战斗计数器（用于依次生成敌人，第一次战斗生成第一个，第二次战斗生成第二个）")]
        [SerializeField] private int combatCounter = 0;

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
        /// 当前战斗计数（用于依次生成敌人）
        /// </summary>
        public int CombatCounter => combatCounter;

        /// <summary>
        /// 根据配置生成敌人（战斗开始时调用）
        /// 敌人依次生成：第一次战斗生成第一个配置的敌人，第二次战斗生成第二个配置的敌人，以此类推
        /// 所有敌人都生成在同一个位置（使用第一个生成位置或默认位置）
        /// </summary>
        /// <param name="configIndices">要使用的配置索引列表（如果为null，则按战斗计数依次生成单个敌人）</param>
        /// <returns>生成的敌人实体列表</returns>
        public List<GameObject> SpawnEnemies(List<int> configIndices = null)
        {
            // 先清除现有敌人
            ClearAllEnemies();

            if (enemyConfig == null)
            {
                Debug.LogError("[EnemySpawner] 敌人配置未设置，无法生成敌人");
                return new List<GameObject>();
            }

            if (enemyConfig.EnemyEntityPrefab == null)
            {
                Debug.LogError("[EnemySpawner] 敌人实体Prefab未设置，无法生成敌人");
                return new List<GameObject>();
            }

            List<GameObject> spawnedEnemies = new List<GameObject>();

            // 确定要使用的配置索引
            List<int> indicesToUse = configIndices;
            if (indicesToUse == null || indicesToUse.Count == 0)
            {
                // 如果没有指定，按战斗计数依次生成单个敌人
                // 第一次战斗生成第一个配置（索引0），第二次战斗生成第二个配置（索引1），以此类推
                if (combatCounter >= enemyConfig.ConfigCount)
                {
                    Debug.LogWarning($"[EnemySpawner] 战斗计数 {combatCounter} 超出配置数量 {enemyConfig.ConfigCount}，循环使用配置");
                    // 循环使用配置
                    indicesToUse = new List<int> { combatCounter % enemyConfig.ConfigCount };
                }
                else
                {
                    indicesToUse = new List<int> { combatCounter };
                }
            }

            // 增加战斗计数（用于下次战斗）
            combatCounter++;

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

                // 实例化敌人实体
                GameObject enemyEntity = Instantiate(enemyConfig.EnemyEntityPrefab, spawnPosition, spawnRotation);
                enemyEntity.name = $"{configData.enemyName}_{i}";
                enemyEntity.tag = enemyConfig.EnemyTag;

                // 获取 HealthComponent 并应用配置
                HealthComponent healthComponent = enemyEntity.GetComponent<HealthComponent>();
                if (healthComponent == null)
                {
                    Debug.LogError($"[EnemySpawner] 敌人实体Prefab缺少 HealthComponent 组件，已销毁：{enemyEntity.name}");
                    Destroy(enemyEntity);
                    continue;
                }

                // 设置生命值
                healthComponent.SetMaxHealth(configData.maxHealth);
                healthComponent.ResetHealth();

                // 设置敌人波数据（如果存在 EnemyWaveManager）
                EnemyWaveManager waveManager = enemyEntity.GetComponent<EnemyWaveManager>();
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
        public void ResetCombatCounter()
        {
            combatCounter = 0;
            Debug.Log("[EnemySpawner] 战斗计数已重置");
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

