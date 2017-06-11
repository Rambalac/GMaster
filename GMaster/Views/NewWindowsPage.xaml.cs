// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GMaster.Views
{
    using Models;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewWindowsPage : Page
    {
        public NewWindowsPage()
        {
            InitializeComponent();
        }

        public void SelectCamera(ConnectedCamera cam)
        {
            CameraView.Model.SelectedCamera = cam;
        }
    }
}
