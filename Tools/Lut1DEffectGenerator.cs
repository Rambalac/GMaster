namespace GMaster.Tools
{
    using System.Linq;
    using Microsoft.Graphics.Canvas.Effects;
    using Windows.Graphics.Effects;

    public class Lut1DEffectGenerator : ILutEffectGenerator
    {
        private readonly float[] blueTable;
        private readonly float[] greenTable;
        private readonly float[] redTable;

        public Lut1DEffectGenerator(Lut lut)
        {
            redTable = lut.Colors.Select(c => c.R / 255f).ToArray();
            greenTable = lut.Colors.Select(c => c.G / 255f).ToArray();
            blueTable = lut.Colors.Select(c => c.B / 255f).ToArray();
        }

        public ICanvasEffect GenerateEffect(IGraphicsEffectSource source)
        {
            return new TableTransferEffect { AlphaDisable = true, RedTable = redTable, GreenTable = greenTable, BlueTable = blueTable, Source = source };
        }
    }
}