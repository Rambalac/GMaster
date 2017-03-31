using GMaster.Camera.LumixResponces;

namespace GMaster.Camera
{
    public class CameraMenuItem : IIdItem
    {
        public CameraMenuItem(Item i, string text)
        {
            Id = i.Id;
            Text = text;
            Command = i.CmdMode;
            CommandType = i.CmdType;
            Value = i.CmdValue;
        }

        public CameraMenuItem(string id, string text, string command, string commandtype, string value)
        {
            Id = id;
            Text = text;
            Command = command;
            CommandType = commandtype;
            Value = value;
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

        public override string ToString()
        {
            return Text;
        }

        protected bool Equals(CameraMenuItem other)
        {
            return string.Equals(Id, other.Id);
        }
    }
}