namespace GMaster.Core.Camera
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using LumixData;
    using Network;
    using Tools;

    public class Http : IDisposable
    {
        private readonly Uri baseUri;
        private readonly IHttpClient camcgi;

        public Http(Uri baseUrl, IHttpClient client)
        {
            baseUri = baseUrl;
            camcgi = client;
        }

        public static TResponse ReadResponse<TResponse>(string str)
        {
            var serializer = new XmlSerializer(typeof(TResponse));
            return (TResponse)serializer.Deserialize(new StringReader(str));
        }

        public void Dispose()
        {
            camcgi.Dispose();
        }

        public async Task<TResponse> Get<TResponse>(string path)
                    where TResponse : BaseRequestResult
        {
            var uri = new Uri(baseUri, path);
            using (var response = await camcgi.GetAsync(uri))
            {
                using (var content = await response.GetContent())
                {
                    var str = await content.ReadToEndAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new LumixException($"Request failed with code {response.Code}: {path}\r\n{str}");
                    }

                    try
                    {
                        var product = ReadResponse<TResponse>(str);
                        if (product.Result != "ok")
                        {
                            throw new LumixException(
                                ValueToEnum<LumixError>.Parse(product.Result, LumixError.Unknown),
                                $"Not ok result. Request deserialize failed: {path}\r\n{str}");
                        }

                        return product;
                    }
                    catch (LumixException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new LumixException($"Request deserialize failed: {path}\r\n{str}", ex);
                    }
                }
            }
        }

        public async Task<TResponse> Get<TResponse>(Dictionary<string, string> parameters)
            where TResponse : BaseRequestResult
        {
            return await Get<TResponse>("?" + string.Join("&", parameters.Select(p => p.Key + "=" + p.Value)));
        }

        public async Task<string> GetString(string path)
        {
            var uri = new Uri(baseUri, path);
            using (var response = await camcgi.GetAsync(uri))
            {
                using (var content = await response.GetContent())
                {
                    var str = await content.ReadToEndAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new LumixException($"Request failed with code {response.Code}: {path}\r\n{str}");
                    }

                    return str;
                }
            }
        }

        private static async Task<TResponse> ReadResponse<TResponse>(IHttpClientResponse response)
        {
            using (var content = await response.GetContent())
            {
                var serializer = new XmlSerializer(typeof(TResponse));
                return (TResponse)serializer.Deserialize(content);
            }
        }
    }
}