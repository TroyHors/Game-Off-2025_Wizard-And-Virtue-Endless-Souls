using System.Collections.Generic;
using UnityEngine;

namespace SquadSystem
{
    /// <summary>
    /// 小队数据
    /// ScriptableObject，管理小队中的成员列表（只存储成员ID）
    /// 支持战斗外更改（添加/删除成员）
    /// </summary>
    [CreateAssetMenu(fileName = "Squad Data", menuName = "Squad System/Squad Data")]
    public class SquadData : ScriptableObject
    {
        [Header("小队成员")]
        [Tooltip("小队中的成员ID列表")]
        [SerializeField] private List<string> memberIds = new List<string>();

        /// <summary>
        /// 获取小队中的成员ID列表（只读）
        /// </summary>
        public IReadOnlyList<string> MemberIds => memberIds;

        /// <summary>
        /// 获取成员数量
        /// </summary>
        public int MemberCount => memberIds.Count;

        /// <summary>
        /// 添加成员到小队
        /// </summary>
        /// <param name="memberId">成员ID</param>
        /// <returns>是否成功添加</returns>
        public bool AddMember(string memberId)
        {
            if (string.IsNullOrEmpty(memberId))
            {
                Debug.LogWarning("[SquadData] 尝试添加空的成员ID");
                return false;
            }

            if (memberIds.Contains(memberId))
            {
                Debug.LogWarning($"[SquadData] 成员ID '{memberId}' 已存在于小队中");
                return false;
            }

            memberIds.Add(memberId);
            Debug.Log($"[SquadData] 添加成员到小队: {memberId}");
            return true;
        }

        /// <summary>
        /// 从小队移除成员
        /// </summary>
        /// <param name="memberId">成员ID</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveMember(string memberId)
        {
            if (string.IsNullOrEmpty(memberId))
            {
                Debug.LogWarning("[SquadData] 尝试移除空的成员ID");
                return false;
            }

            bool removed = memberIds.Remove(memberId);
            if (removed)
            {
                Debug.Log($"[SquadData] 从小队移除成员: {memberId}");
            }
            else
            {
                Debug.LogWarning($"[SquadData] 成员ID '{memberId}' 不存在于小队中");
            }

            return removed;
        }

        /// <summary>
        /// 检查成员是否在小队中
        /// </summary>
        /// <param name="memberId">成员ID</param>
        /// <returns>是否在小队中</returns>
        public bool HasMember(string memberId)
        {
            return memberIds.Contains(memberId);
        }

        /// <summary>
        /// 清空小队
        /// </summary>
        public void ClearSquad()
        {
            memberIds.Clear();
            Debug.Log("[SquadData] 清空小队");
        }

        /// <summary>
        /// 设置小队成员（替换现有成员）
        /// </summary>
        /// <param name="newMemberIds">新的成员ID列表</param>
        public void SetMembers(List<string> newMemberIds)
        {
            memberIds.Clear();
            if (newMemberIds != null)
            {
                memberIds.AddRange(newMemberIds);
            }
            Debug.Log($"[SquadData] 设置小队成员，共 {memberIds.Count} 个成员");
        }
    }
}

