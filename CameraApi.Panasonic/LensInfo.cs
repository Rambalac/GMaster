namespace CameraApi.Panasonic
{
    public class LensInfo
    {
        public int ClosedAperture { get; set; } = int.MaxValue;

        public bool HasPowerZoom { get; set; }

        public int MaxZoom { get; set; }

        public int MinZoom { get; set; }

        public int OpenedAperture { get; set; }

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

            return Equals((LensInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ClosedAperture;
                hashCode = (hashCode * 397) ^ MaxZoom;
                hashCode = (hashCode * 397) ^ MinZoom;
                hashCode = (hashCode * 397) ^ OpenedAperture;
                hashCode = (hashCode * 397) ^ HasPowerZoom.GetHashCode();
                return hashCode;
            }
        }

        protected bool Equals(LensInfo other)
        {
            return ClosedAperture == other.ClosedAperture && MaxZoom == other.MaxZoom && MinZoom == other.MinZoom && OpenedAperture == other.OpenedAperture && HasPowerZoom == other.HasPowerZoom;
        }
    }
}