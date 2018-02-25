using Prime.Utility;
using System;

namespace Prime.Common
{
    public class TradeHistoryContext : NetworkProviderPrivateContext
    {
        public TradeHistoryContext(IUserContext userContext, AssetPair assetPair = null, ILogger logger = null) : base(userContext, logger)
        {
            this.AssetPair = assetPair;
        }

        public AssetPair AssetPair { get; set; }
    }
}
