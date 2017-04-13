// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using System.Collections.Generic;
    using Core.Camera;
    using Windows.Foundation;
    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media;

    public sealed partial class FocusBox
    {
        public static readonly DependencyProperty PropsProperty = DependencyProperty.Register(
            "Props", typeof(FocusAreas.Box.BoxProps), typeof(FocusBox), new PropertyMetadata(default(FocusAreas.Box.BoxProps), PropsChanged));

        public static readonly DependencyProperty RectProperty = DependencyProperty.Register(
            "Rect", typeof(Rect), typeof(FocusBox), new PropertyMetadata(default(Rect), RectChanged));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
                                    "StrokeThickness", typeof(double), typeof(FocusBox), new PropertyMetadata(default(double), StrokeThicknessChanged));

        private static readonly Brush BoxBrush = new SolidColorBrush(Colors.Gold);

        private static readonly Dictionary<FocusAreaType, Brush> Brushes = new Dictionary<FocusAreaType, Brush>
        {
            { FocusAreaType.OneAreaSelected, new SolidColorBrush(Colors.Gold) },
            { FocusAreaType.TrackLock, new SolidColorBrush(Colors.Gold) },
            { FocusAreaType.TrackUnlock, new SolidColorBrush(Colors.White) },
            { FocusAreaType.FaceOther, new SolidColorBrush(Colors.White) }
        };

        private static readonly Brush EyeBrush = new SolidColorBrush(Colors.White);

        private static readonly Brush FailedBrush = new SolidColorBrush(Colors.Red);

        public FocusBox()
        {
            InitializeComponent();
        }

        public FocusAreas.Box.BoxProps Props
        {
            get => (FocusAreas.Box.BoxProps)GetValue(PropsProperty);
            set => SetValue(PropsProperty, value);
        }

        public Rect Rect
        {
            get => (Rect)GetValue(RectProperty);
            set => SetValue(RectProperty, value);
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        private static void PropsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ths = (FocusBox)d;
            ths.ChangeProps();
        }

        private static void RectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ths = (FocusBox)d;
            ths.Transform();
        }

        private static void StrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FocusBox)d).SelectPath.StrokeThickness = (double)e.NewValue;
            ((FocusBox)d).BoxPath.StrokeThickness = (double)e.NewValue;
            ((FocusBox)d).CrossPath.StrokeThickness = (double)e.NewValue;
        }

        private void ChangeProps()
        {
            switch (Props.Type)
            {
                case FocusAreaType.OneAreaSelected:
                case FocusAreaType.FaceOther:
                case FocusAreaType.TrackLock:
                case FocusAreaType.TrackUnlock:
                    SelectPath.Stroke = Props.Failed ? FailedBrush : Brushes[Props.Type];

                    SelectPath.Visibility = Visibility.Visible;
                    BoxPath.Visibility = Visibility.Collapsed;
                    CrossPath.Visibility = Visibility.Collapsed;
                    break;

                case FocusAreaType.MainFace:
                case FocusAreaType.Box:
                    SelectPath.Visibility = Visibility.Collapsed;
                    BoxPath.Visibility = Visibility.Visible;
                    CrossPath.Visibility = Visibility.Collapsed;
                    BoxPath.StrokeThickness = StrokeThickness;
                    BoxPath.Stroke = BoxBrush;
                    break;

                case FocusAreaType.Eye:
                    SelectPath.Visibility = Visibility.Collapsed;
                    BoxPath.Visibility = Visibility.Visible;
                    CrossPath.Visibility = Visibility.Collapsed;
                    BoxPath.StrokeThickness = 1;
                    BoxPath.Stroke = EyeBrush;
                    break;

                case FocusAreaType.Cross:
                    SelectPath.Visibility = Visibility.Collapsed;
                    BoxPath.Visibility = Visibility.Collapsed;
                    CrossPath.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Transform()
        {
            var t = StrokeThickness;
            PathGeometry geom = null;
            switch (Props.Type)
            {
                case FocusAreaType.OneAreaSelected:
                case FocusAreaType.FaceOther:
                case FocusAreaType.TrackLock:
                case FocusAreaType.TrackUnlock:
                    geom = SelectGeometry;
                    break;

                case FocusAreaType.Eye:
                    t = 0;
                    goto case FocusAreaType.Box;
                case FocusAreaType.MainFace:
                case FocusAreaType.Box:
                    geom = BoxGeometry;
                    break;

                case FocusAreaType.Cross:
                    t = 0;
                    geom = CrossGeometry;
                    break;

                default:
                    return;
            }

            geom.Transform = new CompositeTransform
            {
                ScaleX = Rect.Width - t,
                ScaleY = Rect.Height - t,
                TranslateX = Rect.Left + (t / 2),
                TranslateY = Rect.Top + (t / 2)
            };
        }
    }
}