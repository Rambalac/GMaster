namespace GMaster.Camera
{
    public class FocusPoint
    {
        public bool Fixed { get; set; }

        public double X1 { get; set; }

        public double X2 { get; set; }

        public double Y1 { get; set; }

        public double Y2 { get; set; }

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

            return Equals((FocusPoint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X1.GetHashCode();
                hashCode = (hashCode * 397) ^ X2.GetHashCode();
                hashCode = (hashCode * 397) ^ Y1.GetHashCode();
                hashCode = (hashCode * 397) ^ Y2.GetHashCode();
                hashCode = (hashCode * 397) ^ Fixed.GetHashCode();
                return hashCode;
            }
        }

        protected bool Equals(FocusPoint other)
        {
            return X1 == other.X1 && X2 == other.X2 && Y1 == other.Y1 && Y2 == other.Y2 && Fixed == other.Fixed;
        }
    }
}