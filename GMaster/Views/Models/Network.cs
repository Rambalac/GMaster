namespace GMaster.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.Network;
    using Windows.Networking;
    using Windows.Networking.Connectivity;

    public class Network : INetwork
    {
        public Network()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        public event Action NetworkStatusChanged;

        public IDatagramSocket CreateDatagramSocket()
        {
            return new DatagramSocket();
        }

        public IEnumerable<string> GetHostNames()
        {
            return NetworkInformation.GetHostNames()
                .Where(h => h.Type == HostNameType.Ipv4 || h.Type == HostNameType.Ipv6)
                .Select(h => h.CanonicalName);
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            NetworkStatusChanged?.Invoke();
        }
    }
}