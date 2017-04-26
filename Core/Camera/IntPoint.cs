namespace GMaster.Core.Camera
{
    using static System.Math;

    public struct IntPoint
    {
        public IntPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public IntPoint(FloatPoint p1, float m)
        {
            X = (int)(p1.X * m);
            Y = (int)(p1.Y * m);
        }

        public int X { get; }

        public int Y { get; }

        public IntPoint Clamp(int min, int max)
        {
            return new IntPoint(Max(Min(X, max), min), Max(Min(Y, max), min));
        }
    }
}