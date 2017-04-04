// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using System;
    using System.Threading.Tasks;
    using Camera;
    using Camera.LumixData;
    using Windows.UI.Input;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Imaging;

    public sealed partial class CameraViewControl : UserControl
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly GestureRecognizer imageGestureRecognizer;

        private readonly TimeSpan skipableInterval = TimeSpan.FromMilliseconds(100);

        private DateTime lastSkipable;

        public CameraViewControl()
        {
            InitializeComponent();
            imageGestureRecognizer = new GestureRecognizer();
            imageGestureRecognizer.Tapped += ImageGestureRecognizer_Tapped;
            imageGestureRecognizer.ManipulationUpdated += ImageGestureRecognizer_ManipulationUpdated;
            imageGestureRecognizer.ManipulationCompleted += ImageGestureRecognizer_ManipulationCompleted;

            imageGestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY | GestureSettings.Tap | GestureSettings.ManipulationScale;

            LiveView.PointerPressed += (sender, args) => imageGestureRecognizer.ProcessDownEvent(args.GetCurrentPoint(LiveView));
            LiveView.PointerReleased += (sender, args) => imageGestureRecognizer.ProcessUpEvent(args.GetCurrentPoint(LiveView));
            LiveView.PointerCanceled += (sender, args) => imageGestureRecognizer.CompleteGesture();
            LiveView.PointerMoved += (sender, args) => imageGestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(LiveView));
            LiveView.PointerWheelChanged += (sender, args) => imageGestureRecognizer.ProcessMouseWheelEvent(
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

        private double lastExpansion;
        private async void ImageGestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            if (Lumix != null && Math.Abs(args.Cumulative.Expansion) < 0.001)
            {
                await MoveFocusPoint(args.Position.X, args.Position.Y);
            }
        }

        private async void ImageGestureRecognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            if (Lumix != null)
            {
                lastExpansion += args.Delta.Expansion;
                if (Math.Abs(lastExpansion) > 32)
                {
                    await Lumix.ResizeFocusPoint(Math.Sign(lastExpansion));
                    lastExpansion = 0;
                }

                if (Math.Abs(args.Delta.Expansion) < 0.001)
                {
                    var now = DateTime.UtcNow;
                    if (now - lastSkipable < skipableInterval)
                    {
                        return;
                    }

                    lastSkipable = now;
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

            if (!(LiveView.Parent is FrameworkElement parent))
            {
                return;
            }

            var tW = parent.ActualWidth;
            var tH = parent.ActualHeight;

            var iW = LiveView.RenderSize.Width;
            var iH = LiveView.RenderSize.Height;

            double left;
            double top;

            // equal
            if (Math.Abs(tH - iH) < 0.1f)
            {
                left = (tW - iW) / 2;
                top = 0;
            }
            else
            {
                left = 0;
                top = (tH - iH) / 2;
            }

            if (LiveView?.Source is BitmapSource bitmap)
            {
                var fp = Model.FocusPoint;

                double x1 = fp.X1, x2 = fp.X2, y1 = fp.Y1, y2 = fp.Y2;
                if (!fp.Fixed)
                {
                    var shiftX = 0f;
                    var shiftY = 0f;
                    switch (bitmap.PixelWidth * 10 / bitmap.PixelHeight)
                    {
                        case 17:
                            shiftY = 0.125f;
                            break;
                        case 15:
                            shiftY = 0.058f;
                            break;
                        case 10:
                            shiftX = 0.125f;
                            break;
                    }

                    x1 = (x1 - shiftX) / (1 - (2 * shiftX));
                    x2 = (x2 - shiftX) / (1 - (2 * shiftX));
                    y1 = (y1 - shiftY) / (1 - (2 * shiftY));
                    y2 = (y2 - shiftY) / (1 - (2 * shiftY));
                }

                x1 = x1 * iW;
                x2 = x2 * iW;
                y1 = y1 * iH;
                y2 = y2 * iH;

                FocusPoint.Margin = new Thickness(left + x1, top + y1, left + iW - x2, top + iH - y2);
                var t = FocusPoint.StrokeThickness;
                FocusPointGeometry.Transform = new CompositeTransform
                {
                    ScaleX = x2 - x1 - t,
                    ScaleY = y2 - y1 - t,
                    TranslateX = t / 2,
                    TranslateY = t / 2
                };
                FocusPoint.Visibility = Visibility.Visible;
            }
        }

        private async void ZoomReleased(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeZoom(ChangeDirection.ZoomStop);
            }
        }

        private async void ZoomWideFast_Pressed(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeZoom(ChangeDirection.WideFast);
            }
        }

        private async void ZoomWideNormal_Pressed(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeZoom(ChangeDirection.WideNormal);
            }
        }

        private async void ZoomTeleNormal_Pressed(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeZoom(ChangeDirection.TeleNormal);
            }
        }

        private async void ZoomTeleFast_Pressed(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeZoom(ChangeDirection.TeleFast);
            }
        }

        private async void FocusWideFast_Pressed(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeFocus(ChangeDirection.WideFast);
            }
        }

        private async void FocusWideNormal_Pressed(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeFocus(ChangeDirection.WideNormal);
            }
        }

        private async void FocusTeleNormal_Pressed(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeFocus(ChangeDirection.TeleNormal);
            }
        }

        private async void FocusTeleFast_Pressed(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeFocus(ChangeDirection.TeleFast);
            }
        }
    }
}