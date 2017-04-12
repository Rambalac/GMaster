namespace GMaster.Camera
{
    using Rssdp;

    public class DeviceInfo
    {
        internal DeviceInfo(SsdpRootDevice dev, string usn)
        {
            Usn = usn;
            Uuid = dev.Uuid;
            Host = dev.Location.Host;
            FriendlyName = dev.FriendlyName;
            ModelName = dev.ModelNumber;
        }

        public string FriendlyName { get; }

        public string Host { get; }

        public string ModelName { get; }

        public string Usn { get; }

        public string Uuid { get; }
    }
}