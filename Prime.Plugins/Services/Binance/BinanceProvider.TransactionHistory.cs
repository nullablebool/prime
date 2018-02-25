using Prime.Common;
using Prime.Common.Reporting;
using Prime.Utility;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Prime.Plugins.Services.Binance
{
    public partial class BinanceProvider : IWithdrawalHistoryProvider, IDepositHistoryProvider, IPrivateTradeHistoryProvider
    {
        public async Task<TradeOrders> GetPrivateTradeHistoryAsync(TradeHistoryContext context)
        {
            if (context.AssetPair == null) return await GetPrivateTradeHistorySingleAsync(context).ConfigureAwait(false);

            var api = ApiProvider.GetApi(context);

            var validTradePairsStrategy = new ProbableTradePairsDiscovery<BinanceProvider>(this);

            var validTradePairs = await validTradePairsStrategy.GetKnownTradePairs(context);

            var allTradeRequestTasks = new ConcurrentBag<TradeOrders>();

            await validTradePairs.ForEachAsync(async pair =>
            {
                var tradeOrders = await GetPrivateTradeHistorySingleAsync(new TradeHistoryContext(context.UserContext, pair));
                allTradeRequestTasks.Add(tradeOrders);
            }, 5);

            var aggregatedTradeOrders = new TradeOrders(Network);
            allTradeRequestTasks.SelectMany(s => s).ForEach(s => aggregatedTradeOrders.Add(s));
            return aggregatedTradeOrders;
        }

        private async Task<TradeOrders> GetPrivateTradeHistorySingleAsync(TradeHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);

            var rRaw = await api.GetTradeHistoryAsync(context.AssetPair.ToTicker(this)).ConfigureAwait(false);
            CheckResponseErrors(rRaw);

            var r = rRaw.GetContent();

            var orders = new TradeOrders(Network);

            foreach (var order in r)
            {
                orders.Add(new TradeOrder(order.id.ToString(), Network, context.AssetPair, order.isBuyer ? TradeOrderType.LimitBuy : TradeOrderType.LimitSell, order.price)
                {
                    Quantity = order.qty,
                    Closed = order.time.ToUtcDateTime(),
                    CommissionPaid = new Money(order.commission, Assets.I.Get(order.commissionAsset, this))
                });
            }

            return orders;
        }

        public async Task<List<DepositHistoryEntry>> GetDepositHistoryAsync(DepositHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);
            var remoteCode = context.Asset == null ? null : context.Asset.ToRemoteCode(this);
            var rRaw = await api.GetDepositHistoryAsync(remoteCode).ConfigureAwait(false);
            CheckResponseErrors(rRaw);

            var r = rRaw.GetContent();

            var history = new List<DepositHistoryEntry>();

            foreach (var rDeposit in r.depositList)
            {
                var localAsset = Assets.I.Get(rDeposit.asset, this);

                history.Add(new DepositHistoryEntry()
                {
                    Price = new Money(rDeposit.amount, localAsset),
                    Fee = new Money(0m, localAsset), //TODO: Calculate fees using spec
                    CreatedTimeUtc = rDeposit.insertTime.ToUtcDateTime(),
                    Address = rDeposit.address,
                    TxId = rDeposit.txId,
                    DepositStatus = rDeposit.status == 0 ? DepositStatus.Confirmed : DepositStatus.Completed
                });
            }

            return history;
        }

        public async Task<List<WithdrawalHistoryEntry>> GetWithdrawalHistoryAsync(WithdrawalHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);
            var remoteCode = context.Asset == null ? null : context.Asset.ToRemoteCode(this);
            var rRaw = await api.GetWitdrawHistoryAsync(remoteCode).ConfigureAwait(false);
            CheckResponseErrors(rRaw);

            var r = rRaw.GetContent();

            var history = new List<WithdrawalHistoryEntry>();

            foreach (var rHistory in r.withdrawList)
            {
                var localAsset = Assets.I.Get(rHistory.asset, this);

                history.Add(new WithdrawalHistoryEntry()
                {
                    Price = new Money(rHistory.amount, localAsset),
                    Fee = new Money(0m, localAsset), //TODO: Calculate fees using spec
                    CreatedTimeUtc = rHistory.applyTime.ToUtcDateTime(),
                    Address = rHistory.address,
                    TxId = rHistory.txId,
                    WithdrawalRemoteId = rHistory.id,
                    WithdrawalStatus = ParseWithdrawalStatus(rHistory.status)
                });
            }

            return history;
        }

        private WithdrawalStatus ParseWithdrawalStatus(int statusRaw)
        {
            switch (statusRaw)
            {
                case 0: //email sent
                case 2: //awaiting approval
                    return WithdrawalStatus.Awaiting;
                case 1: //canceled
                case 3: //rejected
                case 5: //failure
                    return WithdrawalStatus.Canceled;
                case 4:
                    return WithdrawalStatus.Confirmed;
                case 6:
                    return WithdrawalStatus.Completed;
                default:
                    throw new ApiResponseException($"A withdrawal status of {statusRaw} from Binance could not be parsed.");
            }
        }
    }
}
