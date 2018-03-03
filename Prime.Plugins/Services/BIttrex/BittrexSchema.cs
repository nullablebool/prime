using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prime.Plugins.Services.Bittrex
{
    internal class BittrexSchema
    {
        #region Base

        internal class BaseResponse<TResult>
        {
            public bool success;
            public string message;
            public TResult result;
        }

        #endregion

        #region Private

        internal class BalancesResponse : BaseResponse<IList<BalanceResponse>> { }

        internal class DepositAddressResponse : BaseResponse<DepositAddressContainerResponse> { }

        internal class UuidResponse : BaseResponse<UuidEntry> { }

        internal class OpenOrdersResponse : BaseResponse<IList<GetOpenOrdersEntry>> { }

        internal class GetOrderResponse : BaseResponse<GetOrderEntry> { }

        internal class GetOrderHistoryResponse : BaseResponse<IList<GetOrderHistoryEntry>> { }

        internal class WithdrawalResponse : BaseResponse<UuidEntry> { }

        internal class UuidEntry
        {
            public string uuid;
        }

        internal class OrderCommonBase
        {
            public string OrderUuid;
            public string Exchange;
            public decimal Quantity;
            public decimal QuantityRemaining;
            public decimal Limit;
            public decimal Price;
            public decimal? PricePerUnit;
            public bool IsConditional;
            public string Condition;
            public string ConditionTarget;
            public bool ImmediateOrCancel;
        }

        internal class GetOpenOrdersEntry : OrderCommonBase
        {
            public string Type => OrderType;
            public string OrderType;

            public string Uuid;
            public decimal CommissionPaid;
            public DateTime? Opened;
            public DateTime? Closed;
            public bool CancelInitiated;
        }

        internal class GetOrderEntry : OrderCommonBase
        {
            public string Type;
            public string OrderType => Type;

            public string AccountId;
            public decimal CommissionPaid;
            public decimal CommissionReserved;
            public decimal CommissionReservedRemaining;
            public bool IsOpen;
            public string Sentinel;
            public DateTime? Opened;
            public DateTime? Closed;
            public bool CancelInitiated;
        }

        internal class GetOrderHistoryEntry : OrderCommonBase
        {
            public string Type => OrderType;
            public string OrderType;
            public DateTime TimeStamp;
            public decimal Commission;
        }

        internal class BalanceResponse
        {
            public string Currency;
            public decimal Balance;
            public decimal Available;
            public decimal Pending;
            public string CryptoAddress;
            public bool Requested;
            public string Uuid;
        }

        internal class DepositAddressContainerResponse
        {
            public string Currency;
            public string Address;
        }



        #endregion

        #region Public

        internal class MarketEntriesResponse : BaseResponse<IList<MarketEntryResponse>> { }

        internal class TickerResponse : BaseResponse<TickerContainerResponse> { }

        internal class OrderBookResponse : BaseResponse<OrderBookDataResponse> { }

        internal class MarketSummariesResponse : BaseResponse<IList<MarketSummaryEntryResponse>> { }

        internal class MarketSummaryEntryResponse
        {
            public string MarketName;
            public decimal High;
            public decimal Low;
            public decimal Volume;
            public decimal Last;
            public decimal BaseVolume;
            public DateTime TimeStamp;
            public decimal Bid;
            public decimal Ask;
            public int OpenBuyOrders;
            public int OpenSellOrders;
            public decimal PrevDay;
            public DateTime Created;
            public object DisplayMarketName;
        }

        internal class OrderBookDataResponse
        {
            public OrderBookEntryResponse[] buy;
            public OrderBookEntryResponse[] sell;
        }

        internal class OrderBookEntryResponse
        {
            public decimal Quantity;
            public decimal Rate;
        }

        internal class TickerContainerResponse
        {
            public decimal Bid;
            public decimal Ask;
            public decimal Last;
        }

        internal class MarketEntryResponse
        {
            public string MarketCurrency;
            public string BaseCurrency;
            public string Litecoin;
            public string Bitcoin;
            public decimal MinTradeSize;
            public string MarketName;
            public bool IsActive;
            public DateTime Created;
        }

        #endregion


        internal class GetWithdrawalHistoryResponse : BaseResponse<Withdrawal[]> { }

        internal class Withdrawal 
        {
            /// <summary>
            /// Guid of the payment
            /// </summary>
            public Guid PaymentUuid { get; set; }
            /// <summary>
            /// Currency of the withdrawal
            /// </summary>
            public string Currency { get; set; }
            /// <summary>
            /// Amount of the withdrawal
            /// </summary>
            public decimal Amount { get; set; }
            /// <summary>
            /// Address the withdrawal is to
            /// </summary>
            public string Address { get; set; }
            /// <summary>
            /// Timestamp when withdrawal was opened
            /// </summary>
            public DateTime Opened { get; set; }
            /// <summary>
            /// Whether the withdrawal is authorized
            /// </summary>
            public bool Authorized { get; set; }
            /// <summary>
            /// Whether there is pending payment
            /// </summary>
            public bool PendingPayment { get; set; }
            /// <summary>
            /// Cost of the transaction
            /// </summary>
            public decimal TxCost { get; set; }
            /// <summary>
            /// Id of the transaction
            /// </summary>
            public string TxId { get; set; }
            /// <summary>
            /// Whether the withdrawal is canceled
            /// </summary>
            public bool Canceled { get; set; }
            /// <summary>
            /// Whether the withdrawal is to an invalid address
            /// </summary>
            public bool InvalidAddress { get; set; }
        }

        internal class GetDepositHistoryResponse : BaseResponse<Deposit[]> { }

        internal class Deposit
        {
                /// <summary>
                /// The id of the deposit
                /// </summary>
                public long Id { get; set; }
                /// <summary>
                /// The amount of the deposit
                /// </summary>
                public decimal Amount { get; set; }
                /// <summary>
                /// The currency of the deposit
                /// </summary>
                public string Currency { get; set; }
                /// <summary>
                /// The current number of confirmations the deposit has
                /// </summary>
                public int Confirmations { get; set; }
                /// <summary>
                /// Timestamp of the last update
                /// </summary>
                public DateTime LastUpdated { get; set; }
                /// <summary>
                /// Transaction id of the deposit
                /// </summary>
                public string TxId { get; set; }
                /// <summary>
                /// The address the deposit is to
                /// </summary>
                public string CryptoAddress { get; set; }
            
        }

        internal class GetCurrenciesResponse : BaseResponse<CurrencyItem[]> { }

        internal class CurrencyItem
        {
            /// <summary>
            /// The abbreviation of the currency
            /// </summary>
            public string Currency { get; set; }
            /// <summary>
            /// The full name of the currency
            /// </summary>
            public string CurrencyLong { get; set; }
            /// <summary>
            /// The minimum number of confirmations before a deposit is added to a account
            /// </summary>
            public int MinConfirmation { get; set; }
            /// <summary>
            /// The transaction fee for a currency
            /// </summary>
            public decimal TxFee { get; set; }
            /// <summary>
            /// Whether the currency is currently active
            /// </summary>
            public bool IsActive { get; set; }
            /// <summary>
            /// The base coin type
            /// </summary>
            public string CoinType { get; set; }
            /// <summary>
            /// The base address
            /// </summary>
            public string BaseAddress { get; set; }
            /// <summary>
            /// Additional information about the state of this currency
            /// </summary>
            public string Notice { get; set; }
        }

    }
}
