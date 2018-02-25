using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prime.Common.Reporting
{
    /// <summary>
    /// A strategy to fetch likely tradepairs on a provider. Used for providers that lack the ability to query all trades historically and require a trade pair.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ProbableTradePairsDiscovery<T> : ITradePairsDiscovery where T : IBalanceProvider, IAssetPairsProvider
    {
        T _provider;

        public ProbableTradePairsDiscovery(T provider)
        {
            _provider = provider;
        }

        public async Task<AssetPairs> GetKnownTradePairs(NetworkProviderPrivateContext context)
        {
            var validTradingPairs = new AssetPairs();

            //Let's determine possible trade pairs by looking at the balances
            var assetsWithBalance = await GetAssetsWithBalance(context);

            if (!assetsWithBalance.Any()) return validTradingPairs;

            var prices = await _provider.GetAssetPairsAsync(context).ConfigureAwait(false);

            //We will fetch all the possible quote currencies
            //Then, assume only those with a balance have been traded
            var providerQuotesWithBalance = GetProviderQuotes(prices).Where(s => assetsWithBalance.Contains(s));

            //With our assumed list of traded assets and traded currencies, let's build a list of possible trade pairs
            foreach (var asset in assetsWithBalance)
            {
                //Lets get the available quotes for this asset
                foreach (var quote in providerQuotesWithBalance)
                {
                    if (quote != asset && prices.Any(tp => tp.Equals(new AssetPair(asset, quote))))
                        validTradingPairs.Add(new AssetPair(asset, quote));
                }
            }

            return validTradingPairs;
        }

        private async Task<Asset[]> GetAssetsWithBalance(NetworkProviderPrivateContext context)
        {
            var balances = await _provider.GetBalancesAsync(context).ConfigureAwait(false);
            return balances.Where(s => s.AvailableAndReserved > 0).Select(s => s.Asset).ToArray();
        }

        private static IEnumerable<Asset> GetProviderQuotes(IEnumerable<AssetPair> assetPairs)
        {
            //We will fetch all the possible quote currencies
            return assetPairs.GroupBy(s => s.Asset2).Select(s => s.Key);
        }

    }
}
