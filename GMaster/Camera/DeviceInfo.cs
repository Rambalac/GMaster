namespace GMaster.Camera
{
    using Rssdp;

    public class DeviceInfo
    {
        internal DeviceInfo(SsdpRootDevice dev)
        {
            Uuid = dev.Uuid;
            Host = dev.Location.Host;
            FriendlyName = dev.FriendlyName;
            ModelName = dev.ModelNumber;
        }

        public string FriendlyName { get; }

        public string Host { get; }

        public string ModelName { get; }

        public string Uuid { get; }
    }
}