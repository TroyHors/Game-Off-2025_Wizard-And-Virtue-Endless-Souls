using System.Collections.Generic;
using UnityEngine;
using WaveSystem;

namespace CharacterSystem
{
    /// <summary>
    /// 单个敌人配置数据
    /// </summary>
    [System.Serializable]
    public class EnemyConfigData
    {
        [Header("敌人基本信息")]
        [Tooltip("敌人名称")]
        public string enemyName = "Enemy";

        [Tooltip("最大生命值")]
        public int maxHealth = 50;

        [Header("敌人波数据")]
        [Tooltip("敌人波数据（用于 EnemyWaveManager）")]
        public WaveData waveData;

        [Tooltip("预设波索引（-1表示使用自定义waveData）")]
        public int presetWaveIndex = -1;
    }

    /// <summary>
    /// 敌人配置（ScriptableObject）
    /// 存储一组敌人的配置数据，用于战斗时生成敌人
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Character System/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("敌人配置列表")]
        [Tooltip("敌人配置数据列表（每次战斗会使用一组配置生成敌人）")]
        [SerializeField] private List<EnemyConfigData> enemyConfigs = new List<EnemyConfigData>();

        [Header("敌人实体设置")]
        [Tooltip("敌人实体Prefab（必须包含 HealthComponent 组件）")]
        [SerializeField] private GameObject enemyEntityPrefab;

        [Tooltip("敌人Tag（用于查找敌人）")]
        [SerializeField] private string enemyTag = "Enemy";

        /// <summary>
        /// 敌人配置数据列表
        /// </summary>
        public List<EnemyConfigData> EnemyConfigs => enemyConfigs;

        /// <summary>
        /// 敌人实体Prefab
        /// </summary>
        public GameObject EnemyEntityPrefab => enemyEntityPrefab;

        /// <summary>
        /// 敌人Tag
        /// </summary>
        public string EnemyTag => enemyTag;

        /// <summary>
        /// 获取指定索引的敌人配置
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>敌人配置数据，如果索引无效返回 null</returns>
        public EnemyConfigData GetEnemyConfig(int index)
        {
            if (index >= 0 && index < enemyConfigs.Count)
            {
                return enemyConfigs[index];
            }
            return null;
        }

        /// <summary>
        /// 获取敌人配置数量
        /// </summary>
        public int ConfigCount => enemyConfigs.Count;
    }
}

