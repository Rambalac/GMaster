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
            ModelName = dev.ModelName;
        }

        public string Uuid { get; }

        public string Host { get; }

        public string FriendlyName { get; }

        public string ModelName { get; }
    }
}