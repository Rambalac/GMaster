// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Tools
{
    using Microsoft.Graphics.Canvas;
    using Microsoft.Graphics.Canvas.Effects;
    using Windows.Graphics.Effects;

    public class Lut3DEffectGenerator : ILutEffectGenerator
    {
        private readonly EffectTransferTable3D table;

        public Lut3DEffectGenerator(Lut lut, ICanvasResourceCreator liveView)
        {
            table = EffectTransferTable3D.CreateFromColors(liveView, lut.Colors, lut.BlueNum, lut.GreenNum, lut.RedNum);
        }

        public ICanvasEffect GenerateEffect(IGraphicsEffectSource source)
        {
            return new TableTransfer3DEffect { Table = table, Source = source };
        }
    }
}