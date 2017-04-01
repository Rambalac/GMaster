// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using Camera;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public sealed partial class CameraViewControl : UserControl
    {
        public CameraViewControl()
        {
            InitializeComponent();
        }

        private CameraViewModel Model => DataContext as CameraViewModel;

        private Lumix Lumix => Model?.SelectedCamera?.Camera;

        private async void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var element = (UIElement)sender;
            var point = e.GetCurrentPoint(element);

            var x = (int)(point.Position.X * 1024 / element.RenderSize.Width);
            var y = (int)(point.Position.Y * 1024 / element.RenderSize.Height);

            if (Lumix != null)
            {
                await Lumix.SetFocusPoint(x, y);
            }
        }

        private async void UIElement_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (Model?.SelectedCamera != null)
            {
                var zoom = e.GetCurrentPoint((UIElement)sender).Properties.MouseWheelDelta;

                if (Lumix != null)
                {
                    await Lumix.ResizeFocusPoint(zoom);
                }
            }
        }
    }
}