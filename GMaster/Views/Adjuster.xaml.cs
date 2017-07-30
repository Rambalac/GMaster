// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using System;
    using Core.Camera.Panasonic.LumixData;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public sealed partial class Adjuster : UserControl
    {
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(int), typeof(Adjuster), new PropertyMetadata(default(int), MaximumChanged));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum", typeof(int), typeof(Adjuster), new PropertyMetadata(default(int), MinimumChanged));

        public static readonly DependencyProperty RepeatIntervalProperty = DependencyProperty.Register(
            "RepeatInterval", typeof(int), typeof(Adjuster), new PropertyMetadata(default(int), RepeatIntervalChanged));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                    "Value", typeof(int), typeof(Adjuster), new PropertyMetadata(default(int), ValueChanged));

        public Adjuster()
        {
            InitializeComponent();
        }

        public event EventHandler<ChangeDirection> PressedReleased;

        public event EventHandler<ChangeDirection> RepeatClick;

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public int RepeatInterval
        {
            get => (int)GetValue(RepeatIntervalProperty);
            set => SetValue(RepeatIntervalProperty, value);
        }

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static void MaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Adjuster)d).Slider.Maximum = (int)e.NewValue;
        }

        private static void MinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Adjuster)d).Slider.Minimum = (int)e.NewValue;
        }

        private static void RepeatIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var t = (Adjuster)d;
            var value = (int)e.NewValue;
            t.But1.RepeatInterval = value;
            t.But2.RepeatInterval = value;
            t.But3.RepeatInterval = value;
            t.But4.RepeatInterval = value;
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Adjuster)d).Slider.Value = (int)e.NewValue;
        }

        private void OnReleased(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            PressedReleased?.Invoke(this, ChangeDirection.ZoomStop);
        }

        private void TeleFast_OnPressed(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            PressedReleased?.Invoke(this, ChangeDirection.TeleFast);
        }

        private void TeleFast_Repeat(object sender, EventArgs e)
        {
            RepeatClick?.Invoke(this, ChangeDirection.TeleFast);
        }

        private void TeleNormal_OnPressed(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            PressedReleased?.Invoke(this, ChangeDirection.TeleNormal);
        }

        private void TeleNormal_Repeat(object sender, EventArgs eventArgs)
        {
            RepeatClick?.Invoke(this, ChangeDirection.TeleNormal);
        }

        private void WideFast_OnPressed(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            PressedReleased?.Invoke(this, ChangeDirection.WideFast);
        }

        private void WideFast_Repeat(object sender, EventArgs e)
        {
            RepeatClick?.Invoke(this, ChangeDirection.WideFast);
        }

        private void WideNormal_OnPressed(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            PressedReleased?.Invoke(this, ChangeDirection.WideNormal);
        }

        private void WideNormal_Repeat(object sender, EventArgs e)
        {
            RepeatClick?.Invoke(this, ChangeDirection.WideNormal);
        }
    }
}