namespace CameraApi.Panasonic
{
    using CameraApi.Core;

    public interface ICameraMenuItem : IActionItem
    {
        string Id { get; }

        string Command { get; }

        string CommandType { get; }

        string Value { get; }
    }
}