// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Camera;
    using Camera.LumixData;
    using Microsoft.Graphics.Canvas.UI.Xaml;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Foundation;
    using Windows.UI.Core;
    using Windows.UI.Input;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

    public sealed partial class CameraViewControl : UserControl, IDisposable
    {
        private static readonly Dictionary<int, Point> FocusPointShifts = new Dictionary<int, Point>
        {
            { 13, new Point(0, 0) },
            { 17, new Point(0, 0.125f) },
            { 15, new Point(0, 0.058f) },
            { 10, new Point(0.125f, 0) }
        };

        private readonly FrameRenderer frame;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly GestureRecognizer imageGestureRecognizer;

        private readonly TimeSpan skipableInterval = TimeSpan.FromMilliseconds(100);

        private double aspect = 1;
        private Point focusPointShift;
        private bool is43;
        private Lumix lastCamera;
        private double lastExpansion;
        private ConnectedCamera lastSelectedCamera;
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
            frame = new FrameRenderer(LiveView);
            frame.ImageRectChanged += Frame_ImageRectChanged;
        }

        public CameraViewModel Model => DataContext as CameraViewModel;

        private Lumix Lumix => Model?.SelectedCamera?.Camera;

        public void Dispose()
        {
            frame.Dispose();
        }

        private async void Camera_LiveViewUpdated(ArraySegment<byte> segment)
        {
            var size = await frame.UpdateBitmap(new MemoryStream(segment.Array, segment.Offset, segment.Count));
            if (size != null)
            {
                var intaspect = (int)(size.Value.Width * 10 / size.Value.Height);
                focusPointShift = FocusPointShifts.TryGetValue(intaspect, out var val) ? val : new Point(0, 0);
                is43 = intaspect == 13;
            }
        }

        private async Task CameraSet()
        {
            await SetLut();
            SetAspect();
        }

        private void CameraViewControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                Model.PropertyChanged += Model_PropertyChanged;
                Model.Dispatcher = Dispatcher;
            }
        }

        private void CameraViewControl_OnDragOver(object sender, DragEventArgs e)
        {
            if (e.DataView != null && e.DataView.Properties.ContainsKey("camera"))
            {
                e.AcceptedOperation = DataPackageOperation.Link;
            }
        }

        private void CameraViewControl_OnDrop(object sender, DragEventArgs e)
        {
            if (e.DataView != null && e.DataView.Properties.TryGetValue("camera", out object camera))
            {
                Model.SelectedCamera = camera as ConnectedCamera;
            }
        }

        private async void FocusAdjuster_OnRepeatClick(ChangeDirection obj)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeFocus(obj);
            }
        }

        private void Frame_ImageRectChanged(Rect obj)
        {
            RecalulateFocusPoint();
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
            if (Model.SelectedCamera != null)
            {
                var drawaspect = aspect;
                if (Model.SelectedCamera.IsAspectAnamorphingVideoOnly &&
                    !(Model.SelectedCamera.Camera.IsVideoMode && is43))
                {
                    drawaspect = 1;
                }

                frame.Draw(args.DrawingSession, sender.ActualWidth, sender.ActualHeight, drawaspect);
            }
        }

        private async void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(CameraViewModel.SelectedCamera):
                    if (lastSelectedCamera != null)
                    {
                        lastSelectedCamera.PropertyChanged -= SelectedCamera_PropertyChanged;
                    }

                    lastSelectedCamera = Model?.SelectedCamera;
                    if (lastSelectedCamera != null)
                    {
                        await CameraSet();
                        lastSelectedCamera.PropertyChanged += SelectedCamera_PropertyChanged;
                    }

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
            var ix = (x - frame.ImageRect.X) / frame.ImageRect.Width;
            var iy = (y - frame.ImageRect.Y) / frame.ImageRect.Height;
            if (ix >= 0 && iy >= 0 && ix <= 1 && iy <= 1 && Lumix != null)
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

            var fp = Model.FocusPoint;

            if (!frame.IsReady)
            {
                return;
            }

            double x1 = fp.X1, x2 = fp.X2, y1 = fp.Y1, y2 = fp.Y2;
            if (!fp.Fixed)
            {
                x1 = (x1 - focusPointShift.X) / (1 - (2 * focusPointShift.X));
                x2 = (x2 - focusPointShift.X) / (1 - (2 * focusPointShift.X));
                y1 = (y1 - focusPointShift.Y) / (1 - (2 * focusPointShift.Y));
                y2 = (y2 - focusPointShift.Y) / (1 - (2 * focusPointShift.Y));
            }

            x1 = x1 * frame.ImageRect.Width;
            x2 = x2 * frame.ImageRect.Width;
            y1 = y1 * frame.ImageRect.Height;
            y2 = y2 * frame.ImageRect.Height;

            var iW = LiveView.ActualWidth;
            var iH = LiveView.ActualHeight;

            FocusPoint.Margin = new Thickness(frame.ImageRect.X + x1, frame.ImageRect.Y + y1, iW - frame.ImageRect.X - x2, iH - frame.ImageRect.Y - y2);
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

        private void SelectedCamera_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
              {
                  switch (e.PropertyName)
                  {
                      case nameof(ConnectedCamera.SelectedLut):
                          await SetLut();
                          break;

                      case nameof(ConnectedCamera.SelectedAspect):
                      case nameof(ConnectedCamera.IsAspectAnamorphingVideoOnly):
                          SetAspect();
                          break;
                  }
              });
        }

        private void SetAspect()
        {
            if (!double.TryParse(Model.SelectedCamera.SelectedAspect, out aspect))
            {
                aspect = 1;
            }
        }

        private async Task SetLut()
        {
            var selectedLut = Model?.SelectedCamera?.SelectedLut;
            if (selectedLut?.Id == null)
            {
                frame.LutEffect = null;
                return;
            }

            frame.LutEffect = (await selectedLut.LoadLut())?.GetEffectGenerator(LiveView);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LiveView.RemoveFromVisualTree();
        }

        private async void ZoomAdjuster_OnPressedReleased(ChangeDirection obj)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeZoom(obj);
            }
        }
    }
}