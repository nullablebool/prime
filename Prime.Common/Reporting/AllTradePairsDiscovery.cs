using System.Threading.Tasks;

namespace Prime.Common.Reporting
{
    public class AllTradePairsDiscovery<T> : ITradePairsDiscovery where T : IAssetPairsProvider
    {
        T _provider;

        public AllTradePairsDiscovery(T provider)
        {
            _provider = provider;
        }

        public async Task<AssetPairs> GetKnownTradePairs(NetworkProviderPrivateContext context)
        {
            return await _provider.GetAssetPairsAsync(context).ConfigureAwait(false);
        }
    }
}
