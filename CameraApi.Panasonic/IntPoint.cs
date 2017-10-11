namespace CameraApi.Panasonic
{
    using System;

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
            return new IntPoint(Math.Max(Math.Min(X, max), min), Math.Max(Math.Min(Y, max), min));
        }
    }
}