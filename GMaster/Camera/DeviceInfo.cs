namespace GMaster.Camera
{
    using Rssdp;

    public class DeviceInfo
    {
        internal DeviceInfo(SsdpRootDevice dev)
        {
            Udn = dev.Udn;
            Host = dev.Location.Host;
            FriendlyName = dev.FriendlyName;
            ModelName = dev.ModelName;
        }

        public string Udn { get; }

        public string Host { get; }

        public string FriendlyName { get; }

        public object ModelName { get; }
    }
}