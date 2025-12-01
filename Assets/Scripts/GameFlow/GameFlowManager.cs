using MapSystem;
using UnityEngine;
using UnityEngine.Events;
using CurrencySystem;
using System.Collections;
using CardSystem;

namespace GameFlow
{
    /// <summary>
    /// 游戏流程管理器
    /// 负责管理节点事件流程的切换和生命周期
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        [Header("依赖")]
        [Tooltip("地图管理器")]
        [SerializeField] private MapManager mapManager;

        [Tooltip("UI管理器")]
        [SerializeField] private UIManager uiManager;

        [Header("金币系统设置")]
        [Tooltip("是否在游戏开始时自动重置金币")]
        [SerializeField] private bool resetCoinsOnGameStart = true;

        [Tooltip("游戏开始时的初始金币数量")]
        [SerializeField] private int initialCoins = 0;

        [Tooltip("金币系统（如果为空，会自动查找）")]
        [SerializeField] private CoinSystem coinSystem;

        [Header("玩家数据设置")]
        [Tooltip("是否在游戏开始时自动重置玩家数据")]
        [SerializeField] private bool resetPlayerDataOnGameStart = true;

        [Tooltip("玩家实体管理器（如果为空，会自动查找）")]
        [SerializeField] private CharacterSystem.PlayerEntityManager playerEntityManager;

        [Header("小队数据设置")]
        [Tooltip("是否在游戏开始时自动重置小队数据")]
        [SerializeField] private bool resetSquadDataOnGameStart = true;

        [Tooltip("小队管理器（如果为空，会自动查找）")]
        [SerializeField] private SquadSystem.SquadManager squadManager;

        [Header("卡牌系统设置")]
        [Tooltip("是否在游戏开始时自动初始化卡牌系统")]
        [SerializeField] private bool initializeCardSystemOnGameStart = true;

        [Tooltip("卡牌系统（如果为空，会自动查找）")]
        [SerializeField] private CardSystem.CardSystem cardSystem;

        [Header("游戏流程事件")]
        [Tooltip("游戏开始时触发（地图生成后，第一次进入节点前，用于初始化牌堆等）")]
        [SerializeField] private UnityEvent onGameStart = new UnityEvent();

        [Tooltip("节点事件开始时触发（可通过 CurrentNodeData 属性获取当前节点）")]
        [SerializeField] private UnityEvent onNodeEventStart = new UnityEvent();

        [Tooltip("节点事件结束时触发（可通过 CurrentNodeData 属性获取当前节点）")]
        [SerializeField] private UnityEvent onNodeEventEnd = new UnityEvent();

        [Tooltip("游戏结束时触发（到达Boss节点后）")]
        [SerializeField] private UnityEvent onGameEnd = new UnityEvent();

        [Header("运行时状态")]
        [Tooltip("当前正在执行的节点事件流程")]
        [SerializeField] private INodeEventFlow currentFlow;

        [Tooltip("当前流程的GameObject实例")]
        [SerializeField] private GameObject currentFlowInstance;

        [Tooltip("游戏是否已开始")]
        [SerializeField] private bool isGameStarted = false;

        [Tooltip("当前节点数据（用于在Inspector事件中访问）")]
        [SerializeField] private MapNode currentNodeData;

        /// <summary>
        /// 当前是否正在执行节点事件
        /// </summary>
        public bool IsExecutingNodeEvent => currentFlow != null;

        /// <summary>
        /// 游戏是否已开始
        /// </summary>
        public bool IsGameStarted => isGameStarted;

        /// <summary>
        /// 当前节点数据（用于在Inspector事件中访问）
        /// </summary>
        public MapNode CurrentNodeData => currentNodeData;

        /// <summary>
        /// 当前流程对象（用于外部访问）
        /// </summary>
        public INodeEventFlow CurrentFlow => currentFlow;

        /// <summary>
        /// 游戏开始事件
        /// </summary>
        public UnityEvent OnGameStart => onGameStart;

        /// <summary>
        /// 节点事件开始事件
        /// </summary>
        public UnityEvent OnNodeEventStart => onNodeEventStart;

        /// <summary>
        /// 节点事件结束事件
        /// </summary>
        public UnityEvent OnNodeEventEnd => onNodeEventEnd;

        /// <summary>
        /// 游戏结束事件
        /// </summary>
        public UnityEvent OnGameEnd => onGameEnd;

        private void Awake()
        {
            if (mapManager == null)
            {
                mapManager = FindObjectOfType<MapManager>();
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }
        }

        private void Start()
        {
            // 确保在 Start 时再次查找 mapManager（避免 OnEnable 时还未初始化）
            if (mapManager == null)
            {
                mapManager = FindObjectOfType<MapManager>();
            }

            // 订阅游戏开始事件，在事件触发时执行重置操作
            onGameStart.AddListener(OnGameStartHandler);

            // 订阅地图生成事件，在游戏开始时触发
            if (mapManager != null)
            {
                mapManager.OnMapGenerated += HandleMapGenerated;

                // 如果地图已经生成，立即处理（处理订阅时机问题）
                // 注意：现在游戏从Start Scene开始，所以这里不应该自动触发
                // 只有在Start Scene切换到Main Scene后，地图生成才会触发游戏开始
                if (mapManager.CurrentTopology != null && !isGameStarted)
                {
                    Debug.Log("[GameFlowManager] 检测到地图已生成，立即触发游戏开始事件");
                    HandleMapGenerated(mapManager.CurrentTopology);
                }
            }
            else
            {
                Debug.LogWarning("[GameFlowManager] MapManager未找到，游戏可能无法正常开始");
            }
        }

        private void OnDisable()
        {
            // 取消订阅
            if (mapManager != null)
            {
                mapManager.OnMapGenerated -= HandleMapGenerated;
            }
            
            // 取消订阅游戏开始事件
            onGameStart.RemoveListener(OnGameStartHandler);
        }

        /// <summary>
        /// 处理地图生成事件
        /// 地图生成后触发游戏开始事件
        /// </summary>
        private void HandleMapGenerated(MapTopology topology)
        {
            if (!isGameStarted && topology != null)
            {
                isGameStarted = true;
                Debug.Log("[GameFlowManager] 游戏开始");
                
                // 触发游戏开始事件（重置操作会在事件处理中执行）
                onGameStart?.Invoke();
            }
        }

        /// <summary>
        /// 游戏开始事件处理器
        /// 在 onGameStart 事件触发时执行重置和初始化操作
        /// </summary>
        private void OnGameStartHandler()
        {
            // 在游戏开始时重置金币（如果启用）
            if (resetCoinsOnGameStart)
            {
                ResetCoinsOnGameStart();
            }
            
            // 在游戏开始时重置玩家数据（如果启用）
            if (resetPlayerDataOnGameStart)
            {
                ResetPlayerDataOnGameStart();
            }

            // 在游戏开始时重置小队数据（如果启用）
            if (resetSquadDataOnGameStart)
            {
                ResetSquadDataOnGameStart();
            }

            // 在游戏开始时初始化卡牌系统（如果启用）
            if (initializeCardSystemOnGameStart)
            {
                InitializeCardSystemOnGameStart();
            }
        }

        /// <summary>
        /// 在游戏开始时重置金币
        /// </summary>
        private void ResetCoinsOnGameStart()
        {
            // 自动查找 CoinSystem（如果未设置）
            if (coinSystem == null)
            {
                coinSystem = FindObjectOfType<CoinSystem>();
            }

            if (coinSystem != null)
            {
                coinSystem.ResetCoins(initialCoins);
                Debug.Log($"[GameFlowManager] 游戏开始时重置金币为 {initialCoins}");
            }
            else
            {
                Debug.LogWarning("[GameFlowManager] 未找到 CoinSystem，无法重置金币");
            }
        }

        /// <summary>
        /// 在游戏开始时重置玩家数据
        /// </summary>
        private void ResetPlayerDataOnGameStart()
        {
            // 自动查找 PlayerEntityManager（如果未设置）
            if (playerEntityManager == null)
            {
                playerEntityManager = FindObjectOfType<CharacterSystem.PlayerEntityManager>();
            }

            if (playerEntityManager != null && playerEntityManager.PlayerData != null)
            {
                playerEntityManager.PlayerData.ResetHealth();
                Debug.Log($"[GameFlowManager] 游戏开始时重置玩家数据：生命值 {playerEntityManager.PlayerData.CurrentHealth}/{playerEntityManager.PlayerData.MaxHealth}");
            }
            else
            {
                Debug.LogWarning("[GameFlowManager] 未找到 PlayerEntityManager 或 PlayerData，无法重置玩家数据");
            }
        }

        /// <summary>
        /// 在游戏开始时重置小队数据
        /// </summary>
        private void ResetSquadDataOnGameStart()
        {
            // 自动查找 SquadManager（如果未设置）
            if (squadManager == null)
            {
                squadManager = FindObjectOfType<SquadSystem.SquadManager>();
            }

            if (squadManager != null && squadManager.SquadData != null)
            {
                squadManager.SquadData.ClearSquad();
                Debug.Log("[GameFlowManager] 游戏开始时重置小队数据：清空所有成员");
            }
            else
            {
                Debug.LogWarning("[GameFlowManager] 未找到 SquadManager 或 SquadData，无法重置小队数据");
            }
        }

        /// <summary>
        /// 在游戏开始时初始化卡牌系统
        /// </summary>
        private void InitializeCardSystemOnGameStart()
        {
            // 自动查找 CardSystem（如果未设置）
            if (cardSystem == null)
            {
                cardSystem = FindObjectOfType<CardSystem.CardSystem>();
            }

            if (cardSystem != null)
            {
                cardSystem.InitializeGame();
                Debug.Log("[GameFlowManager] 游戏开始时初始化卡牌系统");
            }
            else
            {
                Debug.LogWarning("[GameFlowManager] 未找到 CardSystem，无法初始化卡牌系统");
            }
        }

        /// <summary>
        /// 启动节点事件流程
        /// 这是地图系统调用统一入口
        /// </summary>
        /// <param name="nodeData">节点数据</param>
        public void StartNodeEvent(MapNode nodeData)
        {
            if (nodeData == null)
            {
                Debug.LogError("[GameFlowManager] 无法启动节点事件: 节点数据为空");
                return;
            }

            if (IsExecutingNodeEvent)
            {
                Debug.LogWarning("[GameFlowManager] 无法启动节点事件: 已有事件正在执行");
                return;
            }

            if (mapManager == null)
            {
                Debug.LogError("[GameFlowManager] 无法启动节点事件: 地图管理器未设置");
                return;
            }

            MapGenerationConfig config = mapManager.Config;
            if (config == null)
            {
                Debug.LogError("[GameFlowManager] 无法启动节点事件: 地图配置未设置");
                return;
            }

            // 查找节点类型对应的流程Prefab
            GameObject flowPrefab = GetFlowPrefabForNodeType(config, nodeData.NodeType);
            if (flowPrefab == null)
            {
                Debug.LogError($"[GameFlowManager] 无法启动节点事件: 节点类型 '{nodeData.NodeType}' 没有配置流程Prefab");
                return;
            }

            // 实例化流程Prefab
            currentFlowInstance = Instantiate(flowPrefab);
            currentFlowInstance.name = $"{nodeData.NodeType}Flow_{nodeData.NodeId}";

            // 获取流程组件
            currentFlow = currentFlowInstance.GetComponent<INodeEventFlow>();
            if (currentFlow == null)
            {
                Debug.LogError($"[GameFlowManager] 流程Prefab '{flowPrefab.name}' 没有实现 INodeEventFlow 接口");
                Destroy(currentFlowInstance);
                currentFlowInstance = null;
                return;
            }

            // 保存当前节点数据（用于Inspector事件访问）
            currentNodeData = nodeData;

            // 初始化流程
            currentFlow.Initialize(nodeData);

            // 订阅完成事件
            currentFlow.OnFlowFinished = OnNodeEventFinished;

            // 隐藏地图UI
            if (uiManager != null)
            {
                uiManager.HideMapUI();
            }

            // 触发节点事件开始事件
            Debug.Log($"[GameFlowManager] 启动节点事件流程: Node[{nodeData.NodeId}] Type:{nodeData.NodeType}");
            if (onNodeEventStart != null)
            {
                int listenerCount = onNodeEventStart.GetPersistentEventCount();
                Debug.Log($"[GameFlowManager] OnNodeEventStart 事件监听器数量: {listenerCount}");
                if (listenerCount > 0)
                {
                    onNodeEventStart.Invoke();
                    Debug.Log("[GameFlowManager] OnNodeEventStart 事件已触发");
                }
                else
                {
                    Debug.Log("[GameFlowManager] OnNodeEventStart 事件没有监听器");
                }
            }
            else
            {
                Debug.LogWarning("[GameFlowManager] OnNodeEventStart 事件为 null");
            }

            // 开始执行流程
            currentFlow.StartFlow();
        }

        /// <summary>
        /// 节点事件完成回调
        /// 统一的事件结束出口
        /// </summary>
        /// <param name="nodeData">完成的节点数据</param>
        private void OnNodeEventFinished(MapNode nodeData)
        {
            if (nodeData == null)
            {
                Debug.LogError("[GameFlowManager] 节点事件完成回调: 节点数据为空");
                return;
            }

            Debug.Log($"[GameFlowManager] 节点事件完成: Node[{nodeData.NodeId}] Type:{nodeData.NodeType}");

            // 触发节点事件结束事件（此时 currentNodeData 仍然有效）
            onNodeEventEnd?.Invoke();

            // 释放流程对象
            if (currentFlowInstance != null)
            {
                Destroy(currentFlowInstance);
                currentFlowInstance = null;
            }
            currentFlow = null;
            currentNodeData = null;

            // 显示地图UI
            if (uiManager != null)
            {
                uiManager.ShowMapUI();
                // 显示非战斗UI（隐藏战斗UI）
                uiManager.ShowNonCombatUI();
            }

            // 检查是否到达Boss节点
            if (mapManager != null && mapManager.IsAtBoss())
            {
                // 进入游戏结束状态
                HandleGameEnd();
            }
        }

        /// <summary>
        /// 处理游戏结束
        /// 可以被外部调用（如CombatNodeFlow、PlayerEntityManager）
        /// 当前的游戏结束状态直接通向结束游戏进程
        /// </summary>
        public void HandleGameEnd()
        {
            Debug.Log("[GameFlowManager] 游戏结束，准备退出游戏");
            
            // 触发游戏结束事件（供其他系统响应，如保存数据、显示结束UI等）
            onGameEnd?.Invoke();
            
            // 当前的游戏结束状态直接通向结束游戏进程
            // 在Unity编辑器中，Application.Quit()不会真正退出，所以使用不同的处理方式
            #if UNITY_EDITOR
                Debug.Log("[GameFlowManager] 游戏结束（编辑器模式，不会真正退出）");
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Debug.Log("[GameFlowManager] 游戏结束，退出游戏");
                Application.Quit();
            #endif
        }

        /// <summary>
        /// 手动触发游戏开始（用于测试或特殊场景）
        /// </summary>
        [ContextMenu("触发游戏开始")]
        public void TriggerGameStart()
        {
            if (!isGameStarted)
            {
                isGameStarted = true;
                Debug.Log("[GameFlowManager] 手动触发游戏开始");
                onGameStart?.Invoke();
            }
            else
            {
                Debug.LogWarning("[GameFlowManager] 游戏已经开始，无法重复触发");
            }
        }

        /// <summary>
        /// 结束当前回合（供UI按钮调用）
        /// 如果当前流程是 CombatNodeFlow，则调用其 EnterTurnEnd 方法
        /// </summary>
        public void EndCurrentTurn()
        {
            if (currentFlow == null)
            {
                Debug.LogWarning("[GameFlowManager] 当前没有正在执行的节点事件流程");
                return;
            }

            // 检查是否是战斗流程
            if (currentFlow is CombatNodeFlow combatFlow)
            {
                combatFlow.EnterTurnEnd();
            }
            else
            {
                Debug.LogWarning($"[GameFlowManager] 当前流程类型 {currentFlow.GetType().Name} 不支持结束回合操作");
            }
        }

        /// <summary>
        /// 检查是否可以结束回合（供UI按钮判断是否可用）
        /// </summary>
        public bool CanEndTurn()
        {
            if (currentFlow == null)
            {
                return false;
            }

            // 检查是否是战斗流程且处于回合进行中状态
            if (currentFlow is CombatNodeFlow combatFlow)
            {
                return combatFlow.CurrentState == CombatTurnState.TurnPlaying;
            }

            return false;
        }

        /// <summary>
        /// 根据节点类型获取对应的流程Prefab
        /// </summary>
        private GameObject GetFlowPrefabForNodeType(MapGenerationConfig config, string nodeType)
        {
            if (config == null || config.nodeTypeConfigs == null)
            {
                return null;
            }

            foreach (var typeConfig in config.nodeTypeConfigs)
            {
                if (typeConfig.nodeType == nodeType)
                {
                    return typeConfig.flowPrefab;
                }
            }

            return null;
        }
    }
}

