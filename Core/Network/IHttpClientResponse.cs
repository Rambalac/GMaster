namespace GMaster.Core.Network
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IHttpClientResponse : IDisposable
    {
        bool IsSuccessStatusCode { get; }

        string Code { get; }

        Task<StreamReader> GetContent();
    }
}