using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace SpotifyLibrary.ClientHandlers
{
    public class ApiResponse
    {
        public ApiResponse(
            HttpResponseMessage message)
        {
            Headers =
                message.Headers.ToDictionary(z => z.Key,
                    z => z.Value);
            StatusCode = message.StatusCode;
            ContentType = message.Content.Headers.ContentType?.ToString();
            Body = message.Content;
        }

        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

        public HttpStatusCode StatusCode { get; }

        public string? ContentType { get; }

        public HttpContent Body { get; }
    }
}