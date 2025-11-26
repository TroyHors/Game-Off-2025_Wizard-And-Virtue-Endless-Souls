using UnityEngine;

namespace SquadSystem
{
    /// <summary>
    /// 小队数据测试器
    /// 用于测试小队数据的添加/删除功能
    /// </summary>
    public class SquadDataTester : MonoBehaviour
    {
        [Header("系统引用")]
        [Tooltip("小队管理器（用于添加成员，会自动同步数据和实例）")]
        [SerializeField] private SquadManager squadManager;

        [Tooltip("成员数据注册表")]
        [SerializeField] private MemberDataRegistry memberDataRegistry;

        [Header("测试参数")]
        [Tooltip("要添加的成员索引（在注册表中的索引）")]
        [SerializeField] private int memberIndex = 0;

        /// <summary>
        /// 按索引添加成员到小队
        /// </summary>
        [ContextMenu("按索引添加成员到小队")]
        public void AddMemberByIndex()
        {
            if (squadManager == null)
            {
                Debug.LogError("[SquadDataTester] 小队管理器未设置");
                return;
            }

            if (memberDataRegistry == null)
            {
                Debug.LogError("[SquadDataTester] 成员数据注册表未设置");
                return;
            }

            // 获取所有成员数据
            var allMemberData = memberDataRegistry.GetAllMemberData();
            if (allMemberData == null || allMemberData.Count == 0)
            {
                Debug.LogWarning("[SquadDataTester] 成员数据注册表为空");
                return;
            }

            // 检查索引是否有效
            if (memberIndex < 0 || memberIndex >= allMemberData.Count)
            {
                Debug.LogError($"[SquadDataTester] 成员索引 {memberIndex} 无效，注册表中共有 {allMemberData.Count} 个成员");
                return;
            }

            // 获取指定索引的成员数据
            MemberData memberData = allMemberData[memberIndex];
            if (memberData == null)
            {
                Debug.LogError($"[SquadDataTester] 索引 {memberIndex} 的成员数据为空");
                return;
            }

            // 通过 SquadManager 添加成员（会自动同步数据和实例）
            string memberId = memberData.MemberId;
            if (squadManager.AddMember(memberId))
            {
                Debug.Log($"[SquadDataTester] 成功添加成员到小队: {memberData.MemberName} (ID: {memberId}, 索引: {memberIndex})");
                Debug.Log($"[SquadDataTester] 当前小队数据成员数: {squadManager.SquadData?.MemberCount ?? 0}");
                Debug.Log($"[SquadDataTester] 当前成员实例数: {squadManager.CurrentMemberCount}");
            }
            else
            {
                Debug.LogWarning($"[SquadDataTester] 添加成员失败: {memberData.MemberName} (ID: {memberId}, 索引: {memberIndex})");
            }
        }
    }
}

