// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

using System;
using Windows.UI.Xaml.Navigation;

namespace GMaster.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private MainPageModel Model => DataContext as MainPageModel;

        private void MainMenu_OnPaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            MenuFrame.Content = null;
        }

        private void CameraSettings_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ConnectedCamera camera)
            {
                OpenFrame(typeof(CameraSettingsPage), camera);
            }
        }

        private void OpenFrame(Type type, object model)
        {
            MainMenu.DisplayMode = SplitViewDisplayMode.Inline;
            MenuFrame.Navigate(type);
            if (MenuFrame.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.DataContext = model;
            }
        }

        private void GeneralSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenFrame(typeof(GeneralSettingsPage), Model);
        }
    }
}