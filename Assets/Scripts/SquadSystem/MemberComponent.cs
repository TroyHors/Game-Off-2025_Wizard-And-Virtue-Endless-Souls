using UnityEngine;
using DamageSystem;

namespace SquadSystem
{
    /// <summary>
    /// 成员组件
    /// 挂载在成员Prefab上，管理成员实例
    /// </summary>
    public class MemberComponent : MonoBehaviour
    {
        [Header("成员数据")]
        [Tooltip("成员数据（运行时设置）")]
        [SerializeField] private MemberData memberData;

        /// <summary>
        /// 成员数据
        /// </summary>
        public MemberData MemberData
        {
            get => memberData;
            set => memberData = value;
        }

        /// <summary>
        /// 成员ID
        /// </summary>
        public string MemberId => memberData != null ? memberData.MemberId : string.Empty;

        /// <summary>
        /// 成员名称
        /// </summary>
        public string MemberName => memberData != null ? memberData.MemberName : string.Empty;

        /// <summary>
        /// 初始化成员（战斗开始时调用）
        /// </summary>
        /// <param name="data">成员数据</param>
        public void Initialize(MemberData data)
        {
            memberData = data;
            if (data != null)
            {
                gameObject.name = $"Member_{data.MemberName}";
                Debug.Log($"[MemberComponent] 初始化成员: {data.MemberName} (ID: {data.MemberId})");
            }
        }
    }
}

