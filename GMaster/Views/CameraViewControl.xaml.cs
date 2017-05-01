// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Camera;
    using Core.Camera.LumixData;
    using Core.Tools;
    using Microsoft.Graphics.Canvas.UI.Xaml;
    using Models;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Devices.Input;
    using Windows.UI.Core;
    using Windows.UI.Input;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public sealed partial class CameraViewControl : UserControl, IDisposable
    {
        private readonly FrameRenderer frame;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly GestureRecognizer imageGestureRecognizer;

        private readonly TimeSpan skipableInterval = TimeSpan.FromMilliseconds(200);

        private double aspect = 1;
        private Lumix currentLumix;
        private bool is43;
        private ConnectedCamera lastSelectedCamera;
        private DateTime lastSkipable;

        private bool manipulating;

        private bool manipulationJustStarted = true;

        public CameraViewControl()
        {
            InitializeComponent();
            imageGestureRecognizer = new GestureRecognizer();
            imageGestureRecognizer.Tapped += ImageGestureRecognizer_Tapped;
            imageGestureRecognizer.ManipulationUpdated += ImageGestureRecognizer_ManipulationUpdated;
            imageGestureRecognizer.ManipulationCompleted += ImageGestureRecognizer_ManipulationCompleted;

            imageGestureRecognizer.GestureSettings = GestureSettings.ManipulationTranslateX
                                                    | GestureSettings.ManipulationTranslateY
                                                    | GestureSettings.Tap
                                                    | GestureSettings.ManipulationScale
                                                    | GestureSettings.DoubleTap;

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
        }

        public CameraViewModel Model => DataContext as CameraViewModel;

        private Lumix Lumix => Model?.SelectedCamera?.Camera;

        public void Dispose()
        {
            frame.Dispose();
        }

        private async Task<IntPoint?> Camera_LiveViewUpdated(ArraySegment<byte> segment)
        {
            var size = await frame.UpdateBitmap(new MemoryStream(segment.Array, segment.Offset, segment.Count));
            if (size != null)
            {
                var intaspect = size.Value.X * 10 / size.Value.Y;
                is43 = intaspect == 13;
            }

            return size;
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

        private async void Capture_OnPressed(object sender, PointerRoutedEventArgs e)
        {
            var lumix = Model.SelectedCamera;
            if (lumix == null)
            {
                return;
            }

            await lumix.Camera.CaptureStart();
        }

        private async void Capture_OnReleased(object sender, PointerRoutedEventArgs e)
        {
            var lumix = Model.SelectedCamera;
            if (lumix == null)
            {
                return;
            }

            await lumix.Camera.CaptureStop();
        }

        private async void FocusAdjuster_OnRepeatClick(object sender, ChangeDirection obj)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeFocus(obj);
            }
        }

        private async void ImageGestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            manipulationJustStarted = true;
            if (Lumix == null)
            {
                return;
            }

            if (Math.Abs(args.Cumulative.Expansion) < 0.001)
            {
                await MoveFocusPoint(args.Position.X, args.Position.Y, PinchStage.Stop);
            }
            else
            {
                if (Lumix.LumixState.FocusMode == FocusMode.MF)
                {
                    if (args.PointerDeviceType != PointerDeviceType.Mouse)
                    {
                        await Lumix.MfAssistZoom(PinchStage.Stop, new FloatPoint(args.Position.X, args.Position.Y), (200 + args.Cumulative.Expansion) / 1000);
                    }

                    Debug.WriteLine("MF Pinch Zoom Stopped", "Manipulation");
                }
            }
        }

        private async void ImageGestureRecognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            if (manipulating || Lumix == null)
            {
                return;
            }

            manipulating = true;

            try
            {
                if (Math.Abs(args.Delta.Expansion) < 0.001 && Math.Abs(args.Delta.Translation.X) + Math.Abs(args.Delta.Translation.Y) < 0.001)
                {
                    return;
                }

                var now = DateTime.UtcNow;
                if (now - lastSkipable < skipableInterval)
                {
                    return;
                }

                lastSkipable = now;

                var pinchStage = manipulationJustStarted ? PinchStage.Start : PinchStage.Continue;
                if (Math.Abs(args.Delta.Expansion) < 0.001)
                {
                    await MoveFocusPoint(args.Position.X, args.Position.Y, pinchStage);
                    manipulationJustStarted = false;
                }
                else
                {
                    var floatPoint = ToFloatPoint(args.Position.X, args.Position.Y);
                    if (args.PointerDeviceType == PointerDeviceType.Mouse)
                    {
                        await PinchZoom(PinchStage.Start, floatPoint, 0);
                        var zoomed = args.Cumulative.Expansion * 3;
                        await Task.Delay(50);
                        await PinchZoom(PinchStage.Continue, floatPoint, 0.5f * zoomed);
                        await Task.Delay(50);
                        await PinchZoom(PinchStage.Stop, floatPoint, 1 * zoomed);
                    }
                    else
                    {
                        await PinchZoom(pinchStage, floatPoint, args.Cumulative.Expansion);
                    }

                    manipulationJustStarted = false;
                }
            }
            finally
            {
                manipulating = false;
            }
        }

        private async void ImageGestureRecognizer_Tapped(GestureRecognizer sender, TappedEventArgs args)
        {
            if (args.TapCount == 1)
            {
                await MoveFocusPoint(args.Position.X, args.Position.Y, PinchStage.Single);
            }
        }

        private void LiveView_OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (Model.SelectedCamera?.Camera != null)
            {
                var drawaspect = aspect;
                if (Model.SelectedCamera.IsAspectAnamorphingVideoOnly &&
                    !(Model.SelectedCamera.Camera.LumixState.IsVideoMode && is43))
                {
                    drawaspect = 1;
                }

                frame.Draw(args.DrawingSession, sender.ActualWidth, sender.ActualHeight, drawaspect, Model.FocusAreas);
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

                    frame.Reset();
                    var newcamera = Model?.SelectedCamera?.Camera;
                    if (!ReferenceEquals(newcamera, currentLumix))
                    {
                        UpdateCamera(newcamera);
                    }

                    break;
            }
        }

        private async Task MoveFocusPoint(double x, double y, PinchStage stage)
        {
            var fp = ToFloatPoint(x, y);
            if (fp.IsInRange(0f, 1f) && Lumix != null)
            {
                if (Lumix.LumixState.FocusMode != FocusMode.MF)
                {
                    await Lumix.FocusPointMove(fp);
                }
                else
                {
                    await Lumix.MfAssistMove(stage, fp);
                }
            }
        }

        private async Task PinchZoom(PinchStage stage, FloatPoint point, float extend)
        {
            extend = (200f + extend) / 1000f;
            if (Lumix.LumixState.FocusMode != FocusMode.MF)
            {
                if (Lumix.LumixState.FocusAreas != null
                    && Lumix.LumixState.FocusAreas.Boxes.Any(b => b.Props.Type == FocusAreaType.OneAreaSelected))
                {
                    await Lumix.FocusPointResize(stage, point, extend);
                }
            }
            else
            {
                await Lumix.MfAssistZoom(stage, point, extend);
            }
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

                      case nameof(ConnectedCamera.Camera):
                          UpdateCamera(Model?.SelectedCamera?.Camera);
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

        private FloatPoint ToFloatPoint(double x, double y)
        {
            var ix = (float)((x - frame.ImageRect.X) / frame.ImageRect.Width);
            var iy = (float)((y - frame.ImageRect.Y) / frame.ImageRect.Height);
            return new FloatPoint(ix, iy);
        }

        private void UpdateCamera(Lumix newcamera)
        {
            if (currentLumix != null)
            {
                currentLumix.LiveViewUpdated -= Camera_LiveViewUpdated;
            }

            currentLumix = newcamera;

            if (currentLumix != null)
            {
                currentLumix.LiveViewUpdated += Camera_LiveViewUpdated;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LiveView.RemoveFromVisualTree();
        }

        private async void ZoomAdjuster_OnPressedReleased(object sender, ChangeDirection obj)
        {
            if (Model?.SelectedCamera?.Camera != null)
            {
                await Model.SelectedCamera.Camera.ChangeZoom(obj);
            }
        }
    }
}