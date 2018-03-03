using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prime.Common;
using System.Text.RegularExpressions;

namespace Prime.Plugins.Services.Coinbase
{
    public partial class CoinbaseProvider : IDepositHistoryProvider, IWithdrawalHistoryProvider, IPrivateTradeHistoryProvider, IPublicPricingBulkProvider
    {
        static Regex parseNextPageId = new Regex("(?<nextId>starting_after=[^&]+)");
        static string Parsenext_uri(string next_uri)
        {
            return parseNextPageId.Match(next_uri ?? "")?.Groups["nextId"]?.Value; ;
        }
        public async Task<MarketPrices> GetPricingBulkAsync(NetworkProviderContext context)
        {
            var assetPairs = await GetAssetPairsAsync(context).ConfigureAwait(false);

            return await GetPricingAsync(new PublicPricesContext(assetPairs.Select(s => s).ToList(), context.L)).ConfigureAwait(false);
        }

        public async Task<TradeOrders> GetPrivateTradeHistoryAsync(TradeHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);
            var accs = await api.GetAccountsAsync().ConfigureAwait(false);
            var tradeOrders = new TradeOrders(Network);

            var accountIds = accs.data.Select(x => new KeyValuePair<string, string>(x.currency, x.id));
            var paymentMethods = new List<CoinbaseSchema.PaymentMethodResponse>();
            try
            {
                paymentMethods = await GetPaymentMethods(context);
            }
            catch { }

            //TODO: Parrallel tasks
            foreach (var kvp in accountIds)
            {
                var nextPageBuys = string.Empty;
                while (true)
                {
                    var rBuys = await api.GetBuysAsync(kvp.Value, nextPageBuys).ConfigureAwait(false);

                    foreach (var t in rBuys.data)
                        if (t.status == "completed")
                            PopulateTradeOrder(tradeOrders, t, paymentMethods);

                    nextPageBuys = Parsenext_uri(rBuys.pagination.next_uri);
                    if (string.IsNullOrEmpty(nextPageBuys)) break;
                }

                var nextPageSells = string.Empty;
                while (true)
                {
                    var rSells = await api.GetSellsAsync(kvp.Value, nextPageSells).ConfigureAwait(false);

                    foreach (var t in rSells.data)
                        if (t.status == "completed")
                            PopulateTradeOrder(tradeOrders, t, paymentMethods);

                    nextPageSells = Parsenext_uri(rSells.pagination.next_uri);
                    if (string.IsNullOrEmpty(nextPageSells)) break;
                }
            }

            return tradeOrders;
        }

        private void PopulateTradeOrder(TradeOrders tradeOrders, CoinbaseSchema.BuyResponse t, List<CoinbaseSchema.PaymentMethodResponse> paymentMethods = null)
        {
            var baseAsset = Assets.I.Get(t.amount.currency, this);
            var quoteAsset = Assets.I.Get(t.total.currency, this);

            var paymentMethod = paymentMethods?.FirstOrDefault(a => a.id == t.payment_method?.id);
            tradeOrders.Add(new TradeOrder(t.id, Network, new AssetPair(baseAsset, quoteAsset), t.resource == "buy" ? TradeOrderType.LimitBuy : TradeOrderType.LimitSell, t.total.amount)
            {
                Quantity = t.amount.amount,
                Closed = t.created_at ?? t.updated_at, //updated_at doesnt reflect close date, so favor created_at
                Opened = t.created_at,
                CommissionPaid = new Money(t.fee.amount, Assets.I.Get(t.fee.currency, this)),
                PricePerUnit = new Money(t.subtotal.amount / t.amount.amount, quoteAsset),
                FundingSource = paymentMethod?.type
            });
        }

        public async Task<DepositHistory> GetDepositHistoryAsync(DepositHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);
            var accs = await api.GetAccountsAsync().ConfigureAwait(false);
            var deposits = new DepositHistory(this);

            var accountIds = accs.data.Select(x => new KeyValuePair<string, string>(x.currency, x.id));

            var transactions = await GetTransactionsAsync(context);
            foreach (var t in transactions)
                if (t.type == CoinbaseSchema.TransactionTypes.SEND && t.status == "completed")
                {
                    deposits.Add(new DepositHistoryEntry()
                    {
                        DepositRemoteId = t.id,
                        CreatedTimeUtc = t.created_at ?? t.updated_at ?? DateTime.MinValue,
                        DepositStatus = t.network?.status == "unconfirmed" ? DepositStatus.Awaiting : DepositStatus.Completed,
                        Price = new Money(t.amount.amount, Assets.I.Get(t.amount.currency, this)),
                        Fee = 0,
                        TxId = t.network?.hash
                    });
                }

            foreach (var kvp in accountIds)
            {
                var nextPage = string.Empty;
                while (true)
                {
                    var r = await api.GetDepositsAsync(kvp.Value).ConfigureAwait(false);

                    foreach (var d in r.data)
                    {
                        var baseAsset = Assets.I.Get(d.amount.currency, this);
                        var feeAsset = Assets.I.Get(d.fee.currency, this);

                        deposits.Add(new DepositHistoryEntry()
                        {
                            CreatedTimeUtc = d.created_at ?? d.updated_at ?? DateTime.MinValue,
                            DepositRemoteId = d.id,
                            Price = new Money(d.amount.amount, baseAsset),
                            Fee = new Money(d.fee.amount, feeAsset),
                            DepositStatus = ParseDepositStatus(d.status, d.payout_at),
                            Address = d.payment_method?.id,
                            TxId = d.transaction?.id
                        });
                    }
                    nextPage = Parsenext_uri(r.pagination.next_uri);
                    if (string.IsNullOrEmpty(nextPage)) break;
                }
            }

            return deposits;
        }

        public async Task<WithdrawalHistory> GetWithdrawalHistoryAsync(WithdrawalHistoryContext context)
        {
            var withdrawals = new WithdrawalHistory(this);
            var api = ApiProvider.GetApi(context);
            var accs = await api.GetAccountsAsync().ConfigureAwait(false);

            var accountIds = accs.data.Select(x => new KeyValuePair<string, string>(x.currency, x.id));

            foreach (var kvp in accountIds)
            {
                var nextPage = string.Empty;
                while (true)
                {
                    var r = await api.GetWithdrawalsAsync(kvp.Value).ConfigureAwait(false);

                    foreach (var t in r.data)
                    {
                        var baseAsset = Assets.I.Get(t.amount.currency, this);
                        var feeAsset = Assets.I.Get(t.fee.currency, this);

                        withdrawals.Add(new WithdrawalHistoryEntry()
                        {
                            CreatedTimeUtc = t.created_at ?? t.updated_at ?? DateTime.MinValue,
                            WithdrawalRemoteId = t.id,
                            Price = new Money(t.amount.amount, baseAsset),
                            Fee = new Money(t.fee.amount, feeAsset),
                            WithdrawalStatus = ParseWithdrawStatus(t.status, t.payout_at),
                            Address = t.payment_method?.id,
                            TxId = t.transaction?.id
                        });
                    }
                    nextPage = Parsenext_uri(r.pagination.next_uri);
                    if (string.IsNullOrEmpty(nextPage)) break;
                }
            }

            return withdrawals;
        }

        //TODO: Create type
        private async Task<List<CoinbaseSchema.PaymentMethodResponse>> GetPaymentMethods(NetworkProviderPrivateContext context)
        {
            var list = new List<CoinbaseSchema.PaymentMethodResponse>();
            var api = ApiProvider.GetApi(context);
            var nextPage = string.Empty;
            while (true)
            {
                var r = await api.GetPaymentMethodsAsync(nextPage).ConfigureAwait(false);
                list.AddRange(r.data);
                nextPage = Parsenext_uri(r.pagination.next_uri);
                if (string.IsNullOrEmpty(nextPage)) break;
            }
            return list;
        }

        private DepositStatus ParseDepositStatus(string status, DateTime? payoutAt)
        {
            switch (status)
            {
                case "created":
                    return DepositStatus.Confirmed;
                case "canceled":
                    return DepositStatus.Canceled;
                case "completed":
                    if (DateTime.UtcNow >= payoutAt)
                        return DepositStatus.Completed;
                    else return DepositStatus.Awaiting;
            }

            throw new ApiResponseException($"The deposit status of '{status}' was not recognized.", this);
        }

        private WithdrawalStatus ParseWithdrawStatus(string status, DateTime? payoutAt)
        {
            switch (status)
            {
                case "created":
                    return WithdrawalStatus.Confirmed;
                case "canceled":
                    return WithdrawalStatus.Canceled;
                case "completed":
                    if (DateTime.UtcNow >= payoutAt)
                        return WithdrawalStatus.Completed;
                    else return WithdrawalStatus.Awaiting;
            }

            throw new ApiResponseException($"The deposit status of '{status}' was not recognized.", this);
        }


        [Obsolete("Use this just for high level information. It does not include fees for trades.")]
        private async Task<List<CoinbaseSchema.TransactionResponse>> GetTransactionsAsync(NetworkProviderPrivateContext context)
        {
            var api = ApiProvider.GetApi(context);
            var accs = await api.GetAccountsAsync().ConfigureAwait(false);
            var result = new List<CoinbaseSchema.TransactionResponse>();

            var accountIds = accs.data.Select(x => new KeyValuePair<string, string>(x.currency, x.id));

            foreach (var kvp in accountIds)
            {
                var nextPage = string.Empty;
                while (true)
                {
                    var r = await api.GetTransactionsAsync(kvp.Value, nextPage).ConfigureAwait(false);
                    result.AddRange(r.data);
                    nextPage = Parsenext_uri(r.pagination.next_uri);
                    if (string.IsNullOrEmpty(nextPage)) break;
                }
            }

            return result;
        }
    }
}