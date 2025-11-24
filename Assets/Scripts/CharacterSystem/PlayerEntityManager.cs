using UnityEngine;
using DamageSystem;

namespace CharacterSystem
{
    /// <summary>
    /// 玩家实体管理器
    /// 负责根据玩家数据动态生成和销毁玩家实体
    /// </summary>
    public class PlayerEntityManager : MonoBehaviour
    {
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

            // 确定生成位置
            Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
            Quaternion spawnRotation = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;

            // 实例化玩家实体
            currentPlayerEntity = Instantiate(playerEntityPrefab, spawnPosition, spawnRotation);
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
    }
}

