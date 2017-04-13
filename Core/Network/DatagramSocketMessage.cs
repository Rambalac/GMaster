namespace GMaster.Core.Network
{
    public class DatagramSocketMessage
    {
        public DatagramSocketMessage(string remoteAddressCanonicalName, byte[] buf)
        {
            RemoteAddress = remoteAddressCanonicalName;
            Data = buf;
        }

        public byte[] Data { get; }

        public string RemoteAddress { get; }
    }
}