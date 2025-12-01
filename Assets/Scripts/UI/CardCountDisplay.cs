using UnityEngine;
using TMPro;
using System.Collections;
using CardSystemManager = CardSystem.CardSystem;

namespace UI
{
    /// <summary>
    /// 卡牌数量显示组件
    /// 同时显示卡组、牌堆和弃牌堆的卡牌数量
    /// 通过挂载方式引用CardSystem和三个TextMeshProUGUI组件
    /// </summary>
    public class CardCountDisplay : MonoBehaviour
    {
        [Header("系统引用")]
        [Tooltip("卡牌系统（用于读取卡牌数量，必须设置）")]
        [SerializeField] private CardSystemManager cardSystem;

        [Header("UI组件")]
        [Tooltip("卡组数量文本（TextMeshProUGUI组件，用于显示卡组数量，可选）")]
        [SerializeField] private TextMeshProUGUI deckCountText;

        [Tooltip("牌堆数量文本（TextMeshProUGUI组件，用于显示牌堆数量，可选）")]
        [SerializeField] private TextMeshProUGUI drawPileCountText;

        [Tooltip("弃牌堆数量文本（TextMeshProUGUI组件，用于显示弃牌堆数量，可选）")]
        [SerializeField] private TextMeshProUGUI discardPileCountText;

        [Header("更新设置")]
        [Tooltip("更新间隔（秒），用于定期检查卡牌数量变化")]
        [SerializeField] private float updateInterval = 0.1f;

        /// <summary>
        /// 上次记录的卡组数量（用于检测变化）
        /// </summary>
        private int lastDeckCount = -1;

        /// <summary>
        /// 上次记录的牌堆数量（用于检测变化）
        /// </summary>
        private int lastDrawPileCount = -1;

        /// <summary>
        /// 上次记录的弃牌堆数量（用于检测变化）
        /// </summary>
        private int lastDiscardPileCount = -1;

        /// <summary>
        /// 更新协程
        /// </summary>
        private Coroutine updateCoroutine;

        private void Awake()
        {
            // 如果没有设置，尝试自动查找
            if (cardSystem == null)
            {
                cardSystem = FindObjectOfType<CardSystemManager>();
            }

            // 如果没有设置文本组件，尝试自动查找（查找所有子对象中的TextMeshProUGUI）
            if (deckCountText == null)
            {
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) deckCountText = texts[0];
            }

            if (drawPileCountText == null)
            {
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 1) drawPileCountText = texts[1];
            }

            if (discardPileCountText == null)
            {
                TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 2) discardPileCountText = texts[2];
            }

            // 验证引用
            if (cardSystem == null)
            {
                Debug.LogError("[CardCountDisplay] CardSystem未设置，无法显示卡牌数量");
            }
        }

        private void Start()
        {
            // 初始化显示
            UpdateCardCountDisplay();
        }

        private void OnEnable()
        {
            // 开始定期更新
            if (updateCoroutine == null)
            {
                updateCoroutine = StartCoroutine(UpdateCardCountCoroutine());
            }

            // 立即更新一次显示
            UpdateCardCountDisplay();
        }

        private void OnDisable()
        {
            // 停止定期更新
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }
        }

        /// <summary>
        /// 定期更新卡牌数量的协程
        /// </summary>
        private IEnumerator UpdateCardCountCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateInterval);
                UpdateCardCountDisplay();
            }
        }

        /// <summary>
        /// 更新卡牌数量显示
        /// </summary>
        private void UpdateCardCountDisplay()
        {
            if (cardSystem == null)
            {
                // 如果CardSystem为空，将所有文本设置为0
                if (deckCountText != null) deckCountText.text = "0";
                if (drawPileCountText != null) drawPileCountText.text = "0";
                if (discardPileCountText != null) discardPileCountText.text = "0";
                return;
            }

            // 更新卡组数量
            if (deckCountText != null)
            {
                int currentDeckCount = cardSystem.Deck != null ? cardSystem.Deck.Count : 0;
                if (currentDeckCount != lastDeckCount)
                {
                    deckCountText.text = currentDeckCount.ToString();
                    lastDeckCount = currentDeckCount;
                }
            }

            // 更新牌堆数量
            if (drawPileCountText != null)
            {
                int currentDrawPileCount = cardSystem.GetDrawPileCount();
                if (currentDrawPileCount != lastDrawPileCount)
                {
                    drawPileCountText.text = currentDrawPileCount.ToString();
                    lastDrawPileCount = currentDrawPileCount;
                }
            }

            // 更新弃牌堆数量
            if (discardPileCountText != null)
            {
                int currentDiscardPileCount = cardSystem.GetDiscardPileCount();
                if (currentDiscardPileCount != lastDiscardPileCount)
                {
                    discardPileCountText.text = currentDiscardPileCount.ToString();
                    lastDiscardPileCount = currentDiscardPileCount;
                }
            }
        }
    }
}

