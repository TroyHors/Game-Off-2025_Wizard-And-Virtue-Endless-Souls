using System.Collections.Generic;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 手牌波管理器
    /// 管理手牌波的合成、撤回和发出
    /// </summary>
    public class HandWaveManager
    {
        /// <summary>
        /// 手牌波
        /// </summary>
        private Wave handWave = new Wave();

        /// <summary>
        /// 获取当前手牌波（只读）
        /// </summary>
        public Wave HandWave => handWave;

        /// <summary>
        /// 获取手牌波中波峰的数量
        /// </summary>
        public int PeakCount => handWave.PeakCount;

        /// <summary>
        /// 检查手牌波是否为空
        /// </summary>
        public bool IsEmpty => handWave.IsEmpty;

        /// <summary>
        /// 设置手牌波（初始为空）
        /// </summary>
        public void ResetHandWave()
        {
            handWave = new Wave();
        }

        /// <summary>
        /// 摆放波牌
        /// 将波牌的波与手牌波配对，生成的新波作为手牌波
        /// 手牌波在合成过程中无任何偏移，只有波牌会根据最尾端波峰位置进行偏移
        /// </summary>
        /// <param name="card">要摆放的波牌</param>
        /// <returns>配对后生成的新波列表</returns>
        public List<Wave> PlaceCard(WaveCard card)
        {
            if (card == null || card.Wave == null)
            {
                Debug.LogWarning("[HandWaveManager] 尝试摆放空的波牌");
                return new List<Wave>();
            }

            if (card.Wave.IsEmpty)
            {
                Debug.LogWarning("[HandWaveManager] 尝试摆放空波的波牌");
                return new List<Wave>();
            }

            // 计算位置偏移：将波牌内部的最尾端波峰位置对齐到TailEndPosition（绝对位置）
            int cardInternalTailEnd = card.Wave.GetMaxPosition();
            int offset = card.TailEndPosition - cardInternalTailEnd;

            // 创建偏移后的波牌波，移除位置小于0的波峰
            Wave offsetCardWave = CreateOffsetWave(card.Wave, offset, removeNegativePositions: true);

            // 配对（手牌波不偏移，直接配对）
            List<Wave> resultWaves = WavePairing.PairWaves(handWave, offsetCardWave);

            // 更新手牌波：合并所有结果波
            handWave = MergeWaves(resultWaves);

            // 移除手牌波中位置小于0的波峰（手牌波最小位置为0）
            RemoveNegativePositionPeaks();

            // 确保手牌波方向为true（朝向敌人）
            if (!handWave.IsEmpty && handWave.AttackDirection != true)
            {
                handWave.SetAttackDirection(true);
            }

            return resultWaves;
        }

        /// <summary>
        /// 撤回波牌
        /// 使用负波与手牌波在相同位置配对
        /// </summary>
        /// <param name="card">要撤回的波牌</param>
        /// <returns>配对后生成的新波列表</returns>
        public List<Wave> WithdrawCard(WaveCard card)
        {
            if (card == null || card.Wave == null)
            {
                Debug.LogWarning("[HandWaveManager] 尝试撤回空的波牌");
                return new List<Wave>();
            }

            if (card.Wave.IsEmpty)
            {
                Debug.LogWarning("[HandWaveManager] 尝试撤回空波的波牌");
                return new List<Wave>();
            }

            // 计算位置偏移（与摆放时相同）
            int cardInternalTailEnd = card.Wave.GetMaxPosition();
            int offset = card.TailEndPosition - cardInternalTailEnd;

            // 创建偏移后的波牌波，移除位置小于0的波峰
            Wave offsetCardWave = CreateOffsetWave(card.Wave, offset, removeNegativePositions: true);

            // 生成负波
            Wave negativeWave = offsetCardWave.GenerateNegativeWave();

            // 配对（手牌波不偏移，直接配对）
            List<Wave> resultWaves = WavePairing.PairWaves(handWave, negativeWave);

            // 更新手牌波
            handWave = MergeWaves(resultWaves);

            // 移除手牌波中位置小于0的波峰（手牌波最小位置为0）
            RemoveNegativePositionPeaks();

            // 确保手牌波方向为true（朝向敌人）
            if (!handWave.IsEmpty && handWave.AttackDirection != true)
            {
                handWave.SetAttackDirection(true);
            }

            return resultWaves;
        }

        /// <summary>
        /// 发出波
        /// 将合成波的首个波峰对齐到0号位，生成新波
        /// </summary>
        /// <returns>发出的波（首个波峰对齐到0号位）</returns>
        public Wave EmitWave()
        {
            if (handWave.IsEmpty)
            {
                Debug.LogWarning("[HandWaveManager] 尝试发出空的手牌波");
                return new Wave();
            }

            int minPosition = handWave.GetMinPosition();
            if (minPosition == 0)
            {
                // 已经对齐到0号位，直接返回副本
                return handWave.Clone();
            }

            // 计算偏移量，使首个波峰对齐到0号位
            int offset = -minPosition;

            // 创建偏移后的波
            Wave emittedWave = CreateOffsetWave(handWave, offset);

            return emittedWave;
        }

        /// <summary>
        /// 创建位置偏移后的波
        /// </summary>
        /// <param name="originalWave">原始波</param>
        /// <param name="offset">偏移量</param>
        /// <param name="removeNegativePositions">是否移除位置小于0的波峰（手牌波最小位置为0）</param>
        /// <returns>偏移后的新波</returns>
        private Wave CreateOffsetWave(Wave originalWave, int offset, bool removeNegativePositions = false)
        {
            if (offset == 0 && !removeNegativePositions)
            {
                return originalWave.Clone();
            }

            Wave offsetWave = new Wave();
            if (originalWave.AttackDirection.HasValue)
            {
                offsetWave.SetAttackDirection(originalWave.AttackDirection.Value);
            }

            foreach (var kvp in originalWave.PeakDictionary)
            {
                int newPosition = kvp.Key + offset;
                
                // 如果启用移除负位置，跳过位置小于0的波峰
                if (removeNegativePositions && newPosition < 0)
                {
                    Debug.Log($"[HandWaveManager] 移除位置小于0的波峰：原始位置={kvp.Key}，偏移后位置={newPosition}（已移除）");
                    continue;
                }

                offsetWave.AddPeak(newPosition, kvp.Value.Clone());
            }

            return offsetWave;
        }

        /// <summary>
        /// 合并多个波为一个波
        /// </summary>
        /// <param name="waves">要合并的波列表</param>
        /// <returns>合并后的波</returns>
        private Wave MergeWaves(List<Wave> waves)
        {
            if (waves == null || waves.Count == 0)
            {
                return new Wave();
            }

            if (waves.Count == 1)
            {
                return waves[0].Clone();
            }

            // 合并所有波到一个波中
            Wave mergedWave = new Wave();
            foreach (var wave in waves)
            {
                if (wave == null || wave.IsEmpty)
                {
                    continue;
                }

                foreach (var kvp in wave.PeakDictionary)
                {
                    mergedWave.AddPeak(kvp.Key, kvp.Value.Clone());
                }
            }

            return mergedWave;
        }

        /// <summary>
        /// 移除手牌波中位置小于0的波峰（手牌波最小位置为0）
        /// </summary>
        private void RemoveNegativePositionPeaks()
        {
            if (handWave.IsEmpty)
            {
                return;
            }

            // 收集所有位置小于0的波峰
            List<int> positionsToRemove = new List<int>();
            foreach (var position in handWave.Positions)
            {
                if (position < 0)
                {
                    positionsToRemove.Add(position);
                }
            }

            // 移除这些波峰
            foreach (var position in positionsToRemove)
            {
                handWave.RemovePeak(position);
                Debug.Log($"[HandWaveManager] 移除了位置小于0的波峰：位置={position}");
            }
        }
    }
}

