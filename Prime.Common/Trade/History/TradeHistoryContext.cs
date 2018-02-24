using Prime.Utility;

namespace Prime.Common
{
    public class TradeHistoryContext : NetworkProviderPrivateContext
    {
        public TradeHistoryContext(AssetPair assetPair, IUserContext userContext, ILogger logger = null) : base(userContext, logger)
        {
            this.AssetPair = assetPair;
        }

        public AssetPair AssetPair { get; set; }
    }
}
