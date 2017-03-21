namespace GMaster.Camera
{
    using LumixResponces;

    public class CameraMenuItem
    {
        public CameraMenuItem(Item i, string text)
        {
            Id = i.Id;
            Text = text;
            Command = i.CmdMode;
            CommandType = i.CmdType;
            Value = i.CmdValue;
        }

        public string Command { get; }

        public string CommandType { get; }

        public string Id { get; }

        public string Text { get; }

        public string Value { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((CameraMenuItem)obj);
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        protected bool Equals(CameraMenuItem other)
        {
            return string.Equals(Id, other.Id);
        }
    }
}