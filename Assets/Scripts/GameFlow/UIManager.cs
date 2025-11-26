using UnityEngine;

namespace GameFlow
{
    /// <summary>
    /// UI管理器
    /// 负责控制UI的显示和隐藏
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI容器")]
        [Tooltip("地图UI容器（进入节点事件时隐藏，返回地图时显示）")]
        [SerializeField] private GameObject mapUIContainer;

        [Tooltip("节点事件UI容器（进入节点事件时显示，返回地图时隐藏）")]
        [SerializeField] private GameObject nodeEventUIContainer;

        [Tooltip("奖励面板UI容器（战斗结束时显示，玩家选择奖励后隐藏）")]
        [SerializeField] private GameObject rewardPanelContainer;

        /// <summary>
        /// 显示地图UI
        /// </summary>
        public void ShowMapUI()
        {
            if (mapUIContainer != null)
            {
                mapUIContainer.SetActive(true);
                Debug.Log("[UIManager] 显示地图UI");
            }

            if (nodeEventUIContainer != null)
            {
                nodeEventUIContainer.SetActive(false);
            }

            if (rewardPanelContainer != null)
            {
                rewardPanelContainer.SetActive(false);
            }
        }

        /// <summary>
        /// 隐藏地图UI
        /// </summary>
        public void HideMapUI()
        {
            if (mapUIContainer != null)
            {
                mapUIContainer.SetActive(false);
                Debug.Log("[UIManager] 隐藏地图UI");
            }

            if (nodeEventUIContainer != null)
            {
                nodeEventUIContainer.SetActive(true);
            }
        }

        /// <summary>
        /// 显示奖励面板
        /// </summary>
        public void ShowRewardPanel()
        {
            if (rewardPanelContainer != null)
            {
                rewardPanelContainer.SetActive(true);
                Debug.Log("[UIManager] 显示奖励面板");
            }
        }

        /// <summary>
        /// 隐藏奖励面板
        /// </summary>
        public void HideRewardPanel()
        {
            if (rewardPanelContainer != null)
            {
                rewardPanelContainer.SetActive(false);
                Debug.Log("[UIManager] 隐藏奖励面板");
            }
        }
    }
}

