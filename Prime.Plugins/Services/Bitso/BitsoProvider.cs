﻿using System;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Prime.Common;
using Prime.Utility;

namespace Prime.Plugins.Services.Bitso
{
    /// <author email="scaruana_prime@outlook.com">Sean Caruana</author>
    // https://bitso.com/api_info
    public class BitsoProvider : IPublicPricingProvider, IAssetPairsProvider, IOrderBookProvider
    {
        private const string BitsoApiVersion = "v3";
        private const string BitsoApiUrl = "https://api.bitso.com/" + BitsoApiVersion + "/";

        private static readonly ObjectId IdHash = "prime:bitso".GetObjectIdHashCode();

        // Rate limits are are based on one minute windows. 
        // If you do more than 300 requests in five minutes, you get locked out for one minute. 
        //https://bitso.com/api_info
        private static readonly IRateLimiter Limiter = new PerMinuteRateLimiter(300, 5);

        private RestApiClientProvider<IBitsoApi> ApiProvider { get; }

        public Network Network { get; } = Networks.I.Get("Bitso");

        public bool Disabled => false;
        public int Priority => 100;
        public string AggregatorName => null;
        public string Title => Network.Name;
        public ObjectId Id => IdHash;
        public IRateLimiter RateLimiter => Limiter;
        public char? CommonPairSeparator => '_';

        public bool IsDirect => true;

        public ApiConfiguration GetApiConfiguration => ApiConfiguration.Standard2;

        public BitsoProvider()
        {
            ApiProvider = new RestApiClientProvider<IBitsoApi>(BitsoApiUrl, this, (k) => null);
        }

        public async Task<bool> TestPublicApiAsync(NetworkProviderContext context)
        {
            var api = ApiProvider.GetApi(context);
            var r = await api.GetTickerAsync("btc_mxn").ConfigureAwait(false);

            return r.success;
        }

        private static readonly PricingFeatures StaticPricingFeatures = new PricingFeatures()
        {
            Single = new PricingSingleFeatures() { CanStatistics = true, CanVolume = true },
            Bulk = new PricingBulkFeatures() { CanStatistics = true, CanVolume = true, CanReturnAll = true }
        };

        public PricingFeatures PricingFeatures => StaticPricingFeatures;

        public async Task<MarketPrices> GetPricingAsync(PublicPricesContext context)
        {
            if (context.ForSingleMethod)
                return await GetPriceAsync(context).ConfigureAwait(false);

            return await GetPricesAsync(context).ConfigureAwait(false);
        }

        public async Task<MarketPrices> GetPricesAsync(PublicPricesContext context)
        {
            var api = ApiProvider.GetApi(context);
            var r = await api.GetTickersAsync().ConfigureAwait(false);
            
            if (r.success)
            {
                var prices = new MarketPrices();
                
                var pairsQueryable = context.IsRequestAll ? r.payload.Select(x => x.book.ToAssetPair(this)) : context.Pairs;

                foreach (var pair in pairsQueryable)
                {
                    var currentTicker = r.payload.FirstOrDefault(x => x.book.ToAssetPair(this).Equals(pair));

                    if (currentTicker == null)
                    {
                        prices.MissedPairs.Add(pair);
                    }
                    else
                    {
                        prices.Add(new MarketPrice(Network, pair, currentTicker.last)
                        {
                            PriceStatistics = new PriceStatistics(Network, pair.Asset2, currentTicker.ask,
                                currentTicker.bid, currentTicker.low, currentTicker.high),
                            Volume = new NetworkPairVolume(Network, pair, currentTicker.volume)
                        });
                    }
                }

                return prices;
            }
            else
            {
                throw new ApiResponseException("Error processing request", this);
            }
        }

        public async Task<MarketPrices> GetPriceAsync(PublicPricesContext context)
        {
            var api = ApiProvider.GetApi(context);
            var pairCode = context.Pair.ToTicker(this).ToLower();
            var r = await api.GetTickerAsync(pairCode).ConfigureAwait(false);

            if (r.success)
            {
                var price = new MarketPrice(Network, context.Pair.Asset1, new Money(r.payload.last, context.Pair.Asset2))
                {
                    PriceStatistics = new PriceStatistics(Network, context.Pair.Asset2, r.payload.ask, r.payload.bid,
                        r.payload.low, r.payload.high),
                    Volume = new NetworkPairVolume(Network, context.Pair, r.payload.volume, null)
                };

                return new MarketPrices(price);
            }
            else
            {
                throw new ApiResponseException("Error processing request", this);
            }
        }

        public async Task<AssetPairs> GetAssetPairsAsync(NetworkProviderContext context)
        {
            var api = ApiProvider.GetApi(context);

            var r = await api.GetTickersAsync().ConfigureAwait(false);

            if (r.success)
            {
                var pairs = new AssetPairs();

                foreach (var rCurrentResponse in r.payload)
                {
                    pairs.Add(rCurrentResponse.book.ToAssetPair(this));
                }

                return pairs;
            }
            else
            {
                throw new ApiResponseException("Error processing request", this);
            }
        }

        public async Task<OrderBook> GetOrderBookAsync(OrderBookContext context)
        {
            var api = ApiProvider.GetApi(context);
            var pairCode = context.Pair.ToTicker(this);

            var r = await api.GetOrderBookAsync(pairCode).ConfigureAwait(false);
            var orderBook = new OrderBook(Network, context.Pair);

            var maxCount = Math.Min(1000, context.MaxRecordsCount);

            var response = r.payload;

            var asks = response.asks.Take(maxCount);
            var bids = response.bids.Take(maxCount);

            foreach (var i in bids)
                orderBook.AddBid(i.price, i.amount, true);

            foreach (var i in asks)
                orderBook.AddAsk(i.price, i.amount, true);

            return orderBook;
        }

        public IAssetCodeConverter GetAssetCodeConverter()
        {
            return null;
        }
    }
}
