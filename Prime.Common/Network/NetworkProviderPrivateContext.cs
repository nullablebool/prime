using Prime.Utility;
using System;

namespace Prime.Common
{
    public class NetworkProviderPrivateContext : NetworkProviderContext
    {
        public readonly IUserContext UserContext;

        public NetworkProviderPrivateContext(IUserContext userContext, ILogger logger = null) : base(logger)
        {
            UserContext = userContext;
        }

        public override bool IsPublic => UserContext == null;

        public virtual ApiKey GetKey(INetworkProvider provider)
        {
            if (!IsPrivate) return null;

            if (UserContext == null)
                throw new ArgumentNullException(nameof(UserContext), $"A {nameof(UserContext)} is required for private network providers.");

            var apiKey = UserContext.GetApiKey(provider);
            if (apiKey == null)
                throw new ArgumentNullException(nameof(ApiKey), $"The {nameof(UserContext)} failed to provide an API Key.");

            return apiKey;
        }
    }
}