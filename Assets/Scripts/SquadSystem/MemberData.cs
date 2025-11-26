using System.Collections.Generic;
using UnityEngine;

namespace SquadSystem
{
    /// <summary>
    /// 成员数据
    /// ScriptableObject，定义单个成员的配置信息
    /// </summary>
    [CreateAssetMenu(fileName = "Member Data", menuName = "Squad System/Member Data")]
    public class MemberData : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("成员ID（唯一标识符）")]
        [SerializeField] private string memberId;

        [Tooltip("成员名称（用于UI显示）")]
        [SerializeField] private string memberName;

        [Header("经济")]
        [Tooltip("每次战斗的雇佣金")]
        [SerializeField] private int hireCost = 0;

        [Header("成员实体")]
        [Tooltip("成员Prefab（必须包含 MemberComponent 组件）")]
        [SerializeField] private GameObject memberPrefab;

        [Header("能力")]
        [Tooltip("成员能力列表")]
        [SerializeField] private List<MemberAbility> abilities = new List<MemberAbility>();

        /// <summary>
        /// 成员ID（唯一标识符）
        /// </summary>
        public string MemberId => memberId;

        /// <summary>
        /// 成员名称
        /// </summary>
        public string MemberName => memberName;

        /// <summary>
        /// 每次战斗的雇佣金
        /// </summary>
        public int HireCost => hireCost;

        /// <summary>
        /// 成员Prefab
        /// </summary>
        public GameObject MemberPrefab => memberPrefab;

        /// <summary>
        /// 成员能力列表（只读）
        /// </summary>
        public IReadOnlyList<MemberAbility> Abilities => abilities;

        /// <summary>
        /// 验证数据有效性
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(memberId))
            {
                Debug.LogError($"[MemberData] {name} 的成员ID为空");
                return false;
            }

            if (memberPrefab == null)
            {
                Debug.LogError($"[MemberData] {name} 的成员Prefab未设置");
                return false;
            }

            if (memberPrefab.GetComponent<MemberComponent>() == null)
            {
                Debug.LogError($"[MemberData] {name} 的成员Prefab缺少 MemberComponent 组件");
                return false;
            }

            return true;
        }
    }
}

