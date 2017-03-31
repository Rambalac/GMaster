namespace GMaster.Camera
{
    public struct TextBinValue
    {
        public TextBinValue(string text, int bin)
        {
            Text = text;
            Bin = bin;
        }

        public int Bin { get; }

        public string Text { get; }

        public bool Equals(TextBinValue other)
        {
            return string.Equals(Text, other.Text) && Bin == other.Bin;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TextBinValue && Equals((TextBinValue)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Text?.GetHashCode() ?? 0) * 397) ^ Bin;
            }
        }
    }
}