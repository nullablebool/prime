using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Prime.Plugins.Services.Cex
{
    internal class CexSchema
    {
        #region Base

        internal class BaseResponse
        {
            public string e;
            public string ok;
        }

        #endregion

        #region Private
        internal class PrivateBaseResponse
        {
            [JsonExtensionData]
            protected IDictionary<string, Newtonsoft.Json.Linq.JToken> _extraData;

            public string Error => _extraData.ContainsKey("error") ? _extraData["error"].ToObject<string>() : null;

            public bool IsFailed => Error != null;
        }

        internal class PrivateBaseResponseList<T> : PrivateBaseResponse, ICollection<T>
        {
            private List<T> _items = new List<T>();
            public int Count => _items.Count;

            public bool IsReadOnly => false;

            public void Add(T item)
            {
                _items.Add(item);
            }

            public void Clear()
            {
                _items.Clear();
            }

            public bool Contains(T item)
            {
                return _items.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _items.CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            public bool Remove(T item)
            {
                return _items.Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _items.GetEnumerator();
            }
        }

        internal class OrderBookResponse
        {
            public long timestamp;
            public decimal[][] bids;
            public decimal[][] asks;
            public string pair;
            public int id;
            public decimal sell_total;
            public decimal buy_total;
        }

        internal class OrderResponseList : PrivateBaseResponseList<OrderResponse> { }

        internal class OrderResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("time")]
            public DateTime Time { get; set; }

            [JsonProperty("lastTxTime")]
            public DateTime LastTxTime { get; set; }

            [JsonProperty("lastTx")]
            public string LastTx { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("symbol1")]
            public string Symbol1 { get; set; }

            [JsonProperty("symbol2")]
            public string Symbol2 { get; set; }

            [JsonProperty("amount")]
            public decimal Amount { get; set; }

            [JsonProperty("price")]
            public decimal Price { get; set; }

            [JsonProperty("remains")]
            public decimal? Remains { get; set; }

            [JsonProperty("tradingFeeMaker")]
            public decimal? TradingFeeMakerPercentage { get; set; }

            [JsonProperty("tradingFeeTaker")]
            public decimal? TradingFeeTakerPercentage { get; set; }

            [JsonProperty("orderId")]
            public string OrderId { get; set; }

            [JsonExtensionData]
            IDictionary<string, Newtonsoft.Json.Linq.JToken> _extraData;

            public TotalFields Totals => TotalFields.ParseJson(_extraData, Symbol1, Symbol2);

            internal class TotalFields
            {
                public decimal CreditDebitBalanceFee { get; private set; }
                public decimal CreditDebitBalanceSymbol { get; private set; }
                public decimal CreditDebitBalanceCurrency { get; private set; }
                public decimal FeeAmountTaker { get; private set; }
                public decimal FeeAmountMaker { get; private set; }
                public decimal TotalAmountTaker { get; private set; }
                public decimal TotalAmountMaker { get; private set; }

                private TotalFields() { }

                internal static TotalFields ParseJson(IDictionary<string, Newtonsoft.Json.Linq.JToken> json, string symbol, string currency)
                {
                    var totals = new TotalFields();

                    if (json == null || !json.Any()) return totals;

                    foreach (var prop in json)
                    {
                        if (!prop.Key.Contains(":")) continue;
                        decimal value = 0;
                        try
                        {
                            value = prop.Value.ToObject<decimal>();
                        }
                        catch
                        {
                            //todo log, for now, safely swallow
                        }
                        if (prop.Key == $"ta:{currency}")
                            totals.TotalAmountMaker = value;
                        else if (prop.Key == $"tta:{currency}")
                            totals.TotalAmountTaker = value;
                        else if (prop.Key == $"fa:{currency}")
                            totals.FeeAmountMaker = value;
                        else if (prop.Key == $"tfa:{currency}")
                            totals.FeeAmountTaker = value;
                        else if (prop.Key == $"a:{symbol}:cds")
                            totals.CreditDebitBalanceSymbol = value;
                        else if (prop.Key == $"a:{currency}:cds")
                            totals.CreditDebitBalanceCurrency = value;
                        else if (prop.Key == $"f:{currency}:cds")
                            totals.CreditDebitBalanceFee = value;
                    }

                    return totals;
                }
            }
        }


        #region Balances
        internal class BalancesResponse
        {
            [JsonProperty("timestamp")]
            public long Timestamp { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("BTC")]
            public Balance Btc { get; set; }

            [JsonProperty("BCH")]
            public Balance Bch { get; set; }

            [JsonProperty("ETH")]
            public Balance Eth { get; set; }

            [JsonProperty("LTC")]
            public Balance Ltc { get; set; }

            [JsonProperty("DASH")]
            public Balance Dash { get; set; }

            [JsonProperty("ZEC")]
            public Balance Zec { get; set; }

            [JsonProperty("USD")]
            public Balance Usd { get; set; }

            [JsonProperty("EUR")]
            public Balance Eur { get; set; }

            [JsonProperty("GBP")]
            public Balance Gbp { get; set; }

            [JsonProperty("RUB")]
            public Balance Rub { get; set; }

            [JsonProperty("GHS")]
            public Balance Ghs { get; set; }
        } 


        internal class BalancesResponseDynamic : PrivateBaseResponse
        {
            [JsonProperty("timestamp")]
            public long Timestamp { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            public IDictionary<string, Balance> Balances => _extraData.ToDictionary(s => s.Key, s =>
            {
                try { return s.Value.ToObject<Balance>(); } catch { }
                return default;
            });
        }

        internal class Balance
        {
            [JsonProperty("available")]
            public decimal? Available { get; set; }

            [JsonProperty("orders")]
            public decimal? Orders { get; set; }
        }

        #endregion

        #endregion

        #region Public

        internal class TickersResponse : BaseResponse
        {
            public TickerResponse[] data;
        }

        internal class TickerResponse
        {
            public long timestamp;
            public string pair;
            public decimal low;
            public decimal high;
            public decimal last;
            public decimal volume;
            public decimal volume30d;
            public decimal bid;
            public decimal ask;
        }

        internal class LatestPricesResponse : BaseResponse
        {
            public LatestPriceResponse[] data;
        }

        internal class LatestPriceResponse
        {
            public string symbol1;
            public string symbol2;
            public decimal lprice;
        }

        #endregion
    }
}
