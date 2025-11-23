using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DamageSystem;

namespace WaveSystem
{
    /// <summary>
    /// 波伤害序列生成器
    /// 将波转换为有序波峰伤害列表
    /// </summary>
    public static class WaveHitSequenceGenerator
    {
        /// <summary>
        /// 从配对后的波列表生成有序波峰伤害列表
        /// 配对后的波列表可能包含1个或2个波（根据攻击方向分类）
        /// </summary>
        /// <param name="pairedWaves">配对后的波列表</param>
        /// <param name="targetManager">目标管理器，用于查找目标实体</param>
        /// <returns>有序波峰伤害列表（已按 orderIndex 升序排序）</returns>
        public static List<PeakHit> GenerateHitSequenceFromPairedWaves(List<Wave> pairedWaves, TargetManager targetManager)
        {
            if (pairedWaves == null || pairedWaves.Count == 0)
            {
                Debug.Log("[WaveHitSequenceGenerator] 配对后的波列表为空，返回空伤害序列");
                return new List<PeakHit>();
            }

            List<PeakHit> hitSequence = new List<PeakHit>();

            // 遍历所有配对后的波
            foreach (Wave wave in pairedWaves)
            {
                if (wave == null || wave.IsEmpty)
                {
                    continue;
                }

                // 从每个波生成伤害序列
                List<PeakHit> waveHits = GenerateHitSequence(wave, targetManager);
                hitSequence.AddRange(waveHits);
            }

            // 按 orderIndex 升序排序（合并多个波的结果）
            hitSequence = hitSequence.OrderBy(hit => hit.orderIndex).ToList();

            return hitSequence;
        }

        /// <summary>
        /// 从波生成有序波峰伤害列表
        /// </summary>
        /// <param name="wave">要转换的波</param>
        /// <param name="targetManager">目标管理器，用于查找目标实体</param>
        /// <returns>有序波峰伤害列表（已按 orderIndex 升序排序）</returns>
        public static List<PeakHit> GenerateHitSequence(Wave wave, TargetManager targetManager)
        {
            if (wave == null)
            {
                Debug.LogWarning("[WaveHitSequenceGenerator] 波为空，无法生成伤害序列");
                return new List<PeakHit>();
            }

            if (wave.IsEmpty)
            {
                Debug.Log("[WaveHitSequenceGenerator] 波为空波，返回空伤害序列");
                return new List<PeakHit>();
            }

            if (targetManager == null)
            {
                Debug.LogError("[WaveHitSequenceGenerator] TargetManager 未设置，无法生成伤害序列");
                return new List<PeakHit>();
            }

            List<PeakHit> hitSequence = new List<PeakHit>();

            // 遍历所有波峰
            foreach (var kvp in wave.PeakDictionary)
            {
                int position = kvp.Key;
                WavePeak peak = kvp.Value;

                // 跳过强度为0的波峰（不造成伤害）
                if (peak.Value == 0)
                {
                    continue;
                }

                // 根据攻击方向确定目标
                GameObject target = targetManager.GetTargetByDirection(peak.AttackDirection);
                
                if (target == null)
                {
                    Debug.LogWarning($"[WaveHitSequenceGenerator] 位置 {position} 的波峰无法找到目标（攻击方向：{(peak.AttackDirection ? "攻向敌人" : "攻向玩家")}）");
                    continue;
                }

                // 使用强度绝对值确定伤害值
                int damage = Mathf.Abs(peak.Value);

                // 使用波峰位置确定 orderIndex（序号/时间顺序）
                int orderIndex = position;

                // 创建 PeakHit
                PeakHit hit = new PeakHit(target, damage, orderIndex);
                hitSequence.Add(hit);
            }

            // 按 orderIndex 升序排序
            hitSequence = hitSequence.OrderBy(hit => hit.orderIndex).ToList();

            return hitSequence;
        }
    }
}

