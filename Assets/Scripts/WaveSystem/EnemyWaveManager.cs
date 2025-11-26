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

        [Header("波显示设置")]
        [Tooltip("敌人波显示容器（用于显示敌人波波形，必须设置）")]
        [SerializeField] private RectTransform waveContainer;

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
            // 注意：不在启动时初始化波显示，等待战斗开始时再初始化
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
                UpdateWaveDisplay();
                return;
            }

            currentEnemyWave = wave.Clone();
            currentPresetIndex = -1; // 使用自定义波，不再使用预设
            UpdateWaveDisplay();
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
            UpdateWaveDisplay();
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
            UpdateWaveDisplay();
        }

        /// <summary>
        /// 波显示器（自动创建，无需手动挂载）
        /// </summary>
        private WaveVisualizer waveVisualizer;

        /// <summary>
        /// 初始化波显示器（使用手牌波的参数）
        /// </summary>
        /// <param name="handWaveGridManager">手牌波格表管理器（用于获取位置范围和容器参数）</param>
        public void InitializeWaveVisualizer(HandWaveGridManager handWaveGridManager)
        {
            if (handWaveGridManager == null)
            {
                Debug.LogWarning("[EnemyWaveManager] HandWaveGridManager未设置，无法初始化波显示器");
                return;
            }

            if (waveContainer == null)
            {
                Debug.LogWarning("[EnemyWaveManager] 波显示容器未设置，无法初始化波显示器");
                return;
            }

            // 获取手牌波的WaveVisualizer（用于获取计算后的参数）
            WaveVisualizer handWaveVisualizer = handWaveGridManager.WaveVisualizer;
            if (handWaveVisualizer == null)
            {
                Debug.LogWarning("[EnemyWaveManager] 无法找到手牌波的WaveVisualizer，无法获取计算后的参数");
                return;
            }
            
            // 确保手牌波已经计算了单位高度（通过触发一次显示来确保计算完成）
            // 手牌波会在战斗开始时更新显示，此时会计算单位高度
            handWaveGridManager.UpdateWaveDisplay();

            // 自动获取或创建 WaveVisualizer 组件（挂载在 waveContainer 上）
            waveVisualizer = waveContainer.GetComponent<WaveVisualizer>();
            if (waveVisualizer == null)
            {
                waveVisualizer = waveContainer.gameObject.AddComponent<WaveVisualizer>();
                Debug.Log("[EnemyWaveManager] 自动创建 WaveVisualizer 组件");
            }

            // 设置波显示器的容器
            waveVisualizer.WaveContainer = waveContainer;
            
            // 设置波的位置范围（使用与手牌波相同的范围）
            int minPosition = handWaveGridManager.MinGridPosition;
            int maxPosition = handWaveGridManager.MaxGridPosition;
            waveVisualizer.SetPositionRange(minPosition, maxPosition);
            
            // 设置敌人波显示方向为正常（不翻转）
            waveVisualizer.ReverseDirection = false;
            Debug.Log("[EnemyWaveManager] 设置敌人波显示方向为正常（不翻转）");
            
            // 获取手牌波计算后的波峰单位高度
            float calculatedPeakUnitHeight = handWaveVisualizer.CalculatedPeakUnitHeight;
            if (calculatedPeakUnitHeight > 0)
            {
                // 直接设置计算后的单位高度（这样敌人波会使用相同的单位高度）
                waveVisualizer.PeakUnitHeight = calculatedPeakUnitHeight;
                Debug.Log($"[EnemyWaveManager] 使用手牌波计算后的单位高度: {calculatedPeakUnitHeight}");
            }
            else
            {
                Debug.LogWarning("[EnemyWaveManager] 手牌波的单位高度未计算，敌人波将使用默认值");
            }
            
            Debug.Log($"[EnemyWaveManager] 初始化波显示器，位置范围: {minPosition} 到 {maxPosition}");
        }

        /// <summary>
        /// 更新敌人波显示
        /// </summary>
        public void UpdateWaveDisplay()
        {
            if (waveVisualizer != null)
            {
                waveVisualizer.DisplayWave(currentEnemyWave);
            }
            else
            {
                Debug.LogWarning("[EnemyWaveManager] WaveVisualizer未初始化，无法更新显示。请先调用 InitializeWaveVisualizer(HandWaveGridManager)");
            }
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

