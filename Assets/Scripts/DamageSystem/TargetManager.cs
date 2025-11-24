using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DamageSystem
{
    /// <summary>
    /// 目标管理器
    /// 管理玩家和敌人的引用，用于伤害系统查找目标
    /// 支持动态查找动态生成的角色
    /// </summary>
    public class TargetManager : MonoBehaviour
    {
        [Header("目标设置")]
        [Tooltip("玩家GameObject（必须挂载HealthComponent）")]
        [SerializeField] private GameObject player;

        [Tooltip("敌人GameObject（必须挂载HealthComponent，如果有多个敌人，使用第一个）")]
        [SerializeField] private GameObject enemy;

        [Header("自动查找设置")]
        [Tooltip("是否在Start时自动查找玩家和敌人（通过Tag）")]
        [SerializeField] private bool autoFindTargets = true;

        [Tooltip("是否在获取目标时自动刷新（如果目标为空）")]
        [SerializeField] private bool autoRefreshOnGet = true;

        [Tooltip("玩家Tag")]
        [SerializeField] private string playerTag = "Player";

        [Tooltip("敌人Tag")]
        [SerializeField] private string enemyTag = "Enemy";

        /// <summary>
        /// 玩家GameObject
        /// </summary>
        public GameObject Player
        {
            get
            {
                if (player == null && autoRefreshOnGet)
                {
                    RefreshPlayer();
                }
                return player;
            }
        }

        /// <summary>
        /// 敌人GameObject（如果有多个敌人，返回第一个）
        /// </summary>
        public GameObject Enemy
        {
            get
            {
                if (enemy == null && autoRefreshOnGet)
                {
                    RefreshEnemy();
                }
                return enemy;
            }
        }

        private void Start()
        {
            if (autoFindTargets)
            {
                RefreshTargets();
            }

            ValidateTargets();
        }

        /// <summary>
        /// 刷新所有目标（公共方法，可在角色生成后调用）
        /// </summary>
        public void RefreshTargets()
        {
            RefreshPlayer();
            RefreshEnemy();
        }

        /// <summary>
        /// 刷新玩家引用
        /// </summary>
        public void RefreshPlayer()
        {
            if (!string.IsNullOrEmpty(playerTag))
            {
                GameObject foundPlayer = GameObject.FindGameObjectWithTag(playerTag);
                if (foundPlayer != null)
                {
                    player = foundPlayer;
                    Debug.Log($"[TargetManager] 刷新玩家引用: {player.name}");
                    ValidateTargets();
                }
            }
        }

        /// <summary>
        /// 刷新敌人引用（如果有多个敌人，使用第一个）
        /// </summary>
        public void RefreshEnemy()
        {
            if (!string.IsNullOrEmpty(enemyTag))
            {
                GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag(enemyTag);
                if (foundEnemies != null && foundEnemies.Length > 0)
                {
                    // 如果有多个敌人，使用第一个
                    enemy = foundEnemies[0];
                    Debug.Log($"[TargetManager] 刷新敌人引用: {enemy.name}（共找到 {foundEnemies.Length} 个敌人）");
                    ValidateTargets();
                }
            }
        }

        /// <summary>
        /// 获取所有敌人（用于多敌人场景）
        /// </summary>
        /// <returns>所有敌人的GameObject数组</returns>
        public GameObject[] GetAllEnemies()
        {
            if (!string.IsNullOrEmpty(enemyTag))
            {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
                if (enemies != null && enemies.Length > 0)
                {
                    return enemies;
                }
            }
            return new GameObject[0];
        }

        /// <summary>
        /// 验证目标是否有效（是否有HealthComponent）
        /// </summary>
        private void ValidateTargets()
        {
            if (player != null && player.GetComponent<HealthComponent>() == null)
            {
                Debug.LogError($"[TargetManager] 玩家 {player.name} 缺少 HealthComponent 组件");
            }

            if (enemy != null && enemy.GetComponent<HealthComponent>() == null)
            {
                Debug.LogError($"[TargetManager] 敌人 {enemy.name} 缺少 HealthComponent 组件");
            }
        }

        /// <summary>
        /// 根据攻击方向获取目标
        /// true = 攻向敌人，返回敌人
        /// false = 攻向玩家，返回玩家
        /// 如果目标为空且启用了自动刷新，会尝试重新查找
        /// </summary>
        /// <param name="attackDirection">攻击方向（true=攻向敌人）</param>
        /// <returns>目标GameObject，如果不存在则返回null</returns>
        public GameObject GetTargetByDirection(bool attackDirection)
        {
            if (attackDirection)
            {
                // 攻向敌人
                if (enemy == null && autoRefreshOnGet)
                {
                    RefreshEnemy();
                }
                return enemy;
            }
            else
            {
                // 攻向玩家
                if (player == null && autoRefreshOnGet)
                {
                    RefreshPlayer();
                }
                return player;
            }
        }

        /// <summary>
        /// 设置玩家引用
        /// </summary>
        /// <param name="playerObject">玩家GameObject</param>
        public void SetPlayer(GameObject playerObject)
        {
            player = playerObject;
            ValidateTargets();
        }

        /// <summary>
        /// 设置敌人引用
        /// </summary>
        /// <param name="enemyObject">敌人GameObject</param>
        public void SetEnemy(GameObject enemyObject)
        {
            enemy = enemyObject;
            ValidateTargets();
        }
    }
}

