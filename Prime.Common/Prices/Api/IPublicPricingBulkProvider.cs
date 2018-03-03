using System.Threading.Tasks;

namespace Prime.Common
{
    public interface IPublicPricingBulkProvider : IAssetPairsProvider, IPublicPricingProvider
    {
        Task<MarketPrices> GetPricingBulkAsync(NetworkProviderContext context);
    }
}