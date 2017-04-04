namespace GMaster.Camera
{
    using Tools;

    public interface ICameraMenuItem : IStringIdItem
    {
        string Command { get; }

        string CommandType { get; }

        string Text { get; }

        string Value { get; }
    }
}