﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Prime.Common
{
    public class TradeOrder
    {
        public TradeOrder(string remoteOrderId, Network network, AssetPair pair, TradeOrderType orderType, decimal price)
        {
            Network = network;
            RemoteOrderId = remoteOrderId.Trim();
            Pair = pair;
            OrderType = orderType;
            Price = new Money(price, pair.Asset2);
        }

        [Bson]
        public List<string> TradeIds { get; private set; } = new List<string>();

        [Bson]
        public Network Network { get; private set; }

        [Bson]
        public string RemoteOrderId {get; private set; }

        [Bson]
        public AssetPair Pair {get; private set; }

        [Bson]
        public TradeOrderType OrderType {get; private set; }

        [Bson]
        public decimal Quantity {get; set; }

        [Bson]
        public decimal QuantityRemaining {get; set; }

        [Bson]
        public Money Limit {get; set; }

        [Bson]
        public Money CommissionPaid {get; set; }

        [Bson]
        public Money Price {get; set; }

        [Bson]
        public Money? PricePerUnit {get; set; }

        [Bson]
        public DateTime? Opened {get; set; }

        [Bson]
        public DateTime? Closed {get; set; }

        [Bson]
        public bool CancelInitiated {get; set; }

        [Bson]
        public bool ImmediateOrCancel {get; set; }

        [Bson]
        public bool IsConditional {get; set; }

        [Bson]
        public string Condition {get; set; }

        [Bson]
        public string ConditionTarget {get; set;}

        [Bson]
        public string FundingSource { get; set; }
        public override string ToString()
        {
            return $"{RemoteOrderId} {Network.Name} {Pair} {Quantity} {Price}";
        }
    }
}
