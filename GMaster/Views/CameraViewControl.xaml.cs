// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using System;
    using System.Threading.Tasks;
    using Camera;
    using Windows.UI.Input;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public sealed partial class CameraViewControl : UserControl
    {
        private readonly GestureRecognizer ImageGestureRecognizer;

        private readonly TimeSpan SkipableInterval = TimeSpan.FromMilliseconds(100);

        private DateTime lastSkipable;

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
                  true);
            DataContextChanged += CameraViewControl_DataContextChanged;
        }

        private Lumix Lumix => Model?.SelectedCamera?.Camera;

        private CameraViewModel Model => DataContext as CameraViewModel;

        private void CameraViewControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                Model.PropertyChanged += Model_PropertyChanged;
            }
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

        private void LiveView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecalulateFocusPoint();
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CameraViewModel.FocusPoint):
                    RecalulateFocusPoint();
                    break;
            }
        }

        private async Task MoveFocusPoint(double x, double y)
        {
            var ix = x / LiveView.RenderSize.Width;
            var iy = y / LiveView.RenderSize.Height;

            if (Lumix != null)
            {
                await Lumix.SetFocusPoint(ix, iy);
            }
        }

        private void RecalulateFocusPoint()
        {
            if (Model.FocusPoint == null)
            {
                FocusPoint.Visibility = Visibility.Collapsed;
                return;
            }

            var parent = LiveView.Parent as FrameworkElement;
            var tW = parent.ActualWidth;
            var tH = parent.ActualHeight;

            var iW = LiveView.RenderSize.Width;
            var iH = LiveView.RenderSize.Height;

            double left;
            double top;
            if (Math.Abs(tH - iH) < 0.1f) // equal
            {
                left = (tW - iW) / 2;
                top = 0;
            }
            else
            {
                left = 0;
                top = (tH - iH) / 2;
            }

            var fp = Model.FocusPoint;
            var x1 = fp.X1 * iW;
            var x2 = fp.X2 * iW;
            var y1 = fp.Y1 * iH;
            var y2 = fp.Y2 * iH;

            FocusPoint.Margin = new Thickness(left + x1, top + y1, left + iW - x2, top + iH - y2);
            var t = FocusPoint.StrokeThickness;
            FocusPointGeometry.Transform = new CompositeTransform { ScaleX = x2 - x1 - t, ScaleY = y2 - y1 - t, TranslateX = t / 2, TranslateY = t / 2 };
            FocusPoint.Visibility = Visibility.Visible;
        }
    }
}