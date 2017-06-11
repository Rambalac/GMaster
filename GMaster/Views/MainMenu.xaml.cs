namespace GMaster.Views
{
    using System;
    using System.Threading.Tasks;
    using Models;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public partial class MainMenuPane : UserControl
    {
        public static readonly DependencyProperty MainMenuProperty = DependencyProperty.Register(
            "MainMenu", typeof(SplitView), typeof(MainMenuPane), new PropertyMetadata(default(SplitView)));

        public static readonly DependencyProperty MenuFrameProperty = DependencyProperty.Register(
            "MenuFrame", typeof(Frame), typeof(MainMenuPane), new PropertyMetadata(default(Frame)));

        public MainMenuPane()
        {
            InitializeComponent();
        }

        public SplitView MainMenu
        {
            get => (SplitView)GetValue(MainMenuProperty);
            set => SetValue(MainMenuProperty, value);
        }

        public Frame MenuFrame
        {
            get => (Frame)GetValue(MenuFrameProperty);
            set => SetValue(MenuFrameProperty, value);
        }

        private MainPageModel Model => DataContext as MainPageModel;

        private void CameraSettings_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ConnectedCamera camera)
            {
                OpenFrame(typeof(CameraSettingsPage), camera);
            }
        }

        private void GeneralSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenFrame(typeof(GeneralSettingsPage), Model);
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var camera = (ConnectedCamera)e.ClickedItem;
            if (!camera.Camera.LumixState.IsBusy)
            {
                Model.ShowCamera(camera);
                MainMenu.IsPaneOpen = false;
                MenuFrame.Content = null;
                MenuFrame.Visibility = Visibility.Collapsed;
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
            if (((MainMenu.Parent as FrameworkElement)?.ActualWidth ?? 0) < 640)
            {
                MainMenu.DisplayMode = SplitViewDisplayMode.Overlay;
                MainMenu.IsPaneOpen = false;
                MenuFrame.Padding = new Thickness(0, 30, 0, 0);
            }
            else
            {
                MainMenu.DisplayMode = SplitViewDisplayMode.Inline;
                MenuFrame.Padding = new Thickness(0);
            }
        }

        private void WiFi_Click(object sender, RoutedEventArgs e)
        {
            OpenFrame(typeof(WiFiPage), Model);
        }

        private void ViewOne_OnClick(object sender, RoutedEventArgs e)
        {
            Model.SplitMode = SplitMode.One;
        }

        private void ViewHorizontal_OnClick(object sender, RoutedEventArgs e)
        {
            Model.SplitMode = SplitMode.Horizontal;
        }

        private void ViewVertical_OnClick(object sender, RoutedEventArgs e)
        {
            Model.SplitMode = SplitMode.Vertical;
        }

        private void ViewFour_OnClick(object sender, RoutedEventArgs e)
        {
            Model.SplitMode = SplitMode.Four;
        }

        private void ListViewBase_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.Count != 1)
            {
                e.Cancel = true;
                return;
            }

            e.Data.Properties.Add("camera", e.Items[0]);

            MainMenu.IsPaneOpen = false;
            MenuFrame.Content = null;
            MenuFrame.Visibility = Visibility.Collapsed;
        }

        private async void NewWindow_OnClick(object sender, RoutedEventArgs e)
        {
            await OpenNewWindow(null);
        }

        private async void NewWindowCam_Click(object sender, RoutedEventArgs e)
        {
            await OpenNewWindow((ConnectedCamera)((FrameworkElement)sender).DataContext);
        }

        private async Task OpenNewWindow(ConnectedCamera cam)
        {
            var newView = CoreApplication.CreateNewView();
            var newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var newpage = new NewWindowsPage();
                newpage.SelectCamera(cam);
                Window.Current.Content = newpage;
                Window.Current.Activate();

                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }
    }
}