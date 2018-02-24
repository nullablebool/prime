using System;
using System.Collections.Generic;
using System.Text;
using Prime.Utility;

namespace Prime.Common
{
    public class DepositHistoryContext : NetworkProviderPrivateContext
    {
        public DepositHistoryContext(IUserContext userContext, ILogger logger = null) : base(userContext, logger)
        {
        }

        public DepositHistoryContext(Asset asset, IUserContext userContext, ILogger logger = null) : base(userContext, logger)
        {
            this.Asset = asset;
        }

        public Asset Asset { get; set; }

        /// <summary>
        /// Optional. Some providers can use it.
        /// </summary>
        public DateTime FromTimeUtc { get; set; }
    }
}
