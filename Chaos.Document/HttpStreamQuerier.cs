using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Chaos.Document
{
    public class HttpStreamQuerier : IStreamQuerier
    {
        public Identity Identity { get; set; }
        public async Task<Stream> ReadAsStreamAsync(string uri)
        {
            var requestUri = new Uri(uri);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            EnsureRequestWithIdentity(request);

            using (var handler = new HttpClientHandler() { UseCookies = false })
            {
                using (HttpClient client = new HttpClient(handler))
                {
                    return await client
                        .SendAsync(request)
                        .Result
                        .EnsureSuccessStatusCode()
                        .Content
                        .ReadAsStreamAsync();
                }
            }
        }

        private void EnsureRequestWithIdentity(HttpRequestMessage request)
        {
            Identity.Headers.OfType<string>().ToList().ForEach(name =>
            {
                request.Headers.Add(name, Identity.Headers[name]);
            });
        }
    }
}
