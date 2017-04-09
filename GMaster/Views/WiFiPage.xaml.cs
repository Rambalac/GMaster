// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GMaster.Views
{
    using Windows.Devices.WiFi;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WiFiPage : Page
    {
        public WiFiPage()
        {
            InitializeComponent();
        }

        private MainPageModel Model => DataContext as MainPageModel;

        private async void Connect_ItemClick(object sender, ItemClickEventArgs e)
        {
            await Model.ConnectAccessPoint((WiFiAvailableNetwork)e.ClickedItem);
        }

        private void RememberButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Model.WifiMakeCurrentAutoconnect();
        }
    }
}