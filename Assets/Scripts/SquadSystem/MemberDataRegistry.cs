using System.Collections.Generic;
using UnityEngine;

namespace SquadSystem
{
    /// <summary>
    /// 成员数据注册表
    /// 管理所有成员数据，类似节点种类管理
    /// </summary>
    [CreateAssetMenu(fileName = "Member Data Registry", menuName = "Squad System/Member Data Registry")]
    public class MemberDataRegistry : ScriptableObject
    {
        [System.Serializable]
        public class MemberDataEntry
        {
            [Tooltip("成员数据")]
            public MemberData memberData;
        }

        [Header("成员数据列表")]
        [Tooltip("所有成员数据")]
        [SerializeField] private List<MemberDataEntry> memberEntries = new List<MemberDataEntry>();

        /// <summary>
        /// 成员数据字典（运行时使用）
        /// </summary>
        private Dictionary<string, MemberData> memberDataDict;

        /// <summary>
        /// 初始化注册表（构建字典）
        /// </summary>
        private void Initialize()
        {
            if (memberDataDict != null)
            {
                return; // 已经初始化
            }

            memberDataDict = new Dictionary<string, MemberData>();
            foreach (var entry in memberEntries)
            {
                if (entry != null && entry.memberData != null)
                {
                    string memberId = entry.memberData.MemberId;
                    if (string.IsNullOrEmpty(memberId))
                    {
                        Debug.LogWarning($"[MemberDataRegistry] 成员数据 {entry.memberData.name} 的成员ID为空，跳过");
                        continue;
                    }

                    if (memberDataDict.ContainsKey(memberId))
                    {
                        Debug.LogWarning($"[MemberDataRegistry] 成员ID '{memberId}' 重复，将使用最后一个数据");
                    }
                    memberDataDict[memberId] = entry.memberData;
                }
            }

            Debug.Log($"[MemberDataRegistry] 初始化完成，注册了 {memberDataDict.Count} 个成员数据");
        }

        /// <summary>
        /// 根据成员ID获取成员数据
        /// </summary>
        /// <param name="memberId">成员ID</param>
        /// <returns>成员数据，如果不存在则返回null</returns>
        public MemberData GetMemberData(string memberId)
        {
            Initialize();

            if (string.IsNullOrEmpty(memberId))
            {
                Debug.LogWarning("[MemberDataRegistry] 成员ID为空");
                return null;
            }

            if (memberDataDict.TryGetValue(memberId, out MemberData memberData))
            {
                return memberData;
            }

            Debug.LogWarning($"[MemberDataRegistry] 未找到成员ID '{memberId}' 对应的成员数据");
            return null;
        }

        /// <summary>
        /// 获取所有成员数据
        /// </summary>
        /// <returns>所有成员数据的列表</returns>
        public List<MemberData> GetAllMemberData()
        {
            Initialize();
            return new List<MemberData>(memberDataDict.Values);
        }

        /// <summary>
        /// 检查成员ID是否存在
        /// </summary>
        /// <param name="memberId">成员ID</param>
        /// <returns>是否存在</returns>
        public bool HasMemberData(string memberId)
        {
            Initialize();
            return memberDataDict.ContainsKey(memberId);
        }
    }
}

