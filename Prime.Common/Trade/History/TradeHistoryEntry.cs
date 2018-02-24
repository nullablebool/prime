using System;

namespace Prime.Common
{

    public class TradeHistoryEntry
    {
        public string RefId { get; set; }
        public Money Price { get; set; }
        public Money Quantity { get; set; }
        public Money Fee { get; set; }
        public bool IsBuyer { get; set; }
        public bool? IsMaker { get; set; }
        public DateTime Time { get; set; }
        public bool? IsBestMatch { get; set; }

        public TradeHistoryEntry(string id, Money price, Money quantity, Money fee, bool isBuyer, bool? isMaker, DateTime time, bool? isBestMatch)
        {
            this.RefId = id;
            this.Price = price;
            this.Quantity = quantity;
            this.Fee = fee;
            this.IsBuyer = isBuyer;
            this.IsMaker = isMaker;
            this.Time = time;
            this.IsBestMatch = isBestMatch;
        }
    }
}
