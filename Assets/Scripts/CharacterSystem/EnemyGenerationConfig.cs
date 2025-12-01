using System.Collections.Generic;
using UnityEngine;
using WaveSystem;

namespace CharacterSystem
{
    /// <summary>
    /// 波生成方法枚举
    /// </summary>
    public enum WaveGenerationMethod
    {
        Random,         // 随机生成
        Uniform,        // 均匀分布
        Concentrated    // 集中分布
    }

    /// <summary>
    /// 单个战斗类型的敌人生成配置
    /// </summary>
    [System.Serializable]
    public class EnemyTypeGenerationConfig
    {
        [Header("战斗类型")]
        [Tooltip("战斗类型（Combat、Elite、Boss）")]
        public string nodeType = "Combat";

        [Header("敌人外观设置")]
        [Tooltip("敌人Figure列表（敌人外观图片素材列表，Sprite）")]
        public List<Sprite> enemyFigures = new List<Sprite>();

        [Header("生命值设置")]
        [Tooltip("生命值范围（最小值，最大值）")]
        public Vector2Int healthRange = new Vector2Int(50, 100);

        [Header("波生成设置")]
        [Tooltip("波生成方法（代码内置）")]
        public WaveGenerationMethod waveGenerationMethod = WaveGenerationMethod.Random;

        [Tooltip("波峰总值范围（最小值，最大值）")]
        public Vector2Int totalPeakValueRange = new Vector2Int(10, 20);

        [Tooltip("单个波峰最大值")]
        public int maxSinglePeakValue = 5;

        [Tooltip("波峰位置范围（最小值，最大值，用于随机生成波峰位置）")]
        public Vector2Int peakPositionRange = new Vector2Int(0, 10);
    }

    /// <summary>
    /// 敌人生成配置（ScriptableObject）
    /// 为每种战斗类型配置敌人生成参数
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy Generation Config", menuName = "Character System/Enemy Generation Config")]
    public class EnemyGenerationConfig : ScriptableObject
    {
        [Header("战斗类型配置")]
        [Tooltip("各战斗类型的敌人生成配置")]
        [SerializeField] private List<EnemyTypeGenerationConfig> typeConfigs = new List<EnemyTypeGenerationConfig>();

        /// <summary>
        /// 获取指定战斗类型的生成配置
        /// </summary>
        /// <param name="nodeType">战斗类型</param>
        /// <returns>生成配置，如果不存在则返回null</returns>
        public EnemyTypeGenerationConfig GetTypeConfig(string nodeType)
        {
            foreach (var config in typeConfigs)
            {
                if (config != null && config.nodeType == nodeType)
                {
                    return config;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取所有类型配置
        /// </summary>
        public List<EnemyTypeGenerationConfig> TypeConfigs => typeConfigs;
    }
}

