using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prime.Common;
using Prime.Plugins.Services.Bittrex;

namespace Prime.Tests.Providers
{
    [TestClass()]
    public class BittrexTests : ProviderDirectTestsBase
    {
        // OHLC data is not provided by API.

        public BittrexTests()
        {
            Provider = Networks.I.Providers.OfType<BittrexProvider>().FirstProvider();
        }

        [TestMethod]
        public override void TestGetPricing()
        {
            var pairs = new List<AssetPair>()
            {
                "BTC_XRP".ToAssetPairRaw(),
                "BTC_LTC".ToAssetPairRaw(),
                "BTC_ETH".ToAssetPairRaw(),
                "BTC_ETC".ToAssetPairRaw(),
            };

            base.TestGetPricing(pairs, false, false);
        }

        [TestMethod]
        public override void TestApiPublic()
        {
            base.TestApiPublic();
        }

        [TestMethod]
        public override void TestApiPrivate()
        {
            base.TestApiPrivate();
        }

        [TestMethod]
        public override void TestGetAddresses()
        {
            var context = new WalletAddressContext(UserContext.Current);

            base.TestGetAddresses(context);
        }

        [TestMethod]
        public override void TestGetAddressesForAsset()
        {
            var context = new WalletAddressAssetContext("BTC".ToAssetRaw(), UserContext.Current);

            base.TestGetAddressesForAsset(context);
        }

        [TestMethod]
        public override void TestPlaceOrderLimit()
        {
            // Reliable test for selling.
            base.TestPlaceOrderLimit("BTC_XRP".ToAssetPairRaw(), false, new Money(3m, Asset.Xrp), new Money(1m, Asset.Btc));

            // Reliable test for buying.
            base.TestPlaceOrderLimit("BTC_XRP".ToAssetPairRaw(), true, new Money(5000m, Asset.Xrp), new Money(0.00000010m, Asset.Btc));
        }

        [TestMethod]
        public override void TestGetTradeOrderStatus()
        {
            base.TestGetTradeOrderStatus("1c92173b-c6a2-4118-9ff0-b78bd775e0a8");
        }

        [TestMethod]
        public override void TestGetAssetPairs()
        {
            var requiredPairs = new AssetPairs()
            {
                "BTC_LTC".ToAssetPairRaw(),
                "BTC_XRP".ToAssetPairRaw(),
                "BTC_ETH".ToAssetPairRaw(),
                "BTC_ETC".ToAssetPairRaw(),
            };

            base.TestGetAssetPairs(requiredPairs);
        }

        [TestMethod]
        public override void TestGetBalances()
        {
            base.TestGetBalances();
        }

        [TestMethod]
        public override void TestGetOrderBook()
        {
            base.TestGetOrderBook("BTC_XRP".ToAssetPairRaw(), false);
        }

        [TestMethod]
        public override void TestPlaceWithdrawal()
        {
            base.TestPlaceWithdrawal(new WalletAddress("13zPXAsFofXXkczMg9bB6x1L9BWK9Yiawr"), new Money(0.00004911m, Asset.Btc));
        }
    }
}