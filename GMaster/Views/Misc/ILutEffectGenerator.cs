// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using Microsoft.Graphics.Canvas.Effects;
    using Windows.Graphics.Effects;

    public interface ILutEffectGenerator
    {
        ICanvasEffect GenerateEffect(IGraphicsEffectSource source);
    }
}