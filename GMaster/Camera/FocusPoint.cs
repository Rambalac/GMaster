namespace GMaster.Camera
{
    public class FocusPoint
    {
        public FocusPoint(double x1, double y1, double x2, double y2, bool fix)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            Fixed = fix;
        }

        public bool Fixed { get; }

        public double X1 { get; }

        public double X2 { get; }

        public double Y1 { get; }

        public double Y2 { get; }

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
            return Equals(X1, other.X1) && Equals(X2, other.X2) && Equals(Y1, other.Y1) && Equals(Y2, other.Y2) && Fixed == other.Fixed;
        }
    }
}