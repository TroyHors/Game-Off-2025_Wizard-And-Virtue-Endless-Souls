using MapSystem;
using UnityEngine;
using WaveSystem;
using DamageSystem;
using System.Collections.Generic;
using System.Linq;
using StatusSystem;
using CardSystemManager = CardSystem.CardSystem;

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
    /// 所有调用都通过代码直接执行，不依赖 Inspector 配置
    /// </summary>
    public class CombatNodeFlow : NodeEventFlowBase
    {
        [Header("当前状态")]
        [Tooltip("当前回合状态")]
        [SerializeField] private CombatTurnState currentState = CombatTurnState.CombatStart;

        [Header("系统引用（运行时自动查找）")]
        [Tooltip("卡牌系统（回合开始时抽牌）")]
        [SerializeField] private CardSystemManager cardSystem;

        [Tooltip("手波网格管理器（回合结束时发射和弃牌）")]
        [SerializeField] private HandWaveGridManager handWaveGridManager;

        [Tooltip("伤害系统辅助类（完整流程：发出玩家波 -> 获取敌人波 -> 配对 -> 生成伤害序列 -> 处理伤害序列）")]
        [SerializeField] private DamageSystem.DamageSystemHelper damageSystemHelper;

        [Tooltip("伤害系统（用于订阅死亡事件）")]
        [SerializeField] private DamageSystem.DamageSystem damageSystem;

        [Header("敌人设置")]
        [Tooltip("敌人Tag（用于查找敌人）")]
        [SerializeField] private string enemyTag = "Enemy";

        [Header("角色生成系统")]
        [Tooltip("玩家实体管理器（用于生成和销毁玩家实体）")]
        [SerializeField] private CharacterSystem.PlayerEntityManager playerEntityManager;

        [Tooltip("敌人生成器（用于生成和销毁敌人实体）")]
        [SerializeField] private CharacterSystem.EnemySpawner enemySpawner;

        [Tooltip("小队管理器（用于生成和管理小队成员）")]
        [SerializeField] private SquadSystem.SquadManager squadManager;

        [Header("奖励系统")]
        [Tooltip("奖励管理器（用于发放战斗奖励）")]
        [SerializeField] private RewardManager rewardManager;

        [Header("战斗状态")]
        [Tooltip("战斗是否已结束")]
        [SerializeField] private bool isCombatFinished = false;

        /// <summary>
        /// 所有敌人的列表
        /// </summary>
        private List<HealthComponent> enemies = new List<HealthComponent>();

        /// <summary>
        /// 已死亡的敌人数量
        /// </summary>
        private int deadEnemyCount = 0;


        /// <summary>
        /// 当前回合状态
        /// </summary>
        public CombatTurnState CurrentState => currentState;

        /// <summary>
        /// 当前节点类型（从节点数据获取）
        /// </summary>
        public string CurrentNodeType => currentNodeData != null ? currentNodeData.NodeType : string.Empty;

        /// <summary>
        /// 是否为战斗节点
        /// </summary>
        public bool IsCombatNode => CurrentNodeType == "Combat";

        /// <summary>
        /// 是否为精英节点
        /// </summary>
        public bool IsEliteNode => CurrentNodeType == "Elite";

        /// <summary>
        /// 是否为Boss节点
        /// </summary>
        public bool IsBossNode => currentNodeData != null && currentNodeData.IsBoss;

        /// <summary>
        /// 是否可以结束回合（只有在回合进行中状态才能结束）
        /// </summary>
        public bool CanEndTurn => currentState == CombatTurnState.TurnPlaying;

        private void Awake()
        {
            // 自动查找系统引用
            if (cardSystem == null)
            {
                cardSystem = FindObjectOfType<CardSystemManager>();
            }

            if (handWaveGridManager == null)
            {
                handWaveGridManager = FindObjectOfType<HandWaveGridManager>();
            }

            if (damageSystemHelper == null)
            {
                damageSystemHelper = FindObjectOfType<DamageSystem.DamageSystemHelper>();
            }

            if (damageSystem == null)
            {
                damageSystem = FindObjectOfType<DamageSystem.DamageSystem>();
            }
        }

        private void OnEnable()
        {
            // 订阅伤害序列完成事件
            if (damageSystem != null)
            {
                damageSystem.OnHitSequenceComplete.AddListener(OnHitSequenceComplete);
            }
        }

        private void OnDisable()
        {
            // 取消订阅
            if (damageSystem != null)
            {
                damageSystem.OnHitSequenceComplete.RemoveListener(OnHitSequenceComplete);
            }
            // 取消所有敌人的死亡事件订阅
            UnsubscribeAllEnemyDeathEvents();
        }

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

            // 确保系统引用已找到
            if (cardSystem == null)
            {
                cardSystem = FindObjectOfType<CardSystemManager>();
            }

            if (handWaveGridManager == null)
            {
                handWaveGridManager = FindObjectOfType<HandWaveGridManager>();
            }

            if (damageSystemHelper == null)
            {
                damageSystemHelper = FindObjectOfType<DamageSystem.DamageSystemHelper>();
            }

            if (damageSystem == null)
            {
                damageSystem = FindObjectOfType<DamageSystem.DamageSystem>();
            }

            // 自动查找系统引用
            if (playerEntityManager == null)
            {
                playerEntityManager = FindObjectOfType<CharacterSystem.PlayerEntityManager>();
            }

            if (enemySpawner == null)
            {
                enemySpawner = FindObjectOfType<CharacterSystem.EnemySpawner>();
            }

            if (squadManager == null)
            {
                squadManager = FindObjectOfType<SquadSystem.SquadManager>();
            }

            if (rewardManager == null)
            {
                rewardManager = FindObjectOfType<RewardManager>();
            }

            // 生成玩家实体
            if (playerEntityManager != null)
            {
                playerEntityManager.SpawnPlayerEntity();
            }
            else
            {
                Debug.LogWarning("[CombatNodeFlow] PlayerEntityManager 未找到，将使用场景中现有的玩家实体");
            }

            // 生成敌人实体
            if (enemySpawner != null)
            {
                List<GameObject> spawnedEnemies = enemySpawner.SpawnEnemies(null, CurrentNodeType);
                Debug.Log($"[CombatNodeFlow] 通过 EnemySpawner 生成了 {spawnedEnemies.Count} 个敌人");
            }
            else
            {
                Debug.LogWarning("[CombatNodeFlow] EnemySpawner 未找到，将使用场景中现有的敌人实体");
            }

            // 生成小队成员（战斗开始时）
            if (squadManager != null)
            {
                squadManager.SpawnSquadMembers();
            }
            else
            {
                Debug.Log("[CombatNodeFlow] SquadManager 未找到，跳过生成小队成员");
            }

            // 查找所有敌人（包括动态生成的）
            FindAllEnemies();

            // 订阅所有敌人的死亡事件（通过 HealthComponent.OnDeath）
            SubscribeAllEnemyDeathEvents();

            // 注意：玩家死亡事件已在PlayerEntityManager中全局订阅，不需要在这里订阅
            // 这样可以确保在任何时候玩家血量归零都会触发游戏结束，不受flow系统限制

            isCombatFinished = false;
            deadEnemyCount = 0;
            currentState = CombatTurnState.CombatStart;
            Debug.Log($"[CombatNodeFlow] 开始战斗流程: Node[{currentNodeData.NodeId}] Type:{CurrentNodeType}，找到 {enemies.Count} 个敌人");

            // 战斗开始时的逻辑：初始化牌堆和手牌堆
            if (cardSystem != null)
            {
                cardSystem.PrepareForCombat();
            }
            else
            {
                Debug.LogWarning("[CombatNodeFlow] CardSystem 未找到，无法初始化牌堆");
            }

            // 初始化手牌波显示（更新slot位置并显示初始波）
            if (handWaveGridManager != null)
            {
                handWaveGridManager.UpdateWaveDisplay();
            }

            // 初始化敌人波显示（EnemyWaveManager和HandWaveGridManager在一起）
            // 注意：敌人波数据应该从生成的敌人实例的EnemyWaveManager获取，而不是使用预设波
            if (handWaveGridManager != null)
            {
                EnemyWaveManager enemyWaveManager = handWaveGridManager.GetComponent<EnemyWaveManager>();
                if (enemyWaveManager != null)
                {
                    // 初始化敌人波显示器（使用手牌波的参数）
                    enemyWaveManager.InitializeWaveVisualizer(handWaveGridManager);
                    
                    // 注意：敌人波数据应该从生成的敌人实例获取，这里只初始化显示
                    // 实际的波数据会在敌人生成时设置，并在回合开始时更新
                    enemyWaveManager.UpdateWaveDisplay();
                }
            }

            // 开始回合系统
            EnterTurnStart();
        }

        /// <summary>
        /// 查找场景中所有敌人
        /// </summary>
        private void FindAllEnemies()
        {
            enemies.Clear();
            
            if (string.IsNullOrEmpty(enemyTag))
            {
                Debug.LogWarning("[CombatNodeFlow] 敌人Tag未设置，无法查找敌人");
                return;
            }

            GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag(enemyTag);
            foreach (GameObject enemyObj in enemyObjects)
            {
                HealthComponent healthComponent = enemyObj.GetComponent<HealthComponent>();
                if (healthComponent != null)
                {
                    enemies.Add(healthComponent);
                    Debug.Log($"[CombatNodeFlow] 找到敌人: {enemyObj.name}");
                }
                else
                {
                    Debug.LogWarning($"[CombatNodeFlow] 敌人 {enemyObj.name} 缺少 HealthComponent 组件");
                }
            }

            if (enemies.Count == 0)
            {
                Debug.LogWarning("[CombatNodeFlow] 未找到任何敌人，战斗将无法正常结束");
            }
        }

        /// <summary>
        /// 订阅所有敌人的死亡事件（通过 HealthComponent.OnDeath）
        /// </summary>
        private void SubscribeAllEnemyDeathEvents()
        {
            foreach (HealthComponent enemyHealth in enemies)
            {
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath.AddListener(() => OnEnemyDeathByHealthComponent(enemyHealth.gameObject));
                    Debug.Log($"[CombatNodeFlow] 订阅敌人 {enemyHealth.gameObject.name} 的死亡事件");
                }
            }
        }

        /// <summary>
        /// 取消所有敌人的死亡事件订阅
        /// </summary>
        private void UnsubscribeAllEnemyDeathEvents()
        {
            foreach (HealthComponent enemyHealth in enemies)
            {
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath.RemoveAllListeners();
                }
            }
        }

        // 注意：玩家死亡事件订阅已移至PlayerEntityManager中全局管理
        // 这样可以确保在任何时候玩家血量归零都会触发游戏结束，不受flow系统限制

        /// <summary>
        /// 敌人死亡回调（由HealthComponent.OnDeath触发）
        /// </summary>
        private void OnEnemyDeathByHealthComponent(GameObject deadTarget)
        {
            if (isCombatFinished)
            {
                return;
            }

            // 检查死亡的物体是否是敌人
            HealthComponent deadHealthComponent = deadTarget.GetComponent<HealthComponent>();
            if (deadHealthComponent == null || !enemies.Contains(deadHealthComponent))
            {
                // 不是敌人，忽略
                return;
            }

            deadEnemyCount++;
            Debug.Log($"[CombatNodeFlow] 敌人 {deadTarget.name} 死亡（通过HealthComponent.OnDeath），已死亡敌人数: {deadEnemyCount}/{enemies.Count}");

            // 检查是否所有敌人都已死亡，如果全部死亡则立即结束战斗
            if (deadEnemyCount >= enemies.Count)
            {
                Debug.Log("[CombatNodeFlow] 所有敌人已死亡，立即结束战斗");
                FinishCombat();
            }
        }

        /// <summary>
        /// 伤害序列处理完成回调（由DamageSystem触发）
        /// 注意：战斗结束不再依赖此事件，只要所有敌人死亡就立即结束战斗
        /// </summary>
        private void OnHitSequenceComplete(int totalHits, int deaths)
        {
            // 战斗结束判断已改为在敌人死亡时立即执行，此事件仅用于日志记录
            Debug.Log($"[CombatNodeFlow] 伤害序列处理完成，总伤害数：{totalHits}，死亡数：{deaths}");
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
                
                // 回合开始：为每个敌人生成随机波
                GenerateRandomWavesForAllEnemies();
                
                // 回合开始：抽牌
                if (cardSystem != null)
                {
                    cardSystem.DrawCards();
                }
                else
                {
                    Debug.LogWarning("[CombatNodeFlow] CardSystem 未找到，无法抽牌");
                }
                
                // 执行小队成员的回合开始能力
                if (squadManager != null)
                {
                    squadManager.ExecuteAbilitiesOnTurnStart();
                }
                
                // 自动进入回合进行中状态
                EnterTurnPlaying();
            }
            else
            {
                Debug.LogWarning($"[CombatNodeFlow] 无法从 {currentState} 状态进入回合开始状态");
            }
        }

        /// <summary>
        /// 为所有敌人生成随机波（回合开始时调用）
        /// </summary>
        private void GenerateRandomWavesForAllEnemies()
        {
            if (enemySpawner == null || enemySpawner.EnemyConfig == null)
            {
                Debug.LogWarning("[CombatNodeFlow] EnemySpawner或EnemyConfig未设置，无法生成随机波");
                return;
            }

            CharacterSystem.EnemyConfig enemyConfig = enemySpawner.EnemyConfig;
            
            // 调用独立的波生成器，为每个敌人实例生成随机波
            int successCount = CharacterSystem.EnemyWaveGenerator.GenerateRandomWavesForAllEnemies(enemies, enemyConfig);
            Debug.Log($"[CombatNodeFlow] 回合开始：为 {successCount} 个敌人生成了随机波");
            
            // 将第一个敌人的波数据同步到显示用的EnemyWaveManager（handWaveGridManager上的）
            // 注意：如果有多个敌人，这里只显示第一个敌人的波
            if (enemies.Count > 0 && handWaveGridManager != null)
            {
                GameObject firstEnemy = enemies[0].gameObject;
                EnemyWaveManager enemyInstanceWaveManager = firstEnemy.GetComponent<EnemyWaveManager>();
                EnemyWaveManager displayWaveManager = handWaveGridManager.GetComponent<EnemyWaveManager>();
                
                if (enemyInstanceWaveManager != null && displayWaveManager != null)
                {
                    // 将敌人实例的波数据复制到显示用的EnemyWaveManager
                    Wave enemyWave = enemyInstanceWaveManager.CurrentEnemyWave;
                    displayWaveManager.SetEnemyWave(enemyWave);
                    Debug.Log($"[CombatNodeFlow] 已将第一个敌人的波数据同步到显示用的EnemyWaveManager，波峰数: {enemyWave.PeakCount}");
                }
                else
                {
                    Debug.LogWarning($"[CombatNodeFlow] 无法同步敌人波数据: enemyInstanceWaveManager={(enemyInstanceWaveManager != null ? "存在" : "null")}, displayWaveManager={(displayWaveManager != null ? "存在" : "null")}");
                }
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
                // 回合进行中：玩家可以操作卡牌等
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
                
                // 回合结束：完整流程（发出玩家波 -> 获取敌人波 -> 配对 -> 生成伤害序列 -> 处理伤害序列）
                if (damageSystemHelper != null)
                {
                    // 使用 DamageSystemHelper 的完整流程
                    damageSystemHelper.ProcessEmittedWave();
                }
                else
                {
                    Debug.LogWarning("[CombatNodeFlow] DamageSystemHelper 未找到，无法执行回合结束操作");
                }
                
                // 弃牌（在伤害处理之后）
                if (handWaveGridManager != null)
                {
                    handWaveGridManager.DiscardPendingCards();
                }
                else
                {
                    Debug.LogWarning("[CombatNodeFlow] HandWaveGridManager 未找到，无法弃牌");
                }

                // 处理状态效果的回合结束（减少持续回合数）
                ProcessStatusEffectsTurnEnd();
                
                // 执行小队成员的回合结束能力
                if (squadManager != null)
                {
                    squadManager.ExecuteAbilitiesOnTurnEnd();
                }
                
                // 检查战斗是否应该结束
                // 敌人死亡检查通过 DamageSystem 的 OnTargetDeath 事件自动处理
                // 如果所有敌人死亡，会自动调用 FinishCombat()
                
                // 默认继续下一回合（如果战斗未结束）
                if (!isCombatFinished)
                {
                    EnterTurnStart();
                }
            }
            else
            {
                Debug.LogWarning($"[CombatNodeFlow] 无法从 {currentState} 状态进入回合结束状态");
            }
        }

        /// <summary>
        /// 处理状态效果的回合结束（减少持续回合数）
        /// </summary>
        private void ProcessStatusEffectsTurnEnd()
        {
            // 处理玩家状态效果
            if (playerEntityManager != null && playerEntityManager.CurrentPlayerEntity != null)
            {
                StatusEffectManager playerStatusManager = playerEntityManager.CurrentPlayerEntity.GetComponent<StatusEffectManager>();
                if (playerStatusManager != null)
                {
                    playerStatusManager.OnTurnEnd();
                }
            }

            // 处理所有敌人状态效果（通过TargetManager获取）
            DamageSystem.TargetManager targetManager = FindObjectOfType<DamageSystem.TargetManager>();
            if (targetManager != null)
            {
                GameObject[] allEnemies = targetManager.GetAllEnemies();
                foreach (GameObject enemy in allEnemies)
                {
                    if (enemy != null)
                    {
                        StatusEffectManager enemyStatusManager = enemy.GetComponent<StatusEffectManager>();
                        if (enemyStatusManager != null)
                        {
                            enemyStatusManager.OnTurnEnd();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 发放奖励（根据节点类型）
        /// </summary>
        private void GiveReward()
        {
            if (rewardManager == null)
            {
                Debug.LogWarning("[CombatNodeFlow] 奖励管理器未找到，跳过奖励发放");
                return;
            }

            rewardManager.GiveReward(CurrentNodeType, IsBossNode);
        }

        /// <summary>
        /// 完成战斗
        /// 由战斗系统在战斗结束时调用（当所有敌人死亡时自动调用）
        /// </summary>
        public void FinishCombat()
        {
            if (isCombatFinished)
            {
                return;
            }

            isCombatFinished = true;
            Debug.Log($"[CombatNodeFlow] 战斗完成: Node[{currentNodeData.NodeId}]");

            // 取消订阅伤害序列完成事件
            if (damageSystem != null)
            {
                damageSystem.OnHitSequenceComplete.RemoveListener(OnHitSequenceComplete);
            }

            // 取消所有敌人的死亡事件订阅
            UnsubscribeAllEnemyDeathEvents();

            // 注意：玩家死亡事件订阅在PlayerEntityManager中全局管理，不需要在这里取消

            // 注意：不在这里销毁玩家实体，等待玩家确认奖励后再销毁（在 FinishRewardAndFlow 中处理）
            // 这样可以确保所有伤害都处理完毕后再保存数据

            // 清除所有敌人实体
            if (enemySpawner != null)
            {
                enemySpawner.ClearAllEnemies();
            }
            else
            {
                // 如果没有 EnemySpawner，手动清除通过 Tag 找到的敌人
                // 注意：这只会清除场景中现有的敌人，不会清除动态生成的
                GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag(enemyTag);
                foreach (GameObject enemyObj in enemyObjects)
                {
                    Destroy(enemyObj);
                }
                Debug.Log($"[CombatNodeFlow] 手动清除了 {enemyObjects.Length} 个敌人实体");
            }

            // 清除所有小队成员
            if (squadManager != null)
            {
                squadManager.ClearMembers();
            }

            // 清空敌人列表和状态标志
            enemies.Clear();
            deadEnemyCount = 0;

            // 如果战斗类型是Boss，敌人死亡导致节点结束时进入游戏结束状态
            if (IsBossNode)
            {
                Debug.Log("[CombatNodeFlow] Boss节点战斗完成，进入游戏结束状态");
                
                // 获取GameFlowManager并触发游戏结束
                GameFlowManager gameFlowManager = FindObjectOfType<GameFlowManager>();
                if (gameFlowManager != null)
                {
                    gameFlowManager.HandleGameEnd();
                }
                else
                {
                    Debug.LogError("[CombatNodeFlow] 未找到GameFlowManager，无法触发游戏结束");
                }
                
                // Boss节点直接结束流程，不等待奖励确认
                FinishFlow();
                return;
            }

            // 发放奖励（根据节点类型）
            GiveReward();

            // 注意：不立即结束流程，等待玩家通过 RewardManager.FinishRewardAndFlow() 确认奖励后结束
            Debug.Log("[CombatNodeFlow] 战斗完成，等待玩家确认奖励后结束流程");
        }

        /// <summary>
        /// 完成奖励并结束流程（由 RewardManager 调用）
        /// 玩家通过按钮确认奖励后调用此方法
        /// </summary>
        public void FinishRewardAndFlow()
        {
            Debug.Log("[CombatNodeFlow] 玩家确认奖励，准备结束流程");

            // 同步玩家实体数据回玩家数据，然后销毁玩家实体（在确认奖励后，确保所有伤害都已处理）
            if (playerEntityManager != null)
            {
                playerEntityManager.DestroyPlayerEntity();
            }

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

