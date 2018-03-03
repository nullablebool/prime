﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Prime.Common;
using Prime.Utility;

namespace Prime.Plugins.Services.Cex
{
    // https://cex.io/rest-api#public
    /// <author email="yasko.alexander@gmail.com">Alexander Yasko</author>
    public partial class CexProvider : IPublicPricingProvider, IAssetPairsProvider, IPublicVolumeProvider, IOrderBookProvider, IBalanceProvider
    {
        private const string CexApiUrl = "https://cex.io/api";

        private static readonly ObjectId IdHash = "prime:cex".GetObjectIdHashCode();
        public ObjectId Id => IdHash;
        public Network Network { get; } = Networks.I.Get("Cexio");
        public bool Disabled => false;
        public int Priority => 100;
        public string AggregatorName => null;
        public string Title => Network.Name;
        public bool IsDirect => true;
        public char? CommonPairSeparator => '/';

        public async Task<OrderBook> GetOrderBookAsync(OrderBookContext context)
        {
            var api = ApiProvider.GetApi(context);

            var pair = context.Pair.ToTicker(this);

            var r = context.MaxRecordsCount == Int32.MaxValue
                ? await api.GetOrderBookAsync(pair).ConfigureAwait(false)
                : await api.GetOrderBookAsync(pair, context.MaxRecordsCount).ConfigureAwait(false);

            var orderBook = new OrderBook(Network, context.Pair);

            foreach (var rAsk in r.asks.Select(ParseOrderBookData))
            {
                orderBook.AddAsk(rAsk.Price, rAsk.Volume, true);
            }

            foreach (var rBid in r.bids.Select(ParseOrderBookData))
            {
                orderBook.AddBid(rBid.Price, rBid.Volume, true);
            }

            return orderBook;
        }

        private (decimal Price, decimal Volume) ParseOrderBookData(decimal[] data)
        {
            return (data[0], data[1]);
        }

        private RestApiClientProvider<ICexApi> ApiProvider { get; }

        private static readonly IRateLimiter Limiter = new NoRateLimits();
        public IRateLimiter RateLimiter => Limiter;

        public CexProvider()
        {
            ApiProvider = new RestApiClientProvider<ICexApi>(CexApiUrl, this, k => new CexAuthenticator(k).GetRequestModifierAsync);
        }

        public ApiConfiguration GetApiConfiguration => ApiConfiguration.Standard3;
        public async Task<bool> TestPrivateApiAsync(ApiPrivateTestContext context)
        {
            var api = ApiProvider.GetApi(context);
            var r = await api.GetAccountBalancesAsync().ConfigureAwait(false); 
            return !r.IsFailed;
        }
        public async Task<bool> TestPublicApiAsync(NetworkProviderContext context)
        {
            var r = await GetAssetPairsAsync(context).ConfigureAwait(false);

            return r.Count > 0;
        }

        public IAssetCodeConverter GetAssetCodeConverter()
        {
            return null;
        }

        public async Task<AssetPairs> GetAssetPairsAsync(NetworkProviderContext context)
        {
            var api = ApiProvider.GetApi(context);
            var r = await api.GetTickersAsync().ConfigureAwait(false);

            var pairs = new AssetPairs();

            foreach (var rTicker in r.data)
            {
                pairs.Add(rTicker.pair.ToAssetPair(this, ':'));
            }

            return pairs;
        }

        private static readonly PricingFeatures StaticPricingFeatures = new PricingFeatures()
        {
            Bulk = new PricingBulkFeatures() { CanReturnAll = true },
            Single = new PricingSingleFeatures()
        };

        public PricingFeatures PricingFeatures => StaticPricingFeatures;

        public async Task<MarketPrices> GetPricingAsync(PublicPricesContext context)
        {
            if (context.ForSingleMethod)
                return await GetPriceAsync(context).ConfigureAwait(false);

            return await GetPricesAsync(context).ConfigureAwait(false);
        }

        public async Task<MarketPrices> GetPriceAsync(PublicPricesContext context)
        {
            var api = ApiProvider.GetApi(context);
            var pairCode = context.Pair.ToTicker(this, '/');

            var r = await api.GetLastPriceAsync(pairCode).ConfigureAwait(false);

            return new MarketPrices(new MarketPrice(Network, context.Pair, r.lprice));
        }

        public async Task<MarketPrices> GetPricesAsync(PublicPricesContext context)
        {
            var api = ApiProvider.GetApi(context);
            var r = await api.GetLastPricesAsync().ConfigureAwait(false);
            CheckResponseError(r);

            var dictPairsPrices = r.data.ToDictionary(x => new AssetPair(x.symbol1, x.symbol2, this), x => x);
            var pairsQueryable = context.IsRequestAll ? dictPairsPrices.Keys.ToList() : context.Pairs;

            var prices = new MarketPrices();

            foreach (var pair in pairsQueryable)
            {
                if (!dictPairsPrices.TryGetValue(pair, out var rPair))
                {
                    prices.MissedPairs.Add(pair);
                    continue;
                }

                prices.Add(new MarketPrice(Network, pair, rPair.lprice));
            }

            return prices;
        }

        private void CheckResponseError(CexSchema.BaseResponse response)
        {
            if (!response.ok.Equals("ok", StringComparison.OrdinalIgnoreCase))
                throw new ApiResponseException($"Error occurred in provider - {response.ok}", this);
        }

        private void CheckResponseErrors(CexSchema.PrivateBaseResponse response)
        {
            if (response.IsFailed)
                throw new ApiResponseException($"Error occurred in provider. {response.Error}", this);
        }

        public async Task<PublicVolumeResponse> GetVolumeAsync(PublicVolumesContext context)
        {
            var api = ApiProvider.GetApi(context);

            var pairCode = context.Pair.ToTicker(this);

            var r = await api.GetTickerAsync(pairCode).ConfigureAwait(false);

            return new PublicVolumeResponse(Network, context.Pair, r.volume);
        }

        public async Task<PublicVolumeResponse> GetVolumesAsync(PublicVolumesContext context)
        {
            var api = ApiProvider.GetApi(context);
            var r = await api.GetTickersAsync().ConfigureAwait(false);

            CheckResponseError(r);

            var rPairsDict = r.data.ToDictionary(x => x.pair.ToAssetPair(this, ':'), x => x);

            var pairsQueryable = context.IsRequestAll ? rPairsDict.Keys.ToList() : context.Pairs;

            var missing = new List<AssetPair>();
            var volumes = new List<NetworkPairVolume>();

            foreach (var pair in pairsQueryable)
            {
                if (!rPairsDict.TryGetValue(pair, out var ticker))
                {
                    missing.Add(pair);
                    continue;
                }

                volumes.Add(new NetworkPairVolume(Network, pair, ticker.volume));
            }

            return new PublicVolumeResponse(Network, volumes, missing);
        }

        public async Task<PublicVolumeResponse> GetPublicVolumeAsync(PublicVolumesContext context)
        {
            if (context.ForSingleMethod)
                return await GetVolumeAsync(context).ConfigureAwait(false);

            return await GetVolumesAsync(context).ConfigureAwait(false);
        }

        public async Task<BalanceResults> GetBalancesAsync(NetworkProviderPrivateContext context)
        {
            var api = ApiProvider.GetApi(context);

            var r = await api.GetAccountBalancesAsync().ConfigureAwait(false);

            CheckResponseErrors(r);

            var balances = new BalanceResults(this);

            foreach (var rBalance in r.Balances)
            {
                var asset = rBalance.Key.ToAsset(this);
                balances.Add(asset, rBalance.Value.Available ?? 0m, rBalance.Value.Orders ?? 0m);
            }

            return balances;
        }

        private static readonly VolumeFeatures StaticVolumeFeatures = new VolumeFeatures()
        {
            Single = new VolumeSingleFeatures() { CanVolumeBase = true },
            Bulk = new VolumeBulkFeatures() { CanReturnAll = true, CanVolumeBase = true }
        };

        public VolumeFeatures VolumeFeatures => StaticVolumeFeatures;
    }
}
