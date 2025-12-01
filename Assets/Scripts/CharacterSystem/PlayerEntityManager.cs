using UnityEngine;
using DamageSystem;
using GameFlow;

namespace CharacterSystem
{
    /// <summary>
    /// 玩家实体管理器
    /// 负责根据玩家数据动态生成和销毁玩家实体
    /// </summary>
    public class PlayerEntityManager : MonoBehaviour
    {
        [Header("系统引用")]
        [Tooltip("伤害系统（用于订阅玩家死亡事件）")]
        [SerializeField] private DamageSystem.DamageSystem damageSystem;
        [Header("玩家数据")]
        [Tooltip("玩家数据（持久化数据）")]
        [SerializeField] private PlayerData playerData;

        [Header("玩家实体设置")]
        [Tooltip("玩家实体Prefab（必须包含 HealthComponent 组件）")]
        [SerializeField] private GameObject playerEntityPrefab;

        [Tooltip("玩家实体生成位置")]
        [SerializeField] private Transform playerSpawnPoint;

        [Tooltip("玩家Tag（用于 TargetManager 查找）")]
        [SerializeField] private string playerTag = "Player";

        [Header("运行时状态")]
        [Tooltip("当前玩家实体实例")]
        [SerializeField] private GameObject currentPlayerEntity;

        /// <summary>
        /// 玩家数据
        /// </summary>
        public PlayerData PlayerData
        {
            get => playerData;
            set => playerData = value;
        }

        /// <summary>
        /// 当前玩家实体实例
        /// </summary>
        public GameObject CurrentPlayerEntity => currentPlayerEntity;

        /// <summary>
        /// 是否已生成玩家实体
        /// </summary>
        public bool HasPlayerEntity => currentPlayerEntity != null;

        /// <summary>
        /// 生成玩家实体（战斗开始时调用）
        /// </summary>
        /// <returns>生成的玩家实体，如果失败返回 null</returns>
        public GameObject SpawnPlayerEntity()
        {
            if (playerData == null)
            {
                Debug.LogError("[PlayerEntityManager] 玩家数据未设置，无法生成玩家实体");
                return null;
            }

            if (playerEntityPrefab == null)
            {
                Debug.LogError("[PlayerEntityManager] 玩家实体Prefab未设置，无法生成玩家实体");
                return null;
            }

            // 如果已经存在实体，先销毁
            if (currentPlayerEntity != null)
            {
                Debug.LogWarning("[PlayerEntityManager] 玩家实体已存在，先销毁旧实体");
                DestroyPlayerEntity();
            }

            // 确定生成位置和父对象
            Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            Quaternion spawnRotation = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;
            Transform parentTransform = playerSpawnPoint; // 作为spawn point的子对象

            // 实例化玩家实体（作为spawn point的子对象）
            currentPlayerEntity = Instantiate(playerEntityPrefab, spawnPosition, spawnRotation, parentTransform);
            currentPlayerEntity.name = "PlayerEntity";
            currentPlayerEntity.tag = playerTag;

            // 获取 HealthComponent 并应用玩家数据
            HealthComponent healthComponent = currentPlayerEntity.GetComponent<HealthComponent>();
            if (healthComponent == null)
            {
                Debug.LogError("[PlayerEntityManager] 玩家实体Prefab缺少 HealthComponent 组件");
                Destroy(currentPlayerEntity);
                currentPlayerEntity = null;
                return null;
            }

            // 将玩家数据应用到实体
            playerData.ApplyToEntity(healthComponent);

            Debug.Log($"[PlayerEntityManager] 玩家实体已生成，生命值：{healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");

            // 订阅玩家死亡事件（通过DamageSystem，全局监听，独立于flow系统）
            SubscribePlayerDeathEvent();

            // 通知 TargetManager 刷新玩家引用
            RefreshTargetManager();

            return currentPlayerEntity;
        }

        /// <summary>
        /// 销毁玩家实体（战斗结束时调用）
        /// 在销毁前会将实体数据同步回玩家数据
        /// </summary>
        public void DestroyPlayerEntity()
        {
            if (currentPlayerEntity == null)
            {
                return;
            }

            // 取消玩家死亡事件订阅
            UnsubscribePlayerDeathEvent();

            // 同步实体数据回玩家数据
            if (playerData != null)
            {
                HealthComponent healthComponent = currentPlayerEntity.GetComponent<HealthComponent>();
                if (healthComponent != null)
                {
                    playerData.SyncFromEntity(healthComponent);
                }
            }

            // 销毁实体
            Destroy(currentPlayerEntity);
            currentPlayerEntity = null;

            Debug.Log("[PlayerEntityManager] 玩家实体已销毁，数据已同步回玩家数据");
        }

        /// <summary>
        /// 隐藏玩家实体（不销毁，保留在场景中但不可见）
        /// </summary>
        public void HidePlayerEntity()
        {
            if (currentPlayerEntity != null)
            {
                currentPlayerEntity.SetActive(false);
                Debug.Log("[PlayerEntityManager] 玩家实体已隐藏");
            }
        }

        /// <summary>
        /// 显示玩家实体（如果实体已存在但被隐藏）
        /// </summary>
        public void ShowPlayerEntity()
        {
            if (currentPlayerEntity != null)
            {
                currentPlayerEntity.SetActive(true);
                Debug.Log("[PlayerEntityManager] 玩家实体已显示");
            }
        }

        /// <summary>
        /// 通知 TargetManager 刷新玩家引用
        /// </summary>
        private void RefreshTargetManager()
        {
            TargetManager targetManager = FindObjectOfType<TargetManager>();
            if (targetManager != null)
            {
                targetManager.RefreshPlayer();
            }
        }

        /// <summary>
        /// 订阅玩家死亡事件（通过DamageSystem，全局监听，独立于flow系统）
        /// 在任何时候玩家血量归零立刻打断任何进程进入游戏结束状态
        /// </summary>
        private void SubscribePlayerDeathEvent()
        {
            // 自动查找DamageSystem（如果未设置）
            if (damageSystem == null)
            {
                damageSystem = FindObjectOfType<DamageSystem.DamageSystem>();
            }

            if (damageSystem == null)
            {
                Debug.LogError("[PlayerEntityManager] DamageSystem未找到，无法订阅玩家死亡事件！玩家死亡时将无法触发游戏结束");
                return;
            }

            // 先取消之前的订阅（避免重复订阅）
            damageSystem.OnTargetDeath.RemoveListener(OnTargetDeath);

            // 订阅DamageSystem的目标死亡事件
            damageSystem.OnTargetDeath.AddListener(OnTargetDeath);
            Debug.Log($"[PlayerEntityManager] 成功订阅DamageSystem的玩家死亡事件（全局监听），DamageSystem: {damageSystem.name}");
        }

        /// <summary>
        /// 取消玩家死亡事件订阅
        /// </summary>
        private void UnsubscribePlayerDeathEvent()
        {
            if (damageSystem == null)
            {
                return;
            }

            damageSystem.OnTargetDeath.RemoveListener(OnTargetDeath);
            Debug.Log("[PlayerEntityManager] 取消DamageSystem的玩家死亡事件订阅");
        }

        /// <summary>
        /// 目标死亡回调（由DamageSystem.OnTargetDeath触发）
        /// 检查是否是玩家死亡，如果是则触发游戏结束
        /// 独立于flow系统，在任何时候都会触发游戏结束
        /// </summary>
        private void OnTargetDeath(GameObject deadTarget)
        {
            if (deadTarget == null)
            {
                Debug.LogWarning("[PlayerEntityManager] OnTargetDeath收到空目标，忽略");
                return;
            }

            // 检查死亡的目标是否是玩家实体（通过Tag或实例比较）
            bool isPlayer = false;
            if (deadTarget.CompareTag("Player") || deadTarget.CompareTag(playerTag))
            {
                isPlayer = true;
            }
            else if (currentPlayerEntity != null && deadTarget == currentPlayerEntity)
            {
                isPlayer = true;
            }

            if (!isPlayer)
            {
                // 不是玩家，忽略（但记录日志以便调试）
                Debug.Log($"[PlayerEntityManager] OnTargetDeath收到非玩家目标: {deadTarget.name}，忽略");
                return;
            }

            Debug.Log($"[PlayerEntityManager] 玩家死亡（通过DamageSystem），目标: {deadTarget.name}，立即进入游戏结束状态（全局监听）");

            // 获取GameFlowManager并触发游戏结束
            GameFlowManager gameFlowManager = FindObjectOfType<GameFlowManager>();
            if (gameFlowManager != null)
            {
                gameFlowManager.HandleGameEnd();
            }
            else
            {
                Debug.LogError("[PlayerEntityManager] 未找到GameFlowManager，无法触发游戏结束");
            }
        }
    }
}

