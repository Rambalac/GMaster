namespace Tools
{
    using System;
    using Windows.System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public class PressButton : Button
    {
        public static readonly DependencyProperty RepeatIntervalProperty = DependencyProperty.Register(
            "RepeatInterval", typeof(int), typeof(PressButton), new PropertyMetadata(200));

        private readonly DispatcherTimer repeatTimer = new DispatcherTimer();

        private int pressedKeys;

        public PressButton()
        {
            AddHandler(PointerPressedEvent, new PointerEventHandler(PressPointerPressed), true);
            AddHandler(PointerReleasedEvent, new PointerEventHandler(PressPointerReleased), true);
            AddHandler(KeyDownEvent, new KeyEventHandler(KeyPressed), true);
            repeatTimer.Tick += RepeatTimer_Tick;
        }

        public event RoutedEventHandler Pressed;

        public event RoutedEventHandler Released;

        public event RoutedEventHandler Repeat;

        public int RepeatInterval
        {
            get => (int)GetValue(RepeatIntervalProperty);
            set
            {
                SetValue(RepeatIntervalProperty, value);
                repeatTimer.Interval = TimeSpan.FromMilliseconds(value);
            }
        }

        private void CheckStart()
        {
            if (pressedKeys == 0)
            {
                repeatTimer.Start();
            }

            pressedKeys++;
        }

        private void CheckStop()
        {
            pressedKeys--;
            if (pressedKeys == 0)
            {
                repeatTimer.Stop();
            }
        }

        private void KeyPressed(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter || e.Key == VirtualKey.Space)
            {
                CheckStart();
            }
        }

        private void PressPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                CheckStart();
                Pressed?.Invoke(this, new RoutedEventArgs());
            }
        }

        private void PressPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                CheckStop();
                Released?.Invoke(this, new RoutedEventArgs());
            }
        }

        private void RepeatTimer_Tick(object sender, object e)
        {
            Repeat?.Invoke(this, null);
        }
    }
}
