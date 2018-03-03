using Prime.Common;
using Prime.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Prime.Plugins.Services.Bittrex
{
    public partial class BittrexProvider : IWithdrawalHistoryProvider, IDepositHistoryProvider, IPrivateTradeHistoryProvider, IPublicPricingBulkProvider
    {
        public async Task<MarketPrices> GetPricingBulkAsync(NetworkProviderContext context)
        {
            return await GetPricesAsync(new PublicPricesContext(context.L)).ConfigureAwait(false);
        }

        public async Task<TradeOrders> GetPrivateTradeHistoryAsync(TradeHistoryContext context)
        {
            return await GetOrderHistoryAsync(new PrivatePairContext(context.UserContext)).ConfigureAwait(false);
        }

        public async Task<DepositHistory> GetDepositHistoryAsync(DepositHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);
            var remoteCode = context.Asset == null ? null : context.Asset.ToRemoteCode(this);
            var depositHistoryResponse = await api.GetDepositHistoryAsync(remoteCode).ConfigureAwait(false);
            CheckResponseErrors(depositHistoryResponse);

            var history = new DepositHistory(this);
            if (depositHistoryResponse.result.Length == 0) return history;

            var currencies = await GetCurrencies(context);

            foreach (var deposit in depositHistoryResponse.result)
            {
                var localAsset = Assets.I.Get(deposit.Currency, this);

                history.Add(new DepositHistoryEntry()
                {
                    Price = new Money(deposit.Amount, localAsset),
                    Fee = new Money(0m, localAsset),
                    CreatedTimeUtc = deposit.LastUpdated,
                    Address = deposit.CryptoAddress,
                    TxId = deposit.TxId,
                    DepositStatus = deposit.Confirmations < (currencies.FirstOrDefault(s => s.Currency == deposit.Currency)?.MinConfirmation ?? 6) ? DepositStatus.Awaiting : DepositStatus.Completed
                });
            }

            return history;
        }

        public async Task<WithdrawalHistory> GetWithdrawalHistoryAsync(WithdrawalHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);
            var remoteCode = context.Asset == null ? null : context.Asset.ToRemoteCode(this);
            var withdrawalHistoryResponse = await api.GetWithdrawHistoryAsync(remoteCode).ConfigureAwait(false);
            CheckResponseErrors(withdrawalHistoryResponse);

            var history = new WithdrawalHistory(this);
            if (withdrawalHistoryResponse.result.Length == 0) return history;

            foreach (var withdraw in withdrawalHistoryResponse.result)
            {
                var localAsset = Assets.I.Get(withdraw.Currency, this);

                history.Add(new WithdrawalHistoryEntry()
                {
                    WithdrawalRemoteId = withdraw.PaymentUuid.ToString(),
                    Price = new Money(withdraw.Amount, localAsset),
                    Fee = new Money(withdraw.TxCost, localAsset),
                    CreatedTimeUtc = withdraw.Opened,
                    Address = withdraw.Address,
                    TxId = withdraw.TxId,
                    WithdrawalStatus = ParseWithdrawalStatus(withdraw)
                });
            }

            return history;
        }

        internal async Task<BittrexSchema.CurrencyItem[]> GetCurrencies(NetworkProviderContext context)
        {
            var api = ApiProvider.GetApi(context);
            var getCurrencyResponse = await api.GetCurrenciesAsync().ConfigureAwait(false);
            CheckResponseErrors(getCurrencyResponse);

            return getCurrencyResponse.result;
        }

        private WithdrawalStatus ParseWithdrawalStatus(BittrexSchema.Withdrawal withdrawal)
        {
            //Hack: I just took an educated guess with this logic as there is no documentation to make an interpretation. <nullablebool>

            if (withdrawal.Canceled || withdrawal.InvalidAddress) return WithdrawalStatus.Canceled;

            if (!withdrawal.Authorized) return WithdrawalStatus.Awaiting;

            if (withdrawal.PendingPayment || string.IsNullOrWhiteSpace(withdrawal.TxId)) return WithdrawalStatus.Confirmed;

            return WithdrawalStatus.Completed;
        }
    }
}
