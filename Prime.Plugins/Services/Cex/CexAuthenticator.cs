using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Prime.Common;
using System;

namespace Prime.Plugins.Services.Cex
{
    public class CexAuthenticator : BaseAuthenticator
    {
        public CexAuthenticator(ApiKey apiKey) : base(apiKey, Nonce.SeedValues.UtcTicks()) { }

        public override void RequestModify(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var nonce = GetNonce().ToString();
            var message = $"{nonce}{ApiKey.Extra}{ApiKey.Key}";
            var signature = HashHMACSHA256Hex(message, ApiKey.Secret).ToUpper();

            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("key", ApiKey.Key),
                new KeyValuePair<string, string>("signature", signature),
                new KeyValuePair<string, string>("nonce", nonce)
            });
        }
    }
}
