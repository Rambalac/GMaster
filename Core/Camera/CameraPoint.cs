namespace GMaster.Core.Camera
{
    public struct CameraPoint
    {
        public CameraPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }
    }
}