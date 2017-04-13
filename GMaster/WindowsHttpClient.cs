namespace GMaster
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Network;
    using Core.Tools;
    using Windows.Storage.Streams;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;
    using Windows.Web.Http.Headers;

    public class WindowsHttpClient : IHttpClient
    {
        private HttpClient http;

        public WindowsHttpClient()
        {
            var rootFilter = new HttpBaseProtocolFilter();
            rootFilter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            rootFilter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;

            http = new HttpClient(rootFilter);
            http.DefaultRequestHeaders.Accept.Clear();
            http.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/xml"));
            http.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("text/xml"));
            http.DefaultRequestHeaders.UserAgent.Clear();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Apache-HttpClient");
        }

        public void Dispose()
        {
            http.Dispose();
        }

        public async Task<IHttpClientResponse> GetAsync(Uri uri, CancellationToken token)
        {
            do
            {
                try
                {
                    return new WindowsHttpClientResponse(await http.GetAsync(uri).AsTask(token));
                }
                catch (COMException ex) when ((uint)ex.HResult != 0x80072efd)
                {
                    Debug.WriteLine("ComException", "WindowsHttpClient");
                }

                await Task.Delay(100, token);
            }
            while (true);
        }

        public async Task<string> PostStringAsync(Uri baseUri, string str, CancellationToken token)
        {
            do
            {
                try
                {
                    var response = await http.PostAsync(baseUri, new HttpStringContent(str, UnicodeEncoding.Utf8, "application/x-www-form-urlencoded")).AsTask(token);
                    using (var reader = new StreamReader((await response.Content.ReadAsInputStreamAsync()).AsStreamForRead()))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
                catch (COMException)
                {
                    Debug.WriteLine("ComException", "WindowsHttpClient");
                }

                await Task.Delay(100, token);
            }
            while (true);
        }

        public class WindowsHttpClientResponse : IHttpClientResponse
        {
            private readonly HttpResponseMessage httpResponseMessage;

            public WindowsHttpClientResponse(HttpResponseMessage httpResponseMessage)
            {
                this.httpResponseMessage = httpResponseMessage;
            }

            public bool IsSuccessStatusCode => httpResponseMessage.IsSuccessStatusCode;

            public string Code => httpResponseMessage.StatusCode.ToString();

            public void Dispose()
            {
                httpResponseMessage.Dispose();
            }

            public async Task<StreamReader> GetContent()
            {
                return new StreamReader((await httpResponseMessage.Content.ReadAsInputStreamAsync()).AsStreamForRead());
            }
        }
    }
}