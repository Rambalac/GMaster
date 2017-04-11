using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    public sealed partial class FocusBox
    {
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(FocusBox), new PropertyMetadata(default(double)));

        public FocusBox()
        {
            InitializeComponent();
        }

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        private void FocusBox_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var t = StrokeThickness;
            FocusPointGeometry.Transform = new CompositeTransform
            {
                ScaleX = e.NewSize.Width - t,
                ScaleY = e.NewSize.Height - t,
                TranslateX = t / 2,
                TranslateY = t / 2
            };
        }
    }
}