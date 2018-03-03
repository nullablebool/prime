using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Prime.Common;
using Prime.Utility;
using Prime.Common.Reporting;
using System.Collections.Concurrent;

namespace Prime.Plugins.Services.Cex
{
    // https://cex.io/rest-api#public
    /// <author email="yasko.alexander@gmail.com">Alexander Yasko</author>
    public partial class CexProvider : IPrivateTradeHistoryProvider, IPublicPricingBulkProvider
    {
        public async Task<TradeOrders> GetPrivateTradeHistoryAsync(TradeHistoryContext context)
        {
            if (context.AssetPair != null) return await GetPrivateTradeHistorySingleAsync(context).ConfigureAwait(false);

            var api = ApiProvider.GetApi(context);

            var validTradePairsStrategy = new ProbableTradePairsDiscovery<CexProvider>(this);

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

            var r = await api.GetTradeHistoryAsync(context.AssetPair.Asset1.ToRemoteCode(this), context.AssetPair.Asset2.ToRemoteCode(this)).ConfigureAwait(false);

            CheckResponseErrors(r);

            var orders = new TradeOrders(Network);

            foreach (var order in r)
            {
                orders.Add(new TradeOrder(order.Id, Network, context.AssetPair, order.Type == "buy" ? TradeOrderType.LimitBuy : TradeOrderType.LimitSell, order.Totals.CreditDebitBalanceCurrency)
                {
                    Quantity = order.Totals.CreditDebitBalanceSymbol,
                    Closed = order.Time,
                    CommissionPaid = new Money(order.Totals.CreditDebitBalanceFee, Assets.I.Get(order.Symbol2, this)),
                    PricePerUnit = new Money(order.Price, context.AssetPair.Asset2)
                });
            }

            return orders;
        }
        public async Task<MarketPrices> GetPricingBulkAsync(NetworkProviderContext context)
        {
            return await GetPricesAsync(new PublicPricesContext(context.L)).ConfigureAwait(false);
        }
    }
}
