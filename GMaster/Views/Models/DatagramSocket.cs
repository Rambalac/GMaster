namespace GMaster.Views.Models
{
    using System;
    using System.Threading.Tasks;
    using Core.Network;
    using Windows.Networking;

    public class DatagramSocket : IDatagramSocket
    {
        private Windows.Networking.Sockets.DatagramSocket socket;

        public DatagramSocket()
        {
            socket = new Windows.Networking.Sockets.DatagramSocket();
            socket.MessageReceived += Socket_MessageReceived;
        }

        public event Action<DatagramSocketMessage> MessageReceived;

        public async Task Bind(string profile, int liveViewPort)
        {
            await socket.BindEndpointAsync(new HostName(profile), liveViewPort.ToString());
        }

        public void Dispose()
        {
            socket.Dispose();
        }

        private void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
        {
            using (var reader = args.GetDataReader())
            {
                var buf = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(buf);
                MessageReceived?.Invoke(new DatagramSocketMessage(args.RemoteAddress.CanonicalName, buf));
            }
        }
    }
}