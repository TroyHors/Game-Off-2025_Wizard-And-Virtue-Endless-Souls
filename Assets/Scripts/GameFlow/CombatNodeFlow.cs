using MapSystem;
using UnityEngine;
using UnityEngine.Events;

namespace GameFlow
{
    /// <summary>
    /// 战斗回合状态枚举
    /// </summary>
    public enum CombatTurnState
    {
        CombatStart,    // 战斗开始
        TurnStart,      // 回合开始
        TurnPlaying,    // 回合进行中
        TurnEnd         // 回合结束
    }

    /// <summary>
    /// 战斗节点事件流程
    /// 直接集成回合逻辑，替代 GameStateManager
    /// </summary>
    public class CombatNodeFlow : NodeEventFlowBase
    {
        [Header("当前状态")]
        [Tooltip("当前回合状态")]
        [SerializeField] private CombatTurnState currentState = CombatTurnState.CombatStart;

        [Header("回合事件")]
        [Tooltip("战斗开始时的回调")]
        [SerializeField] private UnityEvent onCombatStart = new UnityEvent();

        [Tooltip("回合开始时的回调")]
        [SerializeField] private UnityEvent onTurnStart = new UnityEvent();

        [Tooltip("回合进行中的回调（通常不需要）")]
        [SerializeField] private UnityEvent onTurnPlaying = new UnityEvent();

        [Tooltip("回合结束时的回调")]
        [SerializeField] private UnityEvent onTurnEnd = new UnityEvent();

        [Header("战斗事件")]
        [Tooltip("战斗结束时触发")]
        [SerializeField] private UnityEvent onCombatEnd = new UnityEvent();

        [Header("战斗状态")]
        [Tooltip("战斗是否已结束")]
        [SerializeField] private bool isCombatFinished = false;

        /// <summary>
        /// 当前回合状态
        /// </summary>
        public CombatTurnState CurrentState => currentState;

        /// <summary>
        /// 战斗开始事件
        /// </summary>
        public UnityEvent OnCombatStart => onCombatStart;

        /// <summary>
        /// 回合开始事件
        /// </summary>
        public UnityEvent OnTurnStart => onTurnStart;

        /// <summary>
        /// 回合进行中事件
        /// </summary>
        public UnityEvent OnTurnPlaying => onTurnPlaying;

        /// <summary>
        /// 回合结束事件
        /// </summary>
        public UnityEvent OnTurnEnd => onTurnEnd;

        /// <summary>
        /// 战斗结束事件
        /// </summary>
        public UnityEvent OnCombatEnd => onCombatEnd;

        /// <summary>
        /// 开始执行战斗流程
        /// </summary>
        public override void StartFlow()
        {
            if (currentNodeData == null)
            {
                Debug.LogError("[CombatNodeFlow] 无法开始战斗流程: 节点数据为空");
                return;
            }

            isCombatFinished = false;
            currentState = CombatTurnState.CombatStart;
            Debug.Log($"[CombatNodeFlow] 开始战斗流程: Node[{currentNodeData.NodeId}]");

            // 触发战斗开始事件
            onCombatStart?.Invoke();

            // 开始回合系统
            EnterTurnStart();
        }

        /// <summary>
        /// 进入回合开始状态
        /// </summary>
        public void EnterTurnStart()
        {
            // 可以从战斗开始、回合开始或回合结束状态进入回合开始状态
            if (currentState == CombatTurnState.CombatStart || 
                currentState == CombatTurnState.TurnStart || 
                currentState == CombatTurnState.TurnEnd)
            {
                currentState = CombatTurnState.TurnStart;
                Debug.Log("[CombatNodeFlow] 进入回合开始状态");
                onTurnStart?.Invoke();
                
                // 自动进入回合进行中状态
                EnterTurnPlaying();
            }
            else
            {
                Debug.LogWarning($"[CombatNodeFlow] 无法从 {currentState} 状态进入回合开始状态");
            }
        }

        /// <summary>
        /// 进入回合进行中状态
        /// </summary>
        public void EnterTurnPlaying()
        {
            if (currentState == CombatTurnState.TurnStart)
            {
                currentState = CombatTurnState.TurnPlaying;
                Debug.Log("[CombatNodeFlow] 进入回合进行中状态");
                onTurnPlaying?.Invoke();
            }
        }

        /// <summary>
        /// 进入回合结束状态
        /// </summary>
        public void EnterTurnEnd()
        {
            if (currentState == CombatTurnState.TurnPlaying)
            {
                currentState = CombatTurnState.TurnEnd;
                Debug.Log("[CombatNodeFlow] 进入回合结束状态");
                onTurnEnd?.Invoke();
                
                // 检查战斗是否应该结束
                // 这里可以添加战斗结束条件检查
                // 例如：检查敌人是否全部死亡、玩家是否死亡等
                // 如果满足结束条件，调用 FinishCombat()
                // 否则自动进入下一回合开始状态（回合循环）
                
                // 注意：实际的战斗结束逻辑应该由具体的战斗系统实现
                // 这里只提供框架，具体的结束条件判断由用户后续填充
                
                // 默认继续下一回合
                EnterTurnStart();
            }
            else
            {
                Debug.LogWarning($"[CombatNodeFlow] 无法从 {currentState} 状态进入回合结束状态");
            }
        }

        /// <summary>
        /// 完成战斗
        /// 由战斗系统在战斗结束时调用
        /// </summary>
        public void FinishCombat()
        {
            if (isCombatFinished)
            {
                return;
            }

            isCombatFinished = true;
            Debug.Log($"[CombatNodeFlow] 战斗完成: Node[{currentNodeData.NodeId}]");

            // 触发战斗结束事件
            onCombatEnd?.Invoke();

            // 完成流程
            FinishFlow();
        }

        /// <summary>
        /// 手动设置状态（用于调试）
        /// </summary>
        /// <param name="newState">新状态</param>
        [ContextMenu("设置状态")]
        public void SetState(CombatTurnState newState)
        {
            currentState = newState;
            Debug.Log($"[CombatNodeFlow] 手动设置状态为: {newState}");
        }
    }
}

