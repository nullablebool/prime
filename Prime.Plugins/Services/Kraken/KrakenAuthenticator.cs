using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Prime.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prime.Plugins.Services.Kraken
{
    public class KrakenAuthenticator : BaseAuthenticator
    {
        public KrakenAuthenticator(ApiKey apiKey) : base(apiKey, Nonce.SeedValues.UtcTicks())
        {
        }

        public override void RequestModify(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var nonce = GetNonce();

            request.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("nonce", nonce.ToString()) }.Concat(ReadRequestContentAsKvp(request)));

            var hash256Bytes = HashSHA256Raw(nonce + Convert.ToChar(0) + request.Content.ReadAsStringAsync().Result);

            var pathAndHashedContentBytes = FromUtf8(request.RequestUri.AbsolutePath).Concat(hash256Bytes).ToArray();

            var signature = HashHMACSHA512(pathAndHashedContentBytes, FromBase64(ApiKey.Secret));

            request.Headers.Add("API-Key", ApiKey.Key);
            request.Headers.Add("API-Sign", signature);
        }

        private static IEnumerable<KeyValuePair<string, string>> ReadRequestContentAsKvp(HttpRequestMessage request)
        {
            var content = request.Content != null ? request.Content.ReadAsStringAsync().Result : ""; //ought to be async
            return content.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Split('='))
                          .Where(s => s?.Length > 1)
                          .Select(s => new KeyValuePair<string, string>(s[0], s[1]));
        }

    }
}
