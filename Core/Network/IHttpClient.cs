namespace GMaster.Core.Network
{
    using System;
    using System.Threading.Tasks;

    public interface IHttpClient : IDisposable
    {
        Task<IHttpClientResponse> GetAsync(Uri uri);

        Task<string> PostStringAsync(Uri baseUri, string str);
    }
}