using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Prime.Plugins.Services.Coinbase
{
    internal class CoinbaseSchema
    {
        internal class BaseDocument
        {
            public string id;
            public string name;
            public DateTime? created_at;
            public DateTime? updated_at;
            public string resource;
            public string resource_path;
        }

        internal class PaginationResponse
        {
            public string ending_before;
            public string starting_after;
            public string limit;
            public string order;
            public string previous_uri;
            public string next_uri;
        }

        internal class TimeResponse
        {
            public TimeDataResponse data;
        }

        internal class TimeDataResponse
        {
            public DateTime iso;
            public long epoch;
        }

        internal class SpotPriceResponse
        {
            public PriceDataResponse data;
        }

        internal class PriceDataResponse
        {
            public decimal amount;
            public string currency;
        }

        internal class AccountsResponse : ListBaseResponse<AccountResponse> { }

        internal class AccountResponse : BaseDocument
        {
            public string primary;
            public string type;
            public string currency;
            public PriceDataResponse balance;
            public PriceDataResponse native_balance;
        }

        internal class WalletAddressesResponse : ListBaseResponse<WalletAddressResponse> { }

        internal class WalletAddressResponse : BaseDocument
        {
            public string address;
            public string network;
        }

        internal class CreateWalletAddressResponse : ListBaseResponse<WalletAddressResponse> { }

        #region Transaction

        internal static class TransactionTypes
        {
            public const string SEND = "send";
            public const string REQUEST = "request";
            public const string TRANSFER = "transfer";
            public const string BUY = "buy";
            public const string SELL = "sell";
            public const string FIAT_DEPOSIT = "fiat_deposit";
            public const string FIAT_WITHDRAWAL = "fiat_withdrawal";
            public const string EXCHANGE_DEPOSIT = "exchange_deposit";
            public const string EXCHANGE_WITHDRAWAL = "exchange_withdrawal";
            public const string VAULT_WITHDRAWAL = "vault_withdrawal";

            public static bool IsWithdrawal(string type)
            {
                return type == FIAT_WITHDRAWAL || type == EXCHANGE_WITHDRAWAL || type == VAULT_WITHDRAWAL;
            }

            public static bool IsDeposit(string type)
            {
                return type == FIAT_DEPOSIT || type == EXCHANGE_DEPOSIT;
            }

            public static bool IsTrade(string type)
            {
                return type == BUY || type == SELL; // || type == TRANSFER; //account transfer can be considered a trade
            }
        }
        internal class ListBaseResponse<T>
        {
            public PaginationResponse pagination;
            public T[] data;
        }

        internal class TransactionListResponse : ListBaseResponse<TransactionResponse> { }
        internal class WithdrawListResponse : ListBaseResponse<WithdrawalResponse> { }
        internal class DepositListResponse : ListBaseResponse<DepositResponse> { }
        internal class BuyListResponse : ListBaseResponse<BuyResponse> { }
        internal class SellListResponse : ListBaseResponse<SellResponse> { }
        internal class PaymentMethodListRespose : ListBaseResponse<PaymentMethodResponse> { }

        internal class TransactionBaseResponse : BaseDocument
        {
            public string status;
            public PriceDataResponse amount;
        }

        internal class TransactionResponse : TransactionBaseResponse
        {
            public string type;
            public PriceDataResponse native_amount;
            public string description;
            public ResourceData buy;
            public Details details;
            public ResourceData to;
            public Network network;
        }

        internal class BuyResponse : WithdrawalResponse
        {
            public PriceDataResponse total;
        }

        internal class SellResponse : BuyResponse { }

        internal class WithdrawalResponse : TransactionBaseResponse
        {
            public ResourceData payment_method;
            public ResourceData transaction;
            public PriceDataResponse subtotal;
            public bool committed;
            public bool instant;
            public PriceDataResponse fee;
            public DateTime? payout_at;
        }

        internal class DepositResponse : WithdrawalResponse { }

        internal class PaymentMethodResponse : BaseDocument
        {
            public string type;
            public string currency;
            public bool primary_buy;
            public bool primary_sell;
            public bool allow_buy;
            public bool allow_sell;
            public bool allow_deposit;
            public bool allow_withdraw;
            public bool instant_buy;
            public bool instant_sell;
            public ResourceData fiat_account;
        }

        internal static class PaymentMethodTypes
        {
            public const string ACH_BANK_ACCOUNT = "ach_bank_account";
            public const string SEPA_BANK_ACCOUNT = "sepa_bank_account";
            public const string IDEAL_BANK_ACCOUNT = "ideal_bank_account";
            public const string FIAT_ACCOUNT = "fiat_account";
            public const string BANK_WIRE = "bank_wire";
            public const string CREDIT_CARD = "credit_card";
            public const string SECURE3D_CARD = "secure3d_card";
            public const string EFT_BANK_ACCOUNT = "eft_bank_account";
            public const string INTERAC = "interac";
        }

        internal class Details
        {
            public string title;
            public string subtitle;
        }

        internal class Network
        {
            public string status;
            public string name;
            public string hash;
        }

        internal class ResourceData
        {
            public string id;
            public string resource;
            public string resource_path;
            public string email;
        }
        #endregion

    }
}
