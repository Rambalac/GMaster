// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using Camera;
    using System;
    using System.Threading.Tasks;
    using Windows.UI.Input;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public sealed partial class CameraViewControl : UserControl
    {
        private readonly GestureRecognizer ImageGestureRecognizer;

        public CameraViewControl()
        {
            InitializeComponent();
            ImageGestureRecognizer = new GestureRecognizer();
            ImageGestureRecognizer.Tapped += ImageGestureRecognizer_Tapped;
            ImageGestureRecognizer.ManipulationUpdated += ImageGestureRecognizer_ManipulationUpdated;
            ImageGestureRecognizer.ManipulationCompleted += ImageGestureRecognizer_ManipulationCompleted;

            ImageGestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY | GestureSettings.Tap | GestureSettings.ManipulationScale;

            LiveView.PointerPressed += (sender, args) => ImageGestureRecognizer.ProcessDownEvent(args.GetCurrentPoint(LiveView));
            LiveView.PointerReleased += (sender, args) => ImageGestureRecognizer.ProcessUpEvent(args.GetCurrentPoint(LiveView));
            LiveView.PointerCanceled += (sender, args) => ImageGestureRecognizer.CompleteGesture();
            LiveView.PointerMoved += (sender, args) => ImageGestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(LiveView));
            LiveView.PointerWheelChanged += (sender, args) => ImageGestureRecognizer.ProcessMouseWheelEvent(
                args.GetCurrentPoint(LiveView),
                  args.KeyModifiers.HasFlag(Windows.System.VirtualKeyModifiers.Shift),
                  args.KeyModifiers.HasFlag(Windows.System.VirtualKeyModifiers.Control));

        }

        private async void ImageGestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            if (Lumix != null)
            {
                await MoveFocusPoint(args.Position.X, args.Position.Y);
            }
        }

        private async void ImageGestureRecognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            var now = DateTime.UtcNow;
            if (now - lastSkipable < SkipableInterval)
            {
                return;
            }

            lastSkipable = now;

            if (Lumix != null)
            {
                await Lumix.ResizeFocusPoint((int)Math.Sign(args.Delta.Expansion));
                if (args.Delta.Expansion != 0)
                {
                    await MoveFocusPoint(args.Position.X, args.Position.Y);
                }
            }
        }

        private async void ImageGestureRecognizer_Tapped(GestureRecognizer sender, TappedEventArgs args)
        {
            await MoveFocusPoint(args.Position.X, args.Position.Y);
        }

        private async void ImageGestureRecognizer_Dragging(GestureRecognizer sender, DraggingEventArgs args)
        {
            var now = DateTime.UtcNow;
            if (now - lastSkipable < SkipableInterval)
            {
                return;
            }

            lastSkipable = now;
            await MoveFocusPoint(args.Position.X, args.Position.Y);
        }

        private CameraViewModel Model => DataContext as CameraViewModel;

        private Lumix Lumix => Model?.SelectedCamera?.Camera;

        private DateTime lastSkipable;
        private readonly TimeSpan SkipableInterval = TimeSpan.FromMilliseconds(100);

        private async Task MoveFocusPoint(double x, double y)
        {
            var ix = (int)(x * 1024 / LiveView.RenderSize.Width);
            var iy = (int)(y * 1024 / LiveView.RenderSize.Height);

            if (Lumix != null)
            {
                await Lumix.SetFocusPoint(ix, iy);
            }
        }
    }
}