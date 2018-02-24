using Prime.Utility;

namespace Prime.Common
{
    public class WalletAddressContext : NetworkProviderPrivateContext
    {
        public WalletAddressContext(IUserContext userContext, ILogger logger = null) : base(userContext, logger)
        {
            
        }
    }
}