namespace CameraApi.Core
{
    public struct FloatPoint
    {
        public FloatPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public FloatPoint(double x, double y)
        {
            X = (float)x;
            Y = (float)y;
        }

        public float X { get; }

        public float Y { get; }

        public static FloatPoint operator -(FloatPoint p, float a)
        {
            return new FloatPoint(p.X - a, p.Y - a);
        }

        public static FloatPoint operator +(FloatPoint p, float a)
        {
            return new FloatPoint(p.X + a, p.Y + a);
        }

        public bool IsInRange(float min, float max)
        {
            return X >= min && Y >= min && X <= max && Y <= max;
        }
    }
}