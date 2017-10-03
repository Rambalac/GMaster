using Rssdp;

namespace CameraApi.Panasonic
{
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

        public string Host { get; set; }

        public string ModelName { get; }

        public string Usn { get; }

        public string Uuid { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((DeviceInfo)obj);
        }

        public override int GetHashCode()
        {
            return Uuid != null ? Uuid.GetHashCode() : 0;
        }

        protected bool Equals(DeviceInfo other)
        {
            return string.Equals(Uuid, other.Uuid);
        }
    }
}