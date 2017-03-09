using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFiDirect;

namespace GMaster
{
    public class WiFiDirectHelper
    {
        private WiFiDirectConnectionListener _listener;
        private WiFiDirectAdvertisementPublisher _publisher;

        public WiFiDirectHelper()
        {
        }

        public WiFiDirectAdvertisementPublisherStatus Start()
        {
            _publisher = new WiFiDirectAdvertisementPublisher();
            _listener = new WiFiDirectConnectionListener();

            _publisher.StatusChanged += OnStatusChanged; ;
            try
            {
                // This can raise an exception if the machine does not support WiFi. Sorry.
                _listener.ConnectionRequested += OnConnectionRequested;
            }
            catch (Exception ex)
            {
                return WiFiDirectAdvertisementPublisherStatus.Aborted;
            }

            _publisher.Advertisement.ListenStateDiscoverability = WiFiDirectAdvertisementListenStateDiscoverability.Normal;
            _publisher.Advertisement.IsAutonomousGroupOwnerEnabled = true;
            _publisher.Start();

            return _publisher.Status;
        }

        public void Stop()
        {
            _publisher.Stop();
            _publisher.StatusChanged -= OnStatusChanged;

            _listener.ConnectionRequested -= OnConnectionRequested;
        }

        private void OnStatusChanged(WiFiDirectAdvertisementPublisher sender, WiFiDirectAdvertisementPublisherStatusChangedEventArgs args)
        {
            
        }

        private void OnConnectionRequested(WiFiDirectConnectionListener sender, WiFiDirectConnectionRequestedEventArgs args)
        {
            
        }

    }
}
