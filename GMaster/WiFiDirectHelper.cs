using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFiDirect;
using Windows.Networking.Proximity;
using Microsoft.VisualBasic.CompilerServices;

namespace GMaster
{
    public class WiFiDirectHelper
    {

        public void Start()
        {
            PeerFinder.ConnectionRequested += PeerFinder_ConnectionRequested;
            PeerFinder.Start();
        }

        private void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
           // throw new NotImplementedException();
        }


        public void Stop()
        {
            PeerFinder.ConnectionRequested -= PeerFinder_ConnectionRequested;
            PeerFinder.Stop();
        }
    }
}
