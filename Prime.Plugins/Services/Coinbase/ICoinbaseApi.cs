using System.Threading.Tasks;
using RestEase;

namespace Prime.Plugins.Services.Coinbase
{
    internal interface ICoinbaseApi
    {
        [Get("/accounts")]
        Task<CoinbaseSchema.AccountsResponse> GetAccountsAsync();

        [Get("/accounts/{account_id}/addresses")]
        Task<CoinbaseSchema.WalletAddressesResponse> GetAddressesAsync([Path("account_id")] string accountId);

        [Get("/accounts/{account_id}/addresses/{address_id}")]
        Task<CoinbaseSchema.WalletAddressResponse> GetAddressAsync([Path("account_id")] string accountId, [Path("address_id")] string addressId);

        [Get("/accounts/{account_id}/addresses")]
        Task<CoinbaseSchema.CreateWalletAddressResponse> CreateAddressAsync([Path("account_id")] string accountId);

        [Get("/accounts/{account_id}/transactions/{transaction_id}")]
        Task<CoinbaseSchema.TransactionResponse> GetTransactionAsync([Path("account_id")] string accountId, [Path("transaction_id")] string transactionId);

        [Get("/accounts/{account_id}/transactions?limit=100")]
        Task<CoinbaseSchema.TransactionListResponse> GetTransactionsAsync([Path("account_id")] string accountId, [RawQueryString] string startingAfter = null);

        [Get("/accounts/{account_id}/buys?limit=100")]
        Task<CoinbaseSchema.BuyListResponse> GetBuysAsync([Path("account_id")] string accountId, [RawQueryString] string startingAfter = null);

        [Get("/accounts/{account_id}/sells?limit=100")]
        Task<CoinbaseSchema.SellListResponse> GetSellsAsync([Path("account_id")] string accountId, [RawQueryString] string startingAfter = null);

        [Get("/accounts/{account_id}/withdrawals?limit=100")]
        Task<CoinbaseSchema.WithdrawListResponse> GetWithdrawalsAsync([Path("account_id")] string accountId, [RawQueryString] string startingAfter = null);

        [Get("/accounts/{account_id}/deposits?limit=100")]
        Task<CoinbaseSchema.DepositListResponse> GetDepositsAsync([Path("account_id")] string accountId, [RawQueryString] string startingAfter = null);

        [Get("/payment-methods")]
        Task<CoinbaseSchema.PaymentMethodListRespose> GetPaymentMethodsAsync([RawQueryString] string startingAfter = null);

        [Get("/prices/{currencyPair}/spot")]
        Task<CoinbaseSchema.SpotPriceResponse> GetLatestPriceAsync([Path] string currencyPair);

        [Get("/time")]
        Task<CoinbaseSchema.TimeResponse> GetCurrentServerTimeAsync();
    }
}