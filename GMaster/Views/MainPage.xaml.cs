// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GMaster.Views
{
    using System;
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

        private void CameraSettings_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ConnectedCamera camera)
            {
                OpenFrame(typeof(CameraSettingsPage), camera);
            }
        }

        private void OpenFrame(Type type, object model)
        {
            MenuFrame.Navigate(type);
            if (MenuFrame.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.DataContext = model;
            }

            MenuFrame.Visibility = Visibility.Visible;
            if (ActualWidth < 640)
            {
                MainMenu.DisplayMode = SplitViewDisplayMode.Overlay;
                MainMenu.IsPaneOpen = false;
            }
            else
            {
                MainMenu.DisplayMode = SplitViewDisplayMode.Inline;
            }
        }

        private void GeneralSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenFrame(typeof(GeneralSettingsPage), Model);
        }

        private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (MenuFrame.Content == null && !MainMenu.IsPaneOpen)
            {
                MainMenu.DisplayMode = SplitViewDisplayMode.Overlay;
                MainMenu.IsPaneOpen = true;
            }
            else
            {
                if (MenuFrame.Content == null)
                {
                    MainMenu.IsPaneOpen = false;
                }
                else
                {
                    MenuFrame.Content = null;
                    MenuFrame.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Model.View1.SelectedCamera = (ConnectedCamera)e.ClickedItem;
            MainMenu.IsPaneOpen = false;
            MenuFrame.Content = null;
            MenuFrame.Visibility = Visibility.Collapsed;
        }
    }
}