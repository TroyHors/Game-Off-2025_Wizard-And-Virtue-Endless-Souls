using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 波牌 - 包含波数据的实例
    /// </summary>
    [System.Serializable]
    public class WaveCard
    {
        /// <summary>
        /// 波牌的波数据
        /// </summary>
        public Wave Wave { get; private set; }

        /// <summary>
        /// 最尾端波峰位置
        /// 未来会根据摆放位置决定，现在可以在测试中设置
        /// </summary>
        public int TailEndPosition { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="wave">波数据</param>
        /// <param name="tailEndPosition">最尾端波峰位置</param>
        public WaveCard(Wave wave, int tailEndPosition)
        {
            Wave = wave ?? new Wave();
            TailEndPosition = tailEndPosition;
        }

        /// <summary>
        /// 从波数据创建波牌
        /// </summary>
        /// <param name="waveData">波数据</param>
        /// <param name="tailEndPosition">最尾端波峰位置</param>
        /// <returns>新创建的波牌</returns>
        public static WaveCard FromWaveData(WaveData waveData, int tailEndPosition)
        {
            Wave wave = Wave.FromData(waveData);
            return new WaveCard(wave, tailEndPosition);
        }
    }
}

