using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 手牌槽位管理器
    /// 管理手牌堆中的槽位系统
    /// </summary>
    public class HandSlotManager : MonoBehaviour
    {
        [Header("手牌槽位设置")]
        [Tooltip("手牌槽位容器（所有槽位会作为此GameObject的子对象动态生成）")]
        [SerializeField] private Transform slotContainer;

        [Tooltip("手牌槽位Prefab（必须包含HandSlot组件）")]
        [SerializeField] private GameObject handSlotPrefab;

        [Tooltip("手牌槽位数量（手牌上限）")]
        [SerializeField] private int slotCount = 10;

        [Header("布局设置")]
        [Tooltip("是否自动生成槽位")]
        [SerializeField] private bool autoGenerateSlots = true;

        /// <summary>
        /// 手牌槽位列表
        /// </summary>
        private List<HandSlot> handSlots = new List<HandSlot>();

        /// <summary>
        /// 获取手牌槽位数量
        /// </summary>
        public int SlotCount => handSlots.Count;

        /// <summary>
        /// 获取已占用的槽位数量
        /// </summary>
        public int OccupiedSlotCount
        {
            get
            {
                int count = 0;
                foreach (var slot in handSlots)
                {
                    if (slot.IsOccupied)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// 获取空闲槽位数量
        /// </summary>
        public int AvailableSlotCount => SlotCount - OccupiedSlotCount;

        private void Awake()
        {
            if (autoGenerateSlots)
            {
                GenerateSlots();
            }
        }

        /// <summary>
        /// 生成手牌槽位
        /// </summary>
        public void GenerateSlots()
        {
            if (slotContainer == null)
            {
                Debug.LogWarning("[HandSlotManager] 手牌槽位容器未设置，无法生成槽位");
                return;
            }

            if (handSlotPrefab == null)
            {
                Debug.LogWarning("[HandSlotManager] 手牌槽位Prefab未设置，无法生成槽位");
                return;
            }

            // 清除旧的槽位
            foreach (Transform child in slotContainer)
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.delayCall += () => DestroyImmediate(child.gameObject);
#endif
                }
            }

            handSlots.Clear();

            // 生成槽位
            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotObj = Instantiate(handSlotPrefab, slotContainer);
                slotObj.name = $"HandSlot_{i}";

                HandSlot slot = slotObj.GetComponent<HandSlot>();
                if (slot == null)
                {
                    Debug.LogError("[HandSlotManager] 手牌槽位Prefab缺少HandSlot组件");
                    Destroy(slotObj);
                    continue;
                }

                handSlots.Add(slot);
            }

            Debug.Log($"[HandSlotManager] 生成了 {handSlots.Count} 个手牌槽位");
        }

        /// <summary>
        /// 获取第一个空闲槽位
        /// </summary>
        /// <returns>空闲槽位，如果没有则返回null</returns>
        public HandSlot GetFirstAvailableSlot()
        {
            foreach (var slot in handSlots)
            {
                if (!slot.IsOccupied)
                {
                    return slot;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定索引的槽位
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>槽位，如果索引无效则返回null</returns>
        public HandSlot GetSlot(int index)
        {
            if (index >= 0 && index < handSlots.Count)
            {
                return handSlots[index];
            }
            return null;
        }

        /// <summary>
        /// 获取所有槽位
        /// </summary>
        /// <returns>槽位列表</returns>
        public List<HandSlot> GetAllSlots()
        {
            return new List<HandSlot>(handSlots);
        }

        /// <summary>
        /// 设置手牌槽位数量
        /// </summary>
        /// <param name="count">数量</param>
        public void SetSlotCount(int count)
        {
            slotCount = Mathf.Max(0, count);
            if (autoGenerateSlots)
            {
                GenerateSlots();
            }
        }

        /// <summary>
        /// 在Inspector中手动刷新槽位（用于运行时调试）
        /// </summary>
        [ContextMenu("刷新手牌槽位")]
        public void RefreshSlots()
        {
            GenerateSlots();
        }
    }
}

