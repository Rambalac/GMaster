namespace GMaster.Core.Network
{
    using System;
    using System.Threading.Tasks;

    public interface IDatagramSocket : IDisposable
    {
        event Action<DatagramSocketMessage> MessageReceived;

        Task Bind(string profile, int liveViewPort);
    }
}