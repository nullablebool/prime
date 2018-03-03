﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Prime.Plugins.Services.Kraken
{
    internal class KrakenSchema
    {
        internal class ErrorResponse
        {
            public string[] error;
        }

        //internal class BaseResponse<TResultKey, TResultObject> : ErrorResponse
        //{
        //    public Dictionary<TResultKey, TResultObject> result;
        //}

        internal class BaseResponse<TObject> : ErrorResponse
        {
            public TObject result;
        }

        internal class BalancesResponse : BaseResponse<Dictionary<string, decimal>> { }

        internal class AssetsResponse : BaseResponse<Dictionary<string, Asset>> { }
        internal class AssetPairsResponse : BaseResponse<Dictionary<string, AssetPairResponse>> { }

        internal class TickersInformationResponse : BaseResponse<Dictionary<string, TickerInformationResponse>> { }

        internal class DepositMethodsResponse : BaseResponse<IList<DepositMethodResponse>> { }

        internal class DepositAddressesResponse : BaseResponse<IList<DepositAddressResponse>> { }

        internal class OrderBookResponse : BaseResponse<Dictionary<string, OrderBookRecordResponse>> { }

        internal class TimeResponse : BaseResponse<TimeDataResponse> { }

        internal class LedgersInfoResponse : BaseResponse<LedgersInfo> { }

        internal class TradeHistoryResponse : BaseResponse<TradesHistory> { }

        internal class TimeDataResponse
        {
            public long unixtime;
            public string rfc1123;
        }

        internal class OrderBookRecordResponse
        {
            public string[][] asks;
            public string[][] bids;
        }

        internal class DepositAddressResponse
        {
            public string address;
            public long expiretm;

            [JsonProperty("new")]
            public bool new_address;
        }

        internal class OhlcResponse : ErrorResponse
        {
            public OhlcResultResponse result;
        }

        internal class OhlcResultResponse
        {
            public Dictionary<string, OhlcDataRespose[]> pairs;
            public long last;
        }

        internal class OhlcDataRespose
        {
            public decimal time;
            public decimal open;
            public decimal high;
            public decimal low;
            public decimal close;
            public decimal vwap;
            public decimal volume;
            public decimal count;
        }

        internal class AssetPairResponse
        {
            public string altname;
            public string aclass_base;

            [JsonProperty("base")]
            public string base_c;

            public string aclass_quote;
            public string quote;
            public string lot;
            public int pair_decimals;
            public int lot_decimals;
            public int lot_multiplier;

            public object[] leverage_buy;
            public object[] leverage_sell;

            public decimal[][] fees;
            public decimal[][] fees_maker;

            public string fee_volume_currency;

            public int margin_call;
            public int margin_stop;
        }

        internal class TickerInformationResponse
        {
            public decimal[] a;
            public decimal[] b;
            public decimal[] c;
            public decimal[] v;
            public decimal[] p;

            public int[] t;

            public decimal[] l;
            public decimal[] h;

            public decimal o;
        }

        internal class DepositMethodResponse
        {
            public string method;
            public string limit;
            public decimal fee;

            [JsonProperty("gen-address")]
            public bool gen_address;

            [JsonProperty("address-setup-fee")]
            public decimal address_setup_fee;
        }

        internal class LedgersInfo
        {
            public int count;
            public Dictionary<string, LedgerInfo> ledger;
        }

        internal class LedgerInfo
        {
            public string refid;
            public double time;
            public string type;
            public string aclass;
            public string asset;
            public decimal amount;
            public decimal fee;
            public decimal balance;
        }

        internal class TradesHistory
        {
            public int count;
            public Dictionary<string, TradeInfo> trades;
        }

        internal class TradeInfo
        {
            public string ordertxid;
            public string pair;            
            public double time;
            public string type;
            public string ordertype;            
            public decimal price;            
            public decimal cost;
            public decimal fee;
            public decimal vol;
            public decimal margin;
            public string misc;
            public string posstatus;
            public decimal? cprice;
            public decimal? ccost;
            public decimal? cfee;
            public decimal? cvol;
            public decimal? cmargin;
            public decimal? net;
            public string[] trades;
        }

        internal class Asset
        {
            public string altname;
            public string aclass;
            public int decimals;
            public int displaydecimals;
        }

    }
}
