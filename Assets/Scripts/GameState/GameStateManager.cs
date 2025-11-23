using UnityEngine;
using UnityEngine.Events;

namespace GameState
{
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameStateType
    {
        GameStart,      // 游戏开始
        TurnStart,      // 回合开始
        TurnPlaying,    // 回合进行中
        TurnEnd         // 回合结束
    }

    /// <summary>
    /// 游戏状态管理器
    /// 管理游戏流程和状态转换
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        [Header("当前状态")]
        [Tooltip("当前游戏状态")]
        [SerializeField] private GameStateType currentState = GameStateType.GameStart;

        [Header("状态事件")]
        [Tooltip("游戏开始时的回调")]
        [SerializeField] private UnityEvent onGameStart = new UnityEvent();

        [Tooltip("回合开始时的回调")]
        [SerializeField] private UnityEvent onTurnStart = new UnityEvent();

        [Tooltip("回合进行中的回调（通常不需要）")]
        [SerializeField] private UnityEvent onTurnPlaying = new UnityEvent();

        [Tooltip("回合结束时的回调")]
        [SerializeField] private UnityEvent onTurnEnd = new UnityEvent();

        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public GameStateType CurrentState => currentState;

        /// <summary>
        /// 游戏开始事件
        /// </summary>
        public UnityEvent OnGameStart => onGameStart;

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

        private void Start()
        {
            // 游戏开始时自动触发游戏开始状态
            if (currentState == GameStateType.GameStart)
            {
                EnterGameStart();
            }
        }

        /// <summary>
        /// 进入游戏开始状态
        /// </summary>
        public void EnterGameStart()
        {
            if (currentState == GameStateType.GameStart)
            {
                Debug.Log("[GameStateManager] 进入游戏开始状态");
                onGameStart?.Invoke();
                
                // 游戏开始后自动进入第一回合开始状态
                EnterTurnStart();
            }
        }

        /// <summary>
        /// 进入回合开始状态
        /// </summary>
        public void EnterTurnStart()
        {
            // 可以从游戏开始、回合开始或回合结束状态进入回合开始状态
            if (currentState == GameStateType.GameStart || 
                currentState == GameStateType.TurnStart || 
                currentState == GameStateType.TurnEnd)
            {
                currentState = GameStateType.TurnStart;
                Debug.Log("[GameStateManager] 进入回合开始状态");
                onTurnStart?.Invoke();
                
                // 自动进入回合进行中状态
                EnterTurnPlaying();
            }
            else
            {
                Debug.LogWarning($"[GameStateManager] 无法从 {currentState} 状态进入回合开始状态");
            }
        }

        /// <summary>
        /// 进入回合进行中状态
        /// </summary>
        public void EnterTurnPlaying()
        {
            if (currentState == GameStateType.TurnStart)
            {
                currentState = GameStateType.TurnPlaying;
                Debug.Log("[GameStateManager] 进入回合进行中状态");
                onTurnPlaying?.Invoke();
            }
        }

        /// <summary>
        /// 进入回合结束状态
        /// </summary>
        public void EnterTurnEnd()
        {
            if (currentState == GameStateType.TurnPlaying)
            {
                currentState = GameStateType.TurnEnd;
                Debug.Log("[GameStateManager] 进入回合结束状态");
                onTurnEnd?.Invoke();
                
                // 自动进入下一回合开始状态（回合循环）
                EnterTurnStart();
            }
            else
            {
                Debug.LogWarning($"[GameStateManager] 无法从 {currentState} 状态进入回合结束状态");
            }
        }

        /// <summary>
        /// 手动设置状态（用于调试）
        /// </summary>
        /// <param name="newState">新状态</param>
        [ContextMenu("设置状态")]
        public void SetState(GameStateType newState)
        {
            currentState = newState;
            Debug.Log($"[GameStateManager] 手动设置状态为: {newState}");
        }
    }
}

