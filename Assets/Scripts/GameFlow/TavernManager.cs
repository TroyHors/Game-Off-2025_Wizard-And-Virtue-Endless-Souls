using System.Collections.Generic;
using UnityEngine;
using CurrencySystem;
using SquadSystem;
using UI;

namespace GameFlow
{
    /// <summary>
    /// 酒馆管理器
    /// 处理酒馆节点的成员展示和购买
    /// </summary>
    public class TavernManager : MonoBehaviour
    {
        [Header("酒馆设置")]
        [Tooltip("酒馆商品数量（随机选择的成员数量）")]
        [SerializeField] private int tavernItemCount = 3;

        [Header("系统引用")]
        [Tooltip("金币系统（如果为空，会自动查找）")]
        [SerializeField] private CoinSystem coinSystem;

        [Tooltip("小队管理器（如果为空，会自动查找）")]
        [SerializeField] private SquadManager squadManager;

        [Tooltip("成员数据注册表（用于根据成员ID获取成员数据，如果为空，会自动查找）")]
        [SerializeField] private MemberDataRegistry memberDataRegistry;

        [Header("UI设置")]
        [Tooltip("酒馆容器（用于放置商品和按钮，如果为空，会自动查找名为'TavernContainer'的子对象）")]
        [SerializeField] private Transform tavernContainer;

        [Tooltip("购买按钮Prefab（用于生成购买按钮）")]
        [SerializeField] private GameObject purchaseButtonPrefab;

        [Tooltip("UI管理器（用于控制酒馆面板显示，如果为空，会自动查找）")]
        [SerializeField] private UIManager uiManager;

        [Tooltip("酒馆流程（用于结束流程，如果为空，会自动查找）")]
        [SerializeField] private TavernNodeFlow tavernNodeFlow;

        /// <summary>
        /// 当前酒馆的商品实例列表（用于清理）
        /// </summary>
        private List<GameObject> currentTavernInstances = new List<GameObject>();

        /// <summary>
        /// 当前酒馆的商品ID列表（用于外部获取）
        /// </summary>
        private List<string> currentTavernItemIds = new List<string>();

        private void Awake()
        {
            // 自动查找系统引用
            if (coinSystem == null)
            {
                coinSystem = FindObjectOfType<CoinSystem>();
            }

            if (squadManager == null)
            {
                squadManager = FindObjectOfType<SquadManager>();
            }

            if (memberDataRegistry == null)
            {
                memberDataRegistry = FindObjectOfType<MemberDataRegistry>();
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }

            if (tavernNodeFlow == null)
            {
                tavernNodeFlow = GetComponent<TavernNodeFlow>();
            }

            if (tavernNodeFlow == null)
            {
                tavernNodeFlow = FindObjectOfType<TavernNodeFlow>();
            }

            // 如果没有设置容器，尝试查找名为'TavernContainer'的子对象
            if (tavernContainer == null)
            {
                tavernContainer = transform.Find("TavernContainer");
            }

            // 如果还是找不到，使用自身作为容器
            if (tavernContainer == null)
            {
                tavernContainer = transform;
                Debug.LogWarning("[TavernManager] 酒馆容器未设置，使用自身作为容器");
            }
        }

        /// <summary>
        /// 初始化酒馆（随机选择成员并生成）
        /// </summary>
        public void InitializeTavern()
        {
            Debug.Log($"[TavernManager] 开始初始化酒馆，商品数量: {tavernItemCount}");

            // 清理之前的商品
            ClearTavern();

            // 确保所有系统引用都已找到
            EnsureSystemReferences();

            // 随机选择商品
            GenerateTavernItems();

            // 显示酒馆面板
            if (uiManager != null)
            {
                uiManager.ShowTavernPanel();
                Debug.Log("[TavernManager] 已显示酒馆面板");
            }
            else
            {
                Debug.LogWarning("[TavernManager] UI管理器未找到，无法显示酒馆面板");
            }
        }

        /// <summary>
        /// 确保所有系统引用都已找到
        /// </summary>
        private void EnsureSystemReferences()
        {
            if (coinSystem == null)
            {
                coinSystem = FindObjectOfType<CoinSystem>();
            }

            if (squadManager == null)
            {
                squadManager = FindObjectOfType<SquadManager>();
            }

            if (memberDataRegistry == null)
            {
                // 尝试从SquadManager获取
                if (squadManager != null)
                {
                    // SquadManager可能有memberDataRegistry字段，但需要检查
                    memberDataRegistry = FindObjectOfType<MemberDataRegistry>();
                }
                if (memberDataRegistry == null)
                {
                    Debug.LogWarning("[TavernManager] 成员数据注册表未找到");
                }
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }

            // 如果没有设置容器，尝试查找名为'TavernContainer'的子对象
            if (tavernContainer == null)
            {
                tavernContainer = transform.Find("TavernContainer");
            }

            // 如果还是找不到，使用自身作为容器
            if (tavernContainer == null)
            {
                tavernContainer = transform;
                Debug.LogWarning("[TavernManager] 酒馆容器未设置，使用自身作为容器");
            }
        }

        /// <summary>
        /// 生成酒馆商品（随机选择成员并生成实例和按钮）
        /// </summary>
        private void GenerateTavernItems()
        {
            Debug.Log($"[TavernManager] 开始生成酒馆商品，数量: {tavernItemCount}");

            if (memberDataRegistry == null)
            {
                Debug.LogError("[TavernManager] 成员数据注册表未找到，无法生成酒馆商品");
                return;
            }

            if (purchaseButtonPrefab == null)
            {
                Debug.LogError("[TavernManager] 购买按钮Prefab未设置，无法生成购买按钮");
                return;
            }

            if (tavernContainer == null)
            {
                Debug.LogError("[TavernManager] 酒馆容器未设置，无法生成商品实例");
                return;
            }

            // 获取所有可用的成员数据
            List<MemberData> allMemberData = memberDataRegistry.GetAllMemberData();
            if (allMemberData == null || allMemberData.Count == 0)
            {
                Debug.Log("[TavernManager] 成员数据注册表为空，无法生成酒馆商品");
                currentTavernItemIds = new List<string>();
                return;
            }

            Debug.Log($"[TavernManager] 从注册表中获取到 {allMemberData.Count} 个可用成员数据");

            // 获取小队中已有的成员ID列表（用于排除）
            HashSet<string> squadMemberIds = new HashSet<string>();
            if (squadManager != null && squadManager.SquadData != null)
            {
                foreach (string memberId in squadManager.SquadData.MemberIds)
                {
                    squadMemberIds.Add(memberId);
                }
                Debug.Log($"[TavernManager] 小队中已有 {squadMemberIds.Count} 个成员，将排除这些成员: {string.Join(", ", squadMemberIds)}");
            }

            // 排除已在小队中的成员
            List<MemberData> availableMemberData = new List<MemberData>();
            foreach (MemberData memberData in allMemberData)
            {
                if (memberData != null && !string.IsNullOrEmpty(memberData.MemberId))
                {
                    if (!squadMemberIds.Contains(memberData.MemberId))
                    {
                        availableMemberData.Add(memberData);
                    }
                }
            }

            Debug.Log($"[TavernManager] 排除已在小队中的成员后，剩余 {availableMemberData.Count} 个可用成员");

            // 如果没有可用成员，记录日志并返回空列表
            if (availableMemberData.Count == 0)
            {
                Debug.Log("[TavernManager] 没有可用的成员（所有成员都已在小队中或注册表为空），酒馆商品列表为空");
                currentTavernItemIds = new List<string>();
                return;
            }

            // 随机选择指定数量的成员ID
            List<string> selectedMemberIds = new List<string>();
            List<MemberData> remainingMemberData = new List<MemberData>(availableMemberData);

            for (int i = 0; i < tavernItemCount && remainingMemberData.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, remainingMemberData.Count);
                MemberData selectedMember = remainingMemberData[randomIndex];
                selectedMemberIds.Add(selectedMember.MemberId);
                remainingMemberData.RemoveAt(randomIndex);
            }

            Debug.Log($"[TavernManager] 随机选择了 {selectedMemberIds.Count} 个成员ID: {string.Join(", ", selectedMemberIds)}");

            // 保存商品ID列表（供外部获取）
            currentTavernItemIds = new List<string>(selectedMemberIds);

            // 为每个选中的成员生成实例和按钮
            foreach (string memberId in selectedMemberIds)
            {
                CreateTavernItem(memberId);
            }

            Debug.Log($"[TavernManager] 生成了 {selectedMemberIds.Count} 个酒馆商品");
        }

        /// <summary>
        /// 创建单个酒馆商品（成员实例和购买按钮）
        /// </summary>
        /// <returns>是否成功创建</returns>
        private bool CreateTavernItem(string memberId)
        {
            if (string.IsNullOrEmpty(memberId))
            {
                Debug.LogError($"[TavernManager] CreateTavernItem: 成员ID为空");
                return false;
            }

            Debug.Log($"[TavernManager] CreateTavernItem 开始处理: {memberId}");

            // 检查 memberDataRegistry
            if (memberDataRegistry == null)
            {
                Debug.LogError($"[TavernManager] CreateTavernItem: memberDataRegistry 为 null");
                return false;
            }

            // 从成员数据注册表获取成员数据
            MemberData memberData = memberDataRegistry.GetMemberData(memberId);
            if (memberData == null)
            {
                Debug.LogWarning($"[TavernManager] CreateTavernItem: 未找到成员ID '{memberId}' 对应的数据");
                return false;
            }

            // 获取成员Prefab
            GameObject memberPrefab = memberData.MemberPrefab;
            if (memberPrefab == null)
            {
                Debug.LogWarning($"[TavernManager] CreateTavernItem: 成员ID '{memberId}' 的Prefab为空");
                return false;
            }

            Debug.Log($"[TavernManager] CreateTavernItem: 找到Prefab '{memberPrefab.name}'");

            // 检查酒馆容器
            if (tavernContainer == null)
            {
                Debug.LogError($"[TavernManager] CreateTavernItem: tavernContainer 为 null");
                return false;
            }

            Debug.Log($"[TavernManager] CreateTavernItem: 酒馆容器 '{tavernContainer.name}', 激活: {tavernContainer.gameObject.activeSelf}");

            // 生成成员实例（使用从注册表获取的Prefab）
            GameObject memberInstance = Instantiate(memberPrefab, tavernContainer);
            if (memberInstance == null)
            {
                Debug.LogError($"[TavernManager] CreateTavernItem: Instantiate 返回 null");
                return false;
            }

            memberInstance.name = $"TavernMember_{memberId}";
            memberInstance.SetActive(true); // 确保实例是激活的

            Debug.Log($"[TavernManager] CreateTavernItem: 成员实例已创建 '{memberInstance.name}', 激活: {memberInstance.activeSelf}, 父对象: {(memberInstance.transform.parent != null ? memberInstance.transform.parent.name : "null")}");

            currentTavernInstances.Add(memberInstance);
            Debug.Log($"[TavernManager] CreateTavernItem: 已添加到实例列表，当前数量: {currentTavernInstances.Count}");

            // 检查购买按钮Prefab
            if (purchaseButtonPrefab == null)
            {
                Debug.LogError($"[TavernManager] CreateTavernItem: purchaseButtonPrefab 为 null");
                return false;
            }

            // 生成购买按钮（作为成员的子对象）
            GameObject buttonInstance = Instantiate(purchaseButtonPrefab, memberInstance.transform);
            if (buttonInstance == null)
            {
                Debug.LogError($"[TavernManager] CreateTavernItem: 按钮实例化失败");
                return false;
            }

            buttonInstance.name = $"PurchaseButton_{memberId}";
            buttonInstance.SetActive(true); // 确保按钮是激活的

            Debug.Log($"[TavernManager] CreateTavernItem: 按钮实例已创建 '{buttonInstance.name}', 激活: {buttonInstance.activeSelf}");

            // 初始化购买按钮（使用默认价格，从成员数据获取）
            PurchaseButton purchaseButton = buttonInstance.GetComponent<PurchaseButton>();
            if (purchaseButton != null)
            {
                purchaseButton.Initialize(memberId, PurchaseButton.ItemType.Member);
                // 订阅购买成功事件，购买后销毁实例和按钮
                purchaseButton.OnPurchaseSuccess.AddListener((string purchasedMemberId) =>
                {
                    if (purchasedMemberId == memberId)
                    {
                        Destroy(memberInstance);
                        currentTavernInstances.Remove(memberInstance);
                        currentTavernItemIds.Remove(memberId);
                        Debug.Log($"[TavernManager] 成员 '{purchasedMemberId}' 已被购买，已销毁商品实例");
                    }
                });
                Debug.Log($"[TavernManager] CreateTavernItem: 购买按钮初始化完成");
            }
            else
            {
                Debug.LogWarning($"[TavernManager] CreateTavernItem: 购买按钮Prefab缺少 PurchaseButton 组件");
            }

            currentTavernInstances.Add(buttonInstance);
            Debug.Log($"[TavernManager] CreateTavernItem: 完成处理 '{memberId}', 最终实例列表数量: {currentTavernInstances.Count}");
            return true;
        }

        /// <summary>
        /// 清理所有酒馆商品
        /// </summary>
        public void ClearTavern()
        {
            foreach (GameObject instance in currentTavernInstances)
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
            currentTavernInstances.Clear();
            currentTavernItemIds.Clear();

            // 隐藏酒馆面板
            if (uiManager != null)
            {
                uiManager.HideTavernPanel();
            }
        }

        /// <summary>
        /// 获取当前酒馆的所有商品ID列表
        /// </summary>
        /// <returns>商品ID列表</returns>
        public List<string> GetTavernItemIds()
        {
            return new List<string>(currentTavernItemIds);
        }

        /// <summary>
        /// 获取指定索引的商品ID
        /// </summary>
        /// <param name="index">商品索引</param>
        /// <returns>商品ID，如果索引无效则返回空字符串</returns>
        public string GetTavernItemId(int index)
        {
            if (index >= 0 && index < currentTavernItemIds.Count)
            {
                return currentTavernItemIds[index];
            }
            return string.Empty;
        }

        /// <summary>
        /// 从商品实例获取成员ID
        /// </summary>
        /// <param name="memberInstance">成员实例</param>
        /// <returns>成员ID，如果无效则返回空字符串</returns>
        public string GetMemberIdFromInstance(GameObject memberInstance)
        {
            if (memberInstance == null)
            {
                return string.Empty;
            }

            // 从实例名称中提取ID（格式：TavernMember_{memberId}）
            string name = memberInstance.name;
            if (name.StartsWith("TavernMember_"))
            {
                return name.Substring("TavernMember_".Length);
            }

            return string.Empty;
        }

        /// <summary>
        /// 设置酒馆容器（供TavernNodeFlow调用）
        /// </summary>
        /// <param name="container">酒馆容器</param>
        public void SetTavernContainer(Transform container)
        {
            tavernContainer = container;
            Debug.Log($"[TavernManager] 酒馆容器已设置: {(container != null ? container.name : "null")}");
        }

        /// <summary>
        /// 完成酒馆并结束流程（供按钮调用）
        /// 玩家通过场景上的按钮调用此方法，确认酒馆后结束酒馆流程
        /// </summary>
        public void FinishTavernAndFlow()
        {
            Debug.Log("[TavernManager] 玩家确认酒馆，准备结束流程");

            // 清理酒馆
            ClearTavern();

            // 确保找到 TavernNodeFlow（可能在运行时才创建）
            if (tavernNodeFlow == null)
            {
                tavernNodeFlow = FindObjectOfType<TavernNodeFlow>();
            }

            // 结束酒馆流程
            if (tavernNodeFlow != null)
            {
                Debug.Log("[TavernManager] 找到 TavernNodeFlow，调用 FinishTavernAndFlow()");
                tavernNodeFlow.FinishTavernAndFlow();
            }
            else
            {
                Debug.LogError("[TavernManager] 未找到 TavernNodeFlow，无法结束流程！请检查场景中是否有 TavernNodeFlow 组件");
            }
        }
    }
}

