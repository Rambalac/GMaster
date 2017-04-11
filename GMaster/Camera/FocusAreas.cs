namespace GMaster.Camera
{
    using System.Collections.Generic;

    public class FocusAreas
    {
        private static readonly Dictionary<int, CameraPoint> FocusPointShifts = new Dictionary<int, CameraPoint>
        {
            { 13, new CameraPoint(0, 0) },
            { 17, new CameraPoint(0, 125) },
            { 15, new CameraPoint(0, 58) },
            { 10, new CameraPoint(125, 0) }
        };

        private readonly List<Box> boxes;
        private readonly CameraPoint focusPointShift;
        private int hashcode;

        public FocusAreas(int number, CameraPoint size, bool fix)
        {
            Fixed = fix;
            boxes = new List<Box>(number);
            var intaspect = size.X * 10 / size.Y;
            focusPointShift = FocusPointShifts.TryGetValue(intaspect, out var val) ? val : FocusPointShifts[13];
        }

        public IReadOnlyList<Box> Boxes => boxes;

        public bool Fixed { get; }

        public void AddBox(int x1, int y1, int x2, int y2)
        {
            var box = new Box(x1, y1, x2, y2);
            hashcode = (hashcode * 397) ^ box.GetHashCode();
            box.Fix(focusPointShift);
            boxes.Add(box);
        }

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

            return Equals((FocusAreas)obj);
        }

        public override int GetHashCode() => hashcode;

        protected bool Equals(FocusAreas other)
        {
            return hashcode == other.hashcode;
        }

        public class Box
        {
            private int x1;
            private int x2;
            private double xDivider = 1000;
            private int y1;
            private int y2;
            private double yDivider = 1000;

            internal Box(int x1, int y1, int x2, int y2)
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;
            }

            public double X1 => x1 / xDivider;

            public double X2 => x2 / xDivider;

            public double Y1 => y1 / yDivider;

            public double Y2 => y2 / yDivider;

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

                return Equals((FocusAreas)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = X1.GetHashCode();
                    hashCode = (hashCode * 397) ^ X2.GetHashCode();
                    hashCode = (hashCode * 397) ^ Y1.GetHashCode();
                    hashCode = (hashCode * 397) ^ Y2.GetHashCode();
                    return hashCode;
                }
            }

            internal void Fix(CameraPoint p)
            {
                x1 -= p.X;
                x2 -= p.X;
                y1 -= p.Y;
                y2 -= p.Y;

                xDivider -= p.X * 2;
                yDivider -= p.Y * 2;
            }

            protected bool Equals(Box other)
            {
                return Equals(X1, other.X1) && Equals(X2, other.X2) && Equals(Y1, other.Y1) && Equals(Y2, other.Y2);
            }
        }
    }
}