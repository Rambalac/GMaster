namespace GMaster
{
    using System;
    using System.Threading.Tasks;
    using Windows.Devices.WiFi;

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