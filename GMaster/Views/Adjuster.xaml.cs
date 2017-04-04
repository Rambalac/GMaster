// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GMaster.Views
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;

    public sealed partial class Adjuster : UserControl
    {
        public Adjuster()
        {
            this.InitializeComponent();
        }

        public event Action<int> RepeatClick;

        private void TeleFast_Pressed(object sender, RoutedEventArgs e)
        {
            RepeatClick?.Invoke(+2);
        }

        private void TeleNormal_Pressed(object sender, RoutedEventArgs e)
        {
            RepeatClick?.Invoke(+1);
        }

        private void WideNormal_Pressed(object sender, RoutedEventArgs e)
        {
            RepeatClick?.Invoke(-1);
        }

        private void WideFast_Pressed(object sender, RoutedEventArgs e)
        {
            RepeatClick?.Invoke(-2);
        }
    }
}
