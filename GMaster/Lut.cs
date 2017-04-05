namespace GMaster
{
    using System.Collections.Generic;
    using Windows.UI;

    public class Lut
    {
        public int BlueNum { get; set; }
        public IReadOnlyCollection<Color> Colors { get; set; }
        public int GreenNum { get; set; }
        public int RedNum { get; set; }
    }
}