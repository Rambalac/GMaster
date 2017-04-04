// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Camera;
    using Camera.LumixData;
    using Microsoft.Graphics.Canvas;
    using Microsoft.Graphics.Canvas.UI.Xaml;
    using Windows.Foundation;
    using Windows.UI.Input;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public sealed partial class CameraViewControl : UserControl, IDisposable
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly GestureRecognizer imageGestureRecognizer;

        private readonly TimeSpan skipableInterval = TimeSpan.FromMilliseconds(100);

        private Lumix lastCamera;
        private double lastExpansion;
        private CanvasBitmap lastLiveViewBitmap;
        private DateTime lastSkipable;

        private CanvasBitmap liveViewBitmap;

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

        public void Dispose()
        {
            lastLiveViewBitmap?.Dispose();
        }

        private async void Camera_LiveViewUpdated(Stream stream)
        {
            stream.Position = 0;
            liveViewBitmap = await CanvasBitmap.LoadAsync(LiveView, stream.AsRandomAccessStream());
            LiveView.Invalidate();
        }

        private void CameraViewControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                Model.PropertyChanged += Model_PropertyChanged;
            }
        }

        private async void FocusAdjuster_OnRepeatClick(ChangeDirection obj)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                Debug.WriteLine(obj);
                await Model.SelectedCamera.Camera.ChangeFocus(obj);
            }
        }

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
                    var sig = Math.Sign(lastExpansion);
                    lastExpansion = 0;
                    await Lumix.ResizeFocusPoint(sig);
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

        private void LiveView_OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var bitmap = liveViewBitmap;
            if (!ReferenceEquals(lastLiveViewBitmap, bitmap))
            {
                lastLiveViewBitmap?.Dispose();
            }

            lastLiveViewBitmap = bitmap;
            if (bitmap == null)
            {
                return;
            }

            var iW = bitmap.SizeInPixels.Width;
            var iH = bitmap.SizeInPixels.Height;

            var wW = LiveView.ActualWidth;
            var wH = LiveView.ActualHeight;

            var scaleX = wW / iW;
            var scaleY = wH / iH;

            var scale = Math.Min(scaleX, scaleY);

            var rH = iH * scale;
            var rW = iW * scale;

            args.DrawingSession.DrawImage(bitmap, new Rect((wW - rW) / 2, (wH - rH) / 2, rW, rH), new Rect(0, 0, iW, iH), 1.0f, CanvasImageInterpolation.Linear);
        }

        private void LiveView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RecalulateFocusPoint();
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CameraViewModel.SelectedCamera):

                    var newcamera = Model?.SelectedCamera?.Camera;
                    if (!ReferenceEquals(newcamera, lastCamera))
                    {
                        if (lastCamera != null)
                        {
                            lastCamera.LiveViewUpdated -= Camera_LiveViewUpdated;
                        }

                        lastCamera = newcamera;

                        if (lastCamera != null)
                        {
                            lastCamera.LiveViewUpdated += Camera_LiveViewUpdated;
                        }
                    }

                    break;

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

            if (LiveView.Parent is FrameworkElement parent)
            {
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

                var bitmap = liveViewBitmap;
                if (bitmap != null)
                {
                    var fp = Model.FocusPoint;

                    double x1 = fp.X1, x2 = fp.X2, y1 = fp.Y1, y2 = fp.Y2;
                    if (!fp.Fixed)
                    {
                        var shiftX = 0f;
                        var shiftY = 0f;
                        switch (bitmap.SizeInPixels.Width * 10 / bitmap.SizeInPixels.Height)
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
        }

        private async void ZoomAdjuster_OnPressedReleased(ChangeDirection obj)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                Debug.WriteLine(obj);
                await Model.SelectedCamera.Camera.ChangeZoom(obj);
            }
        }
    }
}