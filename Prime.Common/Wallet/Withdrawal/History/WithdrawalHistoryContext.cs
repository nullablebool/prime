using System;
using System.Collections.Generic;
using System.Text;
using Prime.Utility;

namespace Prime.Common
{
    public class WithdrawalHistoryContext : NetworkProviderPrivateContext
    {
        public WithdrawalHistoryContext(IUserContext userContext, ILogger logger = null) : base(userContext, logger)
        {
        }

        public WithdrawalHistoryContext(Asset asset, IUserContext userContext, ILogger logger = null) : base(userContext, logger)
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
