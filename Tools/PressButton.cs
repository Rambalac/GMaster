namespace Tools
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public class PressButton : Button
    {
        public PressButton()
        {
            AddHandler(PointerPressedEvent, new PointerEventHandler(PressPointerPressed), true);
            AddHandler(PointerReleasedEvent, new PointerEventHandler(PressPointerReleased), true);
        }

        public event RoutedEventHandler Pressed;

        public event RoutedEventHandler Released;

        private void PressPointerReleased(object sender, RoutedEventArgs e)
        {
            Released?.Invoke(this, new RoutedEventArgs());
        }

        private void PressPointerPressed(object sender, RoutedEventArgs e)
        {
            Pressed?.Invoke(this, new RoutedEventArgs());
        }
    }
}
