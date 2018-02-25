using System;
using System.Text;
using System.Threading.Tasks;

namespace Prime.Common.Reporting
{
    public interface ITradePairsDiscovery
    {
        Task<AssetPairs> GetKnownTradePairs(NetworkProviderPrivateContext context);
    }
}
