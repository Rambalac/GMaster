namespace GMaster.Camera
{
    using LumixResponces;

    public interface ICameraMenuItem : IIdItem
    {
        string Command { get; }

        string CommandType { get; }

        string Text { get; }

        string Value { get; }
    }
}