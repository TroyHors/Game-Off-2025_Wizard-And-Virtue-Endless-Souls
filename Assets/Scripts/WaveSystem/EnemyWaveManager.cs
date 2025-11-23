using System.Collections.Generic;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 敌人波管理器
    /// 管理敌人的波，支持预设占位符
    /// 未来会实现动态计算、随机生成波形（而不是在固定库里随机挑选）
    /// </summary>
    public class EnemyWaveManager : MonoBehaviour
    {
        [Header("敌人波设置")]
        [Tooltip("当前敌人的波")]
        [SerializeField] private Wave currentEnemyWave = new Wave();

        [Header("预设波占位符")]
        [Tooltip("预设波列表（用于占位符，未来可能替换为随机或数据库）")]
        [SerializeField] private List<WaveData> presetWaves = new List<WaveData>();

        [Tooltip("当前使用的预设波索引（-1表示使用自定义波）")]
        [SerializeField] private int currentPresetIndex = -1;

        /// <summary>
        /// 当前敌人的波（只读）
        /// </summary>
        public Wave CurrentEnemyWave => currentEnemyWave;

        /// <summary>
        /// 预设波数量
        /// </summary>
        public int PresetWaveCount => presetWaves.Count;

        private void Awake()
        {
            // 如果当前使用的是预设波，加载预设波
            if (currentPresetIndex >= 0 && currentPresetIndex < presetWaves.Count)
            {
                LoadPresetWave(currentPresetIndex);
            }
        }

        /// <summary>
        /// 设置当前敌人的波
        /// </summary>
        /// <param name="wave">要设置的波</param>
        public void SetEnemyWave(Wave wave)
        {
            if (wave == null)
            {
                Debug.LogWarning("[EnemyWaveManager] 尝试设置空的敌人波");
                currentEnemyWave = new Wave();
                currentPresetIndex = -1;
                return;
            }

            currentEnemyWave = wave.Clone();
            currentPresetIndex = -1; // 使用自定义波，不再使用预设
        }

        /// <summary>
        /// 加载预设波
        /// </summary>
        /// <param name="presetIndex">预设波索引</param>
        public void LoadPresetWave(int presetIndex)
        {
            if (presetIndex < 0 || presetIndex >= presetWaves.Count)
            {
                Debug.LogWarning($"[EnemyWaveManager] 预设波索引 {presetIndex} 超出范围（0-{presetWaves.Count - 1}）");
                return;
            }

            WaveData presetData = presetWaves[presetIndex];
            if (presetData == null)
            {
                Debug.LogWarning($"[EnemyWaveManager] 预设波索引 {presetIndex} 的数据为空");
                return;
            }

            // 从 WaveData 创建 Wave
            currentEnemyWave = Wave.FromData(presetData);
            currentPresetIndex = presetIndex;

            Debug.Log($"[EnemyWaveManager] 加载预设波 {presetIndex}，包含 {currentEnemyWave.PeakCount} 个波峰");
        }

        /// <summary>
        /// 随机加载一个预设波
        /// </summary>
        public void LoadRandomPresetWave()
        {
            if (presetWaves.Count == 0)
            {
                Debug.LogWarning("[EnemyWaveManager] 没有可用的预设波");
                return;
            }

            int randomIndex = Random.Range(0, presetWaves.Count);
            LoadPresetWave(randomIndex);
        }

        /// <summary>
        /// 清空当前敌人的波
        /// </summary>
        public void ClearEnemyWave()
        {
            currentEnemyWave = new Wave();
            currentPresetIndex = -1;
        }

        /// <summary>
        /// 获取当前使用的预设波索引
        /// </summary>
        /// <returns>预设波索引，-1表示使用自定义波</returns>
        public int GetCurrentPresetIndex()
        {
            return currentPresetIndex;
        }

        /// <summary>
        /// 添加预设波
        /// </summary>
        /// <param name="waveData">波数据</param>
        public void AddPresetWave(WaveData waveData)
        {
            if (waveData == null)
            {
                Debug.LogWarning("[EnemyWaveManager] 尝试添加空的预设波数据");
                return;
            }

            presetWaves.Add(waveData);
        }

        /// <summary>
        /// 移除预设波
        /// </summary>
        /// <param name="index">预设波索引</param>
        public void RemovePresetWave(int index)
        {
            if (index < 0 || index >= presetWaves.Count)
            {
                Debug.LogWarning($"[EnemyWaveManager] 预设波索引 {index} 超出范围");
                return;
            }

            presetWaves.RemoveAt(index);

            // 如果移除的是当前使用的预设波，清空当前波
            if (currentPresetIndex == index)
            {
                ClearEnemyWave();
            }
            else if (currentPresetIndex > index)
            {
                // 调整当前索引
                currentPresetIndex--;
            }
        }
    }
}

