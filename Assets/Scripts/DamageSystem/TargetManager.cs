using UnityEngine;

namespace DamageSystem
{
    /// <summary>
    /// 目标管理器
    /// 管理玩家和敌人的引用，用于伤害系统查找目标
    /// </summary>
    public class TargetManager : MonoBehaviour
    {
        [Header("目标设置")]
        [Tooltip("玩家GameObject（必须挂载HealthComponent）")]
        [SerializeField] private GameObject player;

        [Tooltip("敌人GameObject（必须挂载HealthComponent）")]
        [SerializeField] private GameObject enemy;

        [Header("自动查找设置")]
        [Tooltip("是否在Start时自动查找玩家和敌人（通过Tag）")]
        [SerializeField] private bool autoFindTargets = true;

        [Tooltip("玩家Tag")]
        [SerializeField] private string playerTag = "Player";

        [Tooltip("敌人Tag")]
        [SerializeField] private string enemyTag = "Enemy";

        /// <summary>
        /// 玩家GameObject
        /// </summary>
        public GameObject Player => player;

        /// <summary>
        /// 敌人GameObject
        /// </summary>
        public GameObject Enemy => enemy;

        private void Start()
        {
            if (autoFindTargets)
            {
                FindTargets();
            }

            ValidateTargets();
        }

        /// <summary>
        /// 自动查找玩家和敌人
        /// </summary>
        private void FindTargets()
        {
            if (player == null && !string.IsNullOrEmpty(playerTag))
            {
                GameObject foundPlayer = GameObject.FindGameObjectWithTag(playerTag);
                if (foundPlayer != null)
                {
                    player = foundPlayer;
                    Debug.Log($"[TargetManager] 自动找到玩家: {player.name}");
                }
            }

            if (enemy == null && !string.IsNullOrEmpty(enemyTag))
            {
                GameObject foundEnemy = GameObject.FindGameObjectWithTag(enemyTag);
                if (foundEnemy != null)
                {
                    enemy = foundEnemy;
                    Debug.Log($"[TargetManager] 自动找到敌人: {enemy.name}");
                }
            }
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
        /// </summary>
        /// <param name="attackDirection">攻击方向（true=攻向敌人）</param>
        /// <returns>目标GameObject，如果不存在则返回null</returns>
        public GameObject GetTargetByDirection(bool attackDirection)
        {
            return attackDirection ? enemy : player;
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

