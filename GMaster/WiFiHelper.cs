using System;
using System.Threading.Tasks;
using Windows.Devices.WiFi;

namespace GMaster
{
    public class WiFiHelper
    {
        public void Start()
        {
        }

        public async Task<bool> CheckPermission()
        {
            return await WiFiAdapter.RequestAccessAsync() == WiFiAccessStatus.Allowed;
        }

        public void Stop()
        {
        }
    }
}