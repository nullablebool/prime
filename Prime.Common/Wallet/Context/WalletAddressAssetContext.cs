﻿using Prime.Utility;

namespace Prime.Common
{
    public class WalletAddressAssetContext : WalletAddressContext
    {
        public Asset Asset { get; set; }

        public WalletAddressAssetContext(Asset asset, IUserContext userContext, ILogger logger = null) : base(userContext, logger)
        {
            Asset = asset;
        }
    }
}