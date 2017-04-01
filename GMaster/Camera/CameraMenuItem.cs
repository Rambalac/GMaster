using GMaster.Camera.LumixResponces;

namespace GMaster.Camera
{
    public class CameraMenuItem256 : ICameraMenuItem
    {
        public string Id { get; }

        public string Command { get; }

        public string CommandType { get; }

        public string Text { get; }

        public string Value => IntValue + "/256";

        public int IntValue { get; }

        public CameraMenuItem256(string id, string text, string command, string commandtype, int value)
        {
            Id = id;
            Text = text;
            Command = command;
            CommandType = commandtype;
            IntValue = value;
        }

        public override string ToString()
        {
            return Text;
        }

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

            return Equals((ICameraMenuItem)obj);
        }

        public override int GetHashCode()
        {
            return Text?.GetHashCode() ?? 0;
        }

        protected bool Equals(ICameraMenuItem other)
        {
            return string.Equals(Text, other.Id);
        }
    }

    public class CameraMenuItemText : ICameraMenuItem
    {
        public CameraMenuItemText(Item i, string text)
        {
            Id = i.Id;
            Text = text;
            Command = i.CmdMode;
            CommandType = i.CmdType;
            Value = i.CmdValue;
        }

        public CameraMenuItemText(string id, string text, string command, string commandtype, string value)
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

            return Equals((ICameraMenuItem)obj);
        }

        public override int GetHashCode()
        {
            return Text?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Text;
        }

        protected bool Equals(ICameraMenuItem other)
        {
            return string.Equals(Text, other.Text);
        }
    }
}