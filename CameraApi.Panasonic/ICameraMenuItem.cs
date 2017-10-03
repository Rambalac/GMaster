namespace CameraApi.Panasonic
{
    using CameraApi.Core;

    public interface ICameraMenuItem : IActionItem
    {
        string Command { get; }

        string CommandType { get; }

        string Value { get; }
    }
}