namespace GMaster
{
    using Microsoft.Graphics.Canvas;
    using Views;
    using Windows.UI;

    public class Lut
    {
        public int BlueNum { get; set; }

        public Color[] Colors { get; set; }

        public int GreenNum { get; set; }

        public int RedNum { get; set; }

        public ILutEffectGenerator GetEffectGenerator(ICanvasResourceCreator liveView)
        {
            if (BlueNum > 0)
            {
                return new Lut3DEffectGenerator(this, liveView);
            }

            return new Lut1DEffectGenerator(this);
        }
    }
}