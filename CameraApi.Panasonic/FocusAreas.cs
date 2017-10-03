using System.Collections.Generic;
using CameraApi.Panasonic.LumixData;

namespace CameraApi.Panasonic
{
    public class FocusAreas
    {
        private static readonly Dictionary<int, IntPoint> FocusPointShifts = new Dictionary<int, IntPoint>
        {
            { 13, new IntPoint(0, 0) },
            { 17, new IntPoint(0, 125) },
            { 15, new IntPoint(0, 58) },
            { 10, new IntPoint(125, 0) }
        };

        private readonly List<Box> boxes;
        private readonly IntPoint focusPointShift;
        private int hashcode;

        public FocusAreas(int number, IntPoint size, bool fix)
        {
            boxes = new List<Box>(number);
            if (fix)
            {
                var intaspect = size.X * 10 / size.Y;
                focusPointShift = FocusPointShifts.TryGetValue(intaspect, out var val) ? val : FocusPointShifts[13];
            }
        }

        public IReadOnlyList<Box> Boxes => boxes;

        public void AddBox(int x1, int y1, int x2, int y2, FocusAreaType type, bool failed)
        {
            var box = new Box(x1, y1, x2, y2, type, failed);
            hashcode = (hashcode * 397) ^ box.GetHashCode();
            box.Fix(focusPointShift);
            if (box.X1 >= 0 && box.X1 <= 1 && box.X2 >= 0 && box.X2 <= 1 && box.Y1 >= 0 && box.Y1 <= 1 && box.Y1 >= 0 && box.Y2 <= 1 && box.X2 >= box.X1 && box.Y2 >= box.Y1)
            {
                boxes.Add(box);
            }
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
            private readonly BoxProps props;
            private int x1;
            private int x2;
            private float xDivider = 1000;
            private int y1;
            private int y2;
            private float yDivider = 1000;

            internal Box(int x1, int y1, int x2, int y2, FocusAreaType type, bool failed)
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;
                props.Type = type;
                props.Failed = failed;
            }

            public float Height => (y2 - y1) / yDivider;

            public BoxProps Props => props;

            public float Width => (x2 - x1) / xDivider;

            public float X1 => x1 / xDivider;

            public float X2 => x2 / xDivider;

            public float Y1 => y1 / yDivider;

            public float Y2 => y2 / yDivider;

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

                return Equals((Box)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = props.GetHashCode();
                    hashCode = (hashCode * 397) ^ x1;
                    hashCode = (hashCode * 397) ^ x2;
                    hashCode = (hashCode * 397) ^ y1;
                    hashCode = (hashCode * 397) ^ y2;
                    return hashCode;
                }
            }

            internal void Fix(IntPoint p)
            {
                x1 -= p.X;
                x2 -= p.X;
                y1 -= p.Y;
                y2 -= p.Y;

                xDivider = 1000 - (p.X * 2);
                yDivider = 1000 - (p.Y * 2);
            }

            protected bool Equals(Box other)
            {
                return props.Equals(other.props) && x1 == other.x1 && x2 == other.x2 && y1 == other.y1 && y2 == other.y2;
            }

            public struct BoxProps
            {
                public bool Failed { get; internal set; }

                public FocusAreaType Type { get; internal set; }

                public bool Equals(BoxProps other)
                {
                    return Failed == other.Failed && Type == other.Type;
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj))
                    {
                        return false;
                    }

                    return obj is BoxProps && Equals((BoxProps)obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        return (Failed.GetHashCode() * 397) ^ (int)Type;
                    }
                }
            }
        }
    }
}