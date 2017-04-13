namespace GMaster.Core.Network
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IHttpClient : IDisposable
    {
        Task<IHttpClientResponse> GetAsync(Uri uri, CancellationToken token);

        Task<string> PostStringAsync(Uri baseUri, string str, CancellationToken token);
    }
}