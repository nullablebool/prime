﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using plugins;
using Prime.Common;
using Prime.Plugins.Services.Coinbase;

namespace Prime.Tests.Providers
{
    [TestClass]
    public class CoinbaseTests : ProviderDirectTestsBase
    {
        public CoinbaseTests()
        {
            Provider = Networks.I.Providers.OfType<CoinbaseProvider>().FirstProvider();
        }

        [TestMethod]
        public override async Task TestGetAssetPairsAsync()
        {
            RequiredAssetPairs = new AssetPairs()
            {
                "LTC_EUR".ToAssetPairRaw(),
                "LTC_BTC".ToAssetPairRaw(),
                "LTC_USD".ToAssetPairRaw(),
                "ETH_BTC".ToAssetPairRaw(),
                "ETH_USD".ToAssetPairRaw(),
                "BTC_USD".ToAssetPairRaw(),
                "BTC_EUR".ToAssetPairRaw(),
                "BTC_GBP".ToAssetPairRaw(),
            };

            await base.TestGetAssetPairsAsync();
        }
        
        [TestMethod]
        public override async Task TestGetPriceAsync()
        {
            await base.TestGetPriceAsync();
        }

        [TestMethod]
        public override async Task TestGetAssetPricesAsync()
        {
            await base.TestGetAssetPricesAsync();
        }

        [TestMethod]
        public override async Task TestGetPricesAsync()
        {
            await base.TestGetPricesAsync();
        }

        [TestMethod]
        public override async Task TestGetOhlcAsync()
        {
            await base.TestGetOhlcAsync();
        }

        [TestMethod]
        public override async Task TestApiAsync()
        {
            await base.TestApiAsync();
        }

        [TestMethod]
        public override async Task TestGetBalancesAsync()
        {
            await base.TestGetBalancesAsync();
        }

        [TestMethod]
        public override async Task TestGetAddressesForAssetAsync()
        {
            await base.TestGetAddressesForAssetAsync();
        }

        [TestMethod]
        public override async Task TestGetAddressesAsync()
        {
            await base.TestGetAddressesAsync();
        }

        [TestMethod]
        public override async Task TestGetOrderBookAsync()
        {
            OrderBookContext = new OrderBookContext(new AssetPair("BTC".ToAssetRaw(), "USD".ToAssetRaw()));
            await base.TestGetOrderBookAsync();

            OrderBookContext = new OrderBookContext(new AssetPair("BTC".ToAssetRaw(), "USD".ToAssetRaw()), 100);
            await base.TestGetOrderBookAsync();
        }
    }
}
