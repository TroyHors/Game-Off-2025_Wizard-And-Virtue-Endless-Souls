using System.Collections.Generic;
using UnityEngine;
using CurrencySystem;
using DamageSystem;

namespace SquadSystem
{
    /// <summary>
    /// 小队管理器
    /// 管理小队数据和成员实例
    /// </summary>
    public class SquadManager : MonoBehaviour
    {
        [Header("小队数据")]
        [Tooltip("小队数据（包含成员ID列表）")]
        [SerializeField] private SquadData squadData;

        [Tooltip("成员数据注册表（用于根据ID获取成员数据）")]
        [SerializeField] private MemberDataRegistry memberDataRegistry;

        [Header("成员生成设置")]
        [Tooltip("成员生成位置容器（成员会作为此GameObject的子对象生成）")]
        [SerializeField] private Transform memberSpawnContainer;

        [Header("系统引用")]
        [Tooltip("金币系统（用于扣除雇佣金）")]
        [SerializeField] private CoinSystem coinSystem;

        [Tooltip("目标管理器（用于能力执行）")]
        [SerializeField] private TargetManager targetManager;

        [Header("运行时状态")]
        [Tooltip("当前生成的成员实例列表")]
        [SerializeField] private List<GameObject> currentMembers = new List<GameObject>();

        /// <summary>
        /// 小队数据
        /// </summary>
        public SquadData SquadData
        {
            get => squadData;
            set => squadData = value;
        }

        /// <summary>
        /// 当前成员实例列表（只读）
        /// </summary>
        public IReadOnlyList<GameObject> CurrentMembers => currentMembers;

        /// <summary>
        /// 当前成员数量
        /// </summary>
        public int CurrentMemberCount => currentMembers.Count;

        private void Awake()
        {
            // 自动查找系统引用
            if (coinSystem == null)
            {
                coinSystem = FindObjectOfType<CoinSystem>();
            }

            if (targetManager == null)
            {
                targetManager = FindObjectOfType<TargetManager>();
            }

            // 如果没有设置生成容器，使用自身作为容器
            if (memberSpawnContainer == null)
            {
                memberSpawnContainer = transform;
            }
        }

        /// <summary>
        /// 生成小队成员（战斗开始时调用）
        /// </summary>
        /// <returns>生成的成员实例列表</returns>
        public List<GameObject> SpawnSquadMembers()
        {
            // 先清除现有成员
            ClearMembers();

            if (squadData == null)
            {
                Debug.LogWarning("[SquadManager] 小队数据未设置，无法生成成员");
                return new List<GameObject>();
            }

            if (memberDataRegistry == null)
            {
                Debug.LogWarning("[SquadManager] 成员数据注册表未设置，无法生成成员");
                return new List<GameObject>();
            }

            List<GameObject> spawnedMembers = new List<GameObject>();
            int totalCost = 0;

            // 遍历小队中的成员ID，生成成员实例
            foreach (string memberId in squadData.MemberIds)
            {
                MemberData memberData = memberDataRegistry.GetMemberData(memberId);
                if (memberData == null)
                {
                    Debug.LogWarning($"[SquadManager] 未找到成员ID '{memberId}' 对应的成员数据，跳过");
                    continue;
                }

                // 检查是否有足够的金币支付雇佣金
                int hireCost = memberData.HireCost;
                if (hireCost > 0 && coinSystem != null)
                {
                    if (!coinSystem.HasEnoughCoins(hireCost))
                    {
                        Debug.LogWarning($"[SquadManager] 金币不足，无法雇佣成员 '{memberData.MemberName}' (需要 {hireCost} 金币)");
                        continue;
                    }
                }

                // 生成成员实例
                GameObject memberInstance = SpawnMember(memberData);
                if (memberInstance != null)
                {
                    spawnedMembers.Add(memberInstance);
                    totalCost += hireCost;
                }
            }

            // 扣除总雇佣金
            if (totalCost > 0 && coinSystem != null)
            {
                coinSystem.RemoveCoins(totalCost);
                Debug.Log($"[SquadManager] 扣除雇佣金: {totalCost} 金币");
            }

            currentMembers = spawnedMembers;
            Debug.Log($"[SquadManager] 生成小队成员完成，共 {spawnedMembers.Count} 个成员，总雇佣金: {totalCost}");

            // 执行战斗开始时的能力
            ExecuteAbilitiesOnCombatStart();

            return spawnedMembers;
        }

        /// <summary>
        /// 生成单个成员实例
        /// </summary>
        private GameObject SpawnMember(MemberData memberData)
        {
            if (memberData.MemberPrefab == null)
            {
                Debug.LogError($"[SquadManager] 成员 '{memberData.MemberName}' 的Prefab未设置");
                return null;
            }

            // 实例化成员Prefab
            GameObject memberInstance = Instantiate(memberData.MemberPrefab, memberSpawnContainer);
            memberInstance.name = $"Member_{memberData.MemberName}";

            // 初始化成员组件
            MemberComponent memberComponent = memberInstance.GetComponent<MemberComponent>();
            if (memberComponent != null)
            {
                memberComponent.Initialize(memberData);
            }
            else
            {
                Debug.LogError($"[SquadManager] 成员Prefab '{memberData.MemberName}' 缺少 MemberComponent 组件");
                Destroy(memberInstance);
                return null;
            }

            Debug.Log($"[SquadManager] 生成成员: {memberData.MemberName} (ID: {memberData.MemberId}, 雇佣金: {memberData.HireCost})");
            return memberInstance;
        }

        /// <summary>
        /// 添加成员到小队（只更新数据，不生成实例）
        /// 成员实例在战斗开始时统一生成
        /// </summary>
        /// <param name="memberId">成员ID</param>
        /// <returns>是否成功添加</returns>
        public bool AddMember(string memberId)
        {
            if (squadData == null)
            {
                Debug.LogError("[SquadManager] 小队数据未设置，无法添加成员");
                return false;
            }

            if (memberDataRegistry == null)
            {
                Debug.LogError("[SquadManager] 成员数据注册表未设置，无法添加成员");
                return false;
            }

            // 验证成员数据是否存在
            MemberData memberData = memberDataRegistry.GetMemberData(memberId);
            if (memberData == null)
            {
                Debug.LogError($"[SquadManager] 未找到成员ID '{memberId}' 对应的成员数据");
                return false;
            }

            // 检查是否有足够的金币支付雇佣金（只检查，不扣除，雇佣金在战斗开始时扣除）
            int hireCost = memberData.HireCost;
            if (hireCost > 0 && coinSystem != null)
            {
                if (!coinSystem.HasEnoughCoins(hireCost))
                {
                    Debug.LogWarning($"[SquadManager] 金币不足，无法添加成员 '{memberData.MemberName}' (需要 {hireCost} 金币)");
                    return false;
                }
            }

            // 只添加到小队数据，不生成实例（实例在战斗开始时统一生成）
            if (squadData.AddMember(memberId))
            {
                Debug.Log($"[SquadManager] 添加成员到小队数据: {memberData.MemberName} (ID: {memberId})，实例将在战斗开始时生成");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 从小队移除成员（只更新数据，不销毁实例）
        /// 如果正在战斗中，实例会在战斗结束时统一销毁
        /// </summary>
        /// <param name="memberId">成员ID</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveMember(string memberId)
        {
            if (squadData == null)
            {
                Debug.LogError("[SquadManager] 小队数据未设置，无法移除成员");
                return false;
            }

            // 只从小队数据移除，不销毁实例（如果正在战斗中，实例会在战斗结束时统一销毁）
            if (squadData.RemoveMember(memberId))
            {
                Debug.Log($"[SquadManager] 从小队数据移除成员: {memberId}，实例将在战斗结束时统一销毁");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 清除所有成员实例
        /// </summary>
        public void ClearMembers()
        {
            foreach (GameObject member in currentMembers)
            {
                if (member != null)
                {
                    Destroy(member);
                }
            }
            currentMembers.Clear();
            Debug.Log("[SquadManager] 清除所有成员实例");
        }

        /// <summary>
        /// 执行战斗开始时的能力
        /// </summary>
        private void ExecuteAbilitiesOnCombatStart()
        {
            // 创建副本以避免在遍历时集合被修改
            List<GameObject> membersCopy = new List<GameObject>(currentMembers);
            foreach (GameObject member in membersCopy)
            {
                if (member == null) continue;

                MemberComponent memberComponent = member.GetComponent<MemberComponent>();
                if (memberComponent == null || memberComponent.MemberData == null) continue;

                MemberData memberData = memberComponent.MemberData;
                foreach (MemberAbility ability in memberData.Abilities)
                {
                    if (ability.Trigger == AbilityTrigger.CombatStart)
                    {
                        MemberAbilityExecutor.ExecuteAbility(ability, member, targetManager);
                    }
                }
            }
        }

        /// <summary>
        /// 执行回合开始时的能力
        /// </summary>
        public void ExecuteAbilitiesOnTurnStart()
        {
            // 创建副本以避免在遍历时集合被修改
            List<GameObject> membersCopy = new List<GameObject>(currentMembers);
            foreach (GameObject member in membersCopy)
            {
                if (member == null) continue;

                MemberComponent memberComponent = member.GetComponent<MemberComponent>();
                if (memberComponent == null || memberComponent.MemberData == null) continue;

                MemberData memberData = memberComponent.MemberData;
                foreach (MemberAbility ability in memberData.Abilities)
                {
                    if (ability.Trigger == AbilityTrigger.TurnStart)
                    {
                        MemberAbilityExecutor.ExecuteAbility(ability, member, targetManager);
                    }
                }
            }
        }

        /// <summary>
        /// 执行回合结束时的能力
        /// </summary>
        public void ExecuteAbilitiesOnTurnEnd()
        {
            // 创建副本以避免在遍历时集合被修改
            // 如果成员能力导致战斗结束，ClearMembers() 会清空原列表，但不会影响副本
            List<GameObject> membersCopy = new List<GameObject>(currentMembers);
            foreach (GameObject member in membersCopy)
            {
                if (member == null) continue;

                MemberComponent memberComponent = member.GetComponent<MemberComponent>();
                if (memberComponent == null || memberComponent.MemberData == null) continue;

                MemberData memberData = memberComponent.MemberData;
                foreach (MemberAbility ability in memberData.Abilities)
                {
                    if (ability.Trigger == AbilityTrigger.TurnEnd)
                    {
                        MemberAbilityExecutor.ExecuteAbility(ability, member, targetManager);
                    }
                }
            }
        }
    }
}

