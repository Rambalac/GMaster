// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

using GMaster.Views.Models;

namespace GMaster.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CameraSettingsPage : Page
    {
        public CameraSettingsPage()
        {
            InitializeComponent();
        }

        public ConnectedCamera Model => DataContext as ConnectedCamera;

        private void ForgerButton_OnClick(object sender, RoutedEventArgs e)
        {
            Model.Remove();
        }

        private void RememberButton_OnClick(object sender, RoutedEventArgs e)
        {
            Model.Add();
        }
    }
}