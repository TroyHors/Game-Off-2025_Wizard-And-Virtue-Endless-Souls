using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 波牌组件
    /// 可以挂载到GameObject/Prefab上，承载波牌数据
    /// </summary>
    public class WaveCardComponent : MonoBehaviour
    {
        [Header("波牌数据")]
        [Tooltip("波数据（使用WaveData格式）")]
        [SerializeField] private WaveData waveData = new WaveData(true);

        [Header("显示设置")]
        [Tooltip("是否在Inspector中显示波数据详情")]
        [SerializeField] private bool showWaveDetails = true;

        /// <summary>
        /// 波牌的波数据（只读）
        /// </summary>
        public Wave Wave { get; private set; }

        private void Awake()
        {
            RefreshWave();
        }

        /// <summary>
        /// 刷新Wave对象（从WaveData创建）
        /// </summary>
        private void RefreshWave()
        {
            if (waveData != null)
            {
                // 确保缓存已刷新（从序列化数据更新）
                // 通过访问IsEmpty来触发RefreshCache
                bool isEmpty = waveData.IsEmpty;
                
                if (!isEmpty)
                {
                    Wave = Wave.FromData(waveData);
                }
                else
                {
                    Wave = new Wave();
                    // 只在运行时且确实为空时警告（避免编辑器中的误报）
                    if (Application.isPlaying && waveData.GetSerializedPeakData().Count == 0)
                    {
                        Debug.LogWarning($"[WaveCardComponent] {gameObject.name} 的波数据为空");
                    }
                }
            }
            else
            {
                Wave = new Wave();
                if (Application.isPlaying)
                {
                    Debug.LogWarning($"[WaveCardComponent] {gameObject.name} 的波数据为null");
                }
            }
        }

        /// <summary>
        /// 设置波数据
        /// </summary>
        /// <param name="data">波数据</param>
        public void SetWaveData(WaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[WaveCardComponent] 尝试设置空的波数据");
                return;
            }

            waveData = data;
            Wave = Wave.FromData(data);
        }

        /// <summary>
        /// 获取波数据
        /// </summary>
        /// <returns>波数据</returns>
        public WaveData GetWaveData()
        {
            return waveData;
        }

        /// <summary>
        /// 在Inspector中显示波数据详情
        /// </summary>
        private void OnValidate()
        {
            if (waveData != null)
            {
                // 确保方向为false（朝向敌人）
                if (waveData.AttackDirection != false)
                {
                    waveData.AttackDirection = false;
                    if (Application.isPlaying)
                    {
                        Debug.LogWarning($"[WaveCardComponent] {gameObject.name} 的波数据方向已自动修正为false（朝向敌人）");
                    }
                }

                // 刷新Wave对象
                RefreshWave();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 在Inspector中显示波数据详情（仅编辑器）
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (showWaveDetails && waveData != null && !waveData.IsEmpty)
            {
                // 这里可以添加可视化代码，例如在Scene视图中显示波数据
            }
        }
#endif
    }
}

