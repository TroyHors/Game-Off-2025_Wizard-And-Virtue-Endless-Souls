namespace CardSystem
{
    /// <summary>
    /// 卡牌槽位接口
    /// 所有可以放置卡牌的槽位都应该实现此接口
    /// </summary>
    public interface ICardSlot
    {
        /// <summary>
        /// 是否可以接受指定的卡牌
        /// </summary>
        /// <param name="card">卡牌</param>
        /// <returns>是否可以接受</returns>
        bool CanAcceptCard(CardDragHandler card);

        /// <summary>
        /// 放置卡牌到槽位
        /// </summary>
        /// <param name="card">卡牌</param>
        void PlaceCard(CardDragHandler card);

        /// <summary>
        /// 从槽位移除卡牌
        /// </summary>
        /// <param name="card">卡牌</param>
        void RemoveCard(CardDragHandler card);

        /// <summary>
        /// 卡牌开始拖动时调用
        /// </summary>
        /// <param name="card">卡牌</param>
        void OnCardBeginDrag(CardDragHandler card);

        /// <summary>
        /// 卡牌拖动结束时调用
        /// </summary>
        /// <param name="card">卡牌</param>
        void OnCardEndDrag(CardDragHandler card);
    }
}

