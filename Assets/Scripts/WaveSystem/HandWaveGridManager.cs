using System.Collections.Generic;
using UnityEngine;

namespace WaveSystem
{
    /// <summary>
    /// 手牌波格表管理器
    /// 管理手牌波和格表系统，处理波牌的放置和配对
    /// </summary>
    public class HandWaveGridManager : MonoBehaviour
    {
        [Header("手牌波设置")]
        [Tooltip("手牌波的最小位置（格表起始位置）")]
        [SerializeField] private int minGridPosition = -10;
        
        [Tooltip("手牌波的最大位置（格表结束位置）")]
        [SerializeField] private int maxGridPosition = 10;
        
        [Header("格表设置")]
        [Tooltip("格表容器（所有格子应该作为此GameObject的子对象）")]
        [SerializeField] private Transform gridContainer;
        
        [Header("调试")]
        [Tooltip("是否在控制台打印手牌波详情")]
        [SerializeField] private bool debugPrintWaveDetails = true;

        /// <summary>
        /// 手牌波管理器
        /// </summary>
        private HandWaveManager handWaveManager = new HandWaveManager();

        /// <summary>
        /// 格子字典：位置 -> 格子组件
        /// </summary>
        private Dictionary<int, WaveGridSlot> gridSlots = new Dictionary<int, WaveGridSlot>();

        /// <summary>
        /// 当前手牌波（只读）
        /// </summary>
        public Wave HandWave => handWaveManager.HandWave;

        /// <summary>
        /// 手牌波管理器（只读）
        /// </summary>
        public HandWaveManager HandWaveManager => handWaveManager;

        /// <summary>
        /// 获取格表的最小位置
        /// </summary>
        public int MinGridPosition => minGridPosition;

        /// <summary>
        /// 获取格表的最大位置
        /// </summary>
        public int MaxGridPosition => maxGridPosition;

        private void Awake()
        {
            InitializeGrid();
        }

        /// <summary>
        /// 初始化格表
        /// </summary>
        private void InitializeGrid()
        {
            gridSlots.Clear();

            if (gridContainer == null)
            {
                Debug.LogWarning("[HandWaveGridManager] 格表容器未设置，无法初始化格子");
                return;
            }

            // 从子对象中查找所有格子组件
            WaveGridSlot[] slots = gridContainer.GetComponentsInChildren<WaveGridSlot>();
            foreach (var slot in slots)
            {
                if (slot.GridPosition >= minGridPosition && slot.GridPosition <= maxGridPosition)
                {
                    gridSlots[slot.GridPosition] = slot;
                    slot.Initialize(this);
                }
                else
                {
                    Debug.LogWarning($"[HandWaveGridManager] 格子位置 {slot.GridPosition} 超出范围 [{minGridPosition}, {maxGridPosition}]，已忽略");
                }
            }

            Debug.Log($"[HandWaveGridManager] 格表初始化完成，共 {gridSlots.Count} 个格子");
        }

        /// <summary>
        /// 在指定位置放置波牌
        /// </summary>
        /// <param name="cardComponent">波牌组件</param>
        /// <param name="gridPosition">格子位置</param>
        /// <returns>是否成功放置</returns>
        public bool PlaceCardAtPosition(WaveCardComponent cardComponent, int gridPosition)
        {
            if (cardComponent == null)
            {
                Debug.LogWarning("[HandWaveGridManager] 尝试放置空的波牌组件");
                return false;
            }

            if (!gridSlots.ContainsKey(gridPosition))
            {
                Debug.LogWarning($"[HandWaveGridManager] 位置 {gridPosition} 不存在格子");
                return false;
            }

            WaveGridSlot slot = gridSlots[gridPosition];
            if (slot.IsOccupied)
            {
                Debug.LogWarning($"[HandWaveGridManager] 位置 {gridPosition} 的格子已被占用");
                return false;
            }

            // 创建波牌数据，使用格子位置作为最尾端位置
            WaveCard card = new WaveCard(cardComponent.Wave, gridPosition);

            // 放置波牌到格子
            slot.PlaceCard(cardComponent);

            // 与手牌波配对
            List<Wave> resultWaves = handWaveManager.PlaceCard(card);

            if (debugPrintWaveDetails)
            {
                PrintHandWaveDetails();
            }

            return true;
        }

        /// <summary>
        /// 从指定位置撤回波牌
        /// </summary>
        /// <param name="gridPosition">格子位置</param>
        /// <returns>是否成功撤回</returns>
        public bool WithdrawCardFromPosition(int gridPosition)
        {
            if (!gridSlots.ContainsKey(gridPosition))
            {
                Debug.LogWarning($"[HandWaveGridManager] 位置 {gridPosition} 不存在格子");
                return false;
            }

            WaveGridSlot slot = gridSlots[gridPosition];
            if (!slot.IsOccupied)
            {
                Debug.LogWarning($"[HandWaveGridManager] 位置 {gridPosition} 的格子未被占用");
                return false;
            }

            WaveCardComponent cardComponent = slot.GetCard();
            if (cardComponent == null)
            {
                Debug.LogWarning($"[HandWaveGridManager] 位置 {gridPosition} 的格子中的波牌组件为空");
                return false;
            }

            // 创建波牌数据，使用格子位置作为最尾端位置
            WaveCard card = new WaveCard(cardComponent.Wave, gridPosition);

            // 从格子移除波牌
            slot.RemoveCard();

            // 与手牌波配对（使用负波）
            List<Wave> resultWaves = handWaveManager.WithdrawCard(card);

            if (debugPrintWaveDetails)
            {
                PrintHandWaveDetails();
            }

            return true;
        }

        /// <summary>
        /// 发出手牌波
        /// </summary>
        /// <returns>发出的波（首个波峰对齐到0号位）</returns>
        public Wave EmitHandWave()
        {
            Wave emittedWave = handWaveManager.EmitWave();
            
            if (debugPrintWaveDetails)
            {
                Debug.Log("[HandWaveGridManager] 发出手牌波");
                PrintWaveDetails(emittedWave, "发出的波");
            }

            return emittedWave;
        }

        /// <summary>
        /// 重置手牌波
        /// </summary>
        public void ResetHandWave()
        {
            handWaveManager.ResetHandWave();
            
            // 清除所有格子中的波牌
            foreach (var slot in gridSlots.Values)
            {
                if (slot.IsOccupied)
                {
                    slot.RemoveCard();
                }
            }

            if (debugPrintWaveDetails)
            {
                Debug.Log("[HandWaveGridManager] 手牌波已重置");
            }
        }

        /// <summary>
        /// 获取指定位置的格子
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>格子组件，如果不存在则返回null</returns>
        public WaveGridSlot GetSlot(int position)
        {
            gridSlots.TryGetValue(position, out WaveGridSlot slot);
            return slot;
        }

        /// <summary>
        /// 打印手牌波详情
        /// </summary>
        private void PrintHandWaveDetails()
        {
            PrintWaveDetails(handWaveManager.HandWave, "当前手牌波");
        }

        /// <summary>
        /// 打印波的详细信息
        /// </summary>
        private void PrintWaveDetails(Wave wave, string label)
        {
            Debug.Log($"--- {label} ---");
            Debug.Log($"  PeakCount: {wave.PeakCount}, IsEmpty: {wave.IsEmpty}");
            Debug.Log($"  波的方向: {(wave.AttackDirection.HasValue ? (wave.AttackDirection.Value ? "→敌人" : "→其他") : "未定义（空波）")}");

            if (wave.IsEmpty)
            {
                Debug.Log("  (空波)");
            }
            else
            {
                var sortedPeaks = wave.GetSortedPeaks();
                foreach (var (position, peak) in sortedPeaks)
                {
                    string direction = peak.AttackDirection ? "→敌人" : "→其他";
                    Debug.Log($"  位置{position}: 强度={peak.Value}, 方向={direction}");
                }
            }
        }

        /// <summary>
        /// 在Inspector中手动刷新格表（用于运行时调试）
        /// </summary>
        [ContextMenu("刷新格表")]
        public void RefreshGrid()
        {
            InitializeGrid();
        }
    }
}

