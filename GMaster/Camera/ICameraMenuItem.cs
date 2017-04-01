using GMaster.Camera.LumixResponces;

namespace GMaster.Camera
{
    public interface ICameraMenuItem : IIdItem
    {
        string Command { get; }

        string CommandType { get; }

        string Text { get; }

        string Value { get; }
    }
}