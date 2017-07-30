using GMaster.Core.Tools;

namespace GMaster.Core.Camera.Panasonic
{
    public interface ICameraMenuItem : IStringIdItem
    {
        string Command { get; }

        string CommandType { get; }

        string Text { get; }

        string Value { get; }
    }
}