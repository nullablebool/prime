using Prime.Common;
using Prime.Utility;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Prime.Plugins.Services.Kraken
{
    public partial class KrakenProvider : IWithdrawalHistoryProvider, IDepositHistoryProvider, IPrivateTradeHistoryProvider, IPublicPricingBulkProvider
    {
        public async Task<MarketPrices> GetPricingBulkAsync(NetworkProviderContext context)
        {
            var api = ApiProvider.GetApi(context);
            await PopulateAssetMaps(context).ConfigureAwait(false);
            var pairsCsv = string.Join(",", _tradeablePairsMap.Keys);
            var r = await api.GetTickerInformationAsync(pairsCsv).ConfigureAwait(false);

            CheckResponseErrors(r);

            var prices = new MarketPrices();

            prices.AddRange(r.result.Select(s =>
            {
                var pair = CreateAssetPair(s.Key);
                var ticker = r.result[s.Key];
                return new MarketPrice(Network, pair, ticker.c[0])
                {
                    PriceStatistics = new PriceStatistics(Network, pair.Asset2, ticker.a[0], ticker.b[0], ticker.l[1], ticker.h[1]),
                    Volume = new NetworkPairVolume(Network, pair, ticker.v[1])
                };
            }));


            return prices;
        }

        public async Task<TradeOrders> GetPrivateTradeHistoryAsync(TradeHistoryContext context)
        {
            var api = ApiProvider.GetApi(context);
            var r = await api.GetTradesHistory().ConfigureAwait(false);

            CheckResponseErrors(r);

            var results = new TradeOrders(this.Network);

            if (r.result.count == 0) return results;

            await PopulateAssetMaps(context).ConfigureAwait(false); //TODO: Encapsulate this mapping into converter

            foreach (var trade in r.result.trades)
            {
                var info = trade.Value;
                var assetPair = CreateAssetPair(info.pair);
                results.Add(new TradeOrder(trade.Key, this.Network, assetPair, "buy".Equals(info.type) ? TradeOrderType.LimitBuy : TradeOrderType.LimitSell, info.cost)
                {
                    CommissionPaid = new Money(info.fee, assetPair.Asset2),
                    Closed = info.time.UnixTimestampToDateTime(),
                    Quantity = info.vol,
                    PricePerUnit = new Money(info.price, assetPair.Asset2)
                });
            }

            return results;
        }

        KrakenSchema.LedgersInfo _ledger; //todo: cache
        private async Task<KrakenSchema.LedgersInfo> GetLedger(NetworkProviderPrivateContext context) //todo: support dep and with contexts
        {
            if (_ledger != null) return _ledger;
            var api = ApiProvider.GetApi(context);
            var r = await api.GetLedgerInfo().ConfigureAwait(false);
            CheckResponseErrors(r);
            return _ledger = r.result;
        }

        public async Task<DepositHistory> GetDepositHistoryAsync(DepositHistoryContext context)
        {
            var ledger = await GetLedger(context);

            var history = new DepositHistory(this);
            if (ledger.count == 0) return history;

            foreach (var item in ledger.ledger.Where(s => s.Value.type == "deposit"))
            {
                var deposit = item.Value;
                var localAsset = Assets.I.Get(TranslateKrakenSymbol(deposit.asset), this);

                history.Add(new DepositHistoryEntry()
                {
                    Price = new Money(deposit.amount, localAsset),
                    Fee = new Money(0m, localAsset),
                    CreatedTimeUtc = deposit.time.UnixTimestampToDateTime(),
                    TxId = deposit.refid,
                    DepositRemoteId = item.Key,
                    DepositStatus = DepositStatus.Completed
                });
            }

            return history;
        }

        public async Task<WithdrawalHistory> GetWithdrawalHistoryAsync(WithdrawalHistoryContext context)
        {
            var ledger = await GetLedger(context);

            var history = new WithdrawalHistory(this);
            if (ledger.count == 0) return history;

            foreach (var item in ledger.ledger.Where(s => s.Value.type == "widthdrawal"))
            {
                var withdraw = item.Value;
                var localAsset = Assets.I.Get(TranslateKrakenSymbol(withdraw.asset), this);

                history.Add(new WithdrawalHistoryEntry()
                {
                    WithdrawalRemoteId = item.Key,
                    Price = new Money(withdraw.amount, localAsset),
                    Fee = new Money(withdraw.fee, localAsset),
                    CreatedTimeUtc = withdraw.time.UnixTimestampToDateTime(),
                    TxId = withdraw.refid,
                    WithdrawalStatus = WithdrawalStatus.Completed
                });
            }

            return history;
        }


        static Dictionary<string, string[]> _tradeablePairsMap;
        static Dictionary<string, string> _altNameMap; //todo: cache
        private async Task PopulateAssetMaps(NetworkProviderContext context)
        {
            if (_tradeablePairsMap != null) return; 

            var api = ApiProvider.GetApi(context);
            var assetPairs = await api.GetAssetPairsAsync().ConfigureAwait(false);
            var assets = await api.GetAssetsAsync().ConfigureAwait(false);
            _altNameMap = assets.result.ToDictionary(s => s.Key, s => s.Value.altname.Replace("XBT", "BTC"));
            _tradeablePairsMap = assetPairs.result.ToDictionary(s => s.Key, s => new string[] { TranslateKrakenSymbol(s.Value.base_c), TranslateKrakenSymbol(s.Key.Replace(s.Value.base_c, "")) });
        }

        private AssetPair CreateAssetPair(string krakenConcatenatedPair)
        {
            var pair = _tradeablePairsMap[krakenConcatenatedPair];
            return new AssetPair(TranslateKrakenSymbol(pair[0]), TranslateKrakenSymbol(pair[1]), this);
        }

        private string TranslateKrakenSymbol(string symbol)
        {
            if (_altNameMap.ContainsKey(symbol)) return _altNameMap[symbol];
            return symbol.Replace("XBT", "BTC");
        }

    }
}
