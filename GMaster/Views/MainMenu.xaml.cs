namespace GMaster.Views
{
    using System;
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
            Model.View1.SelectedCamera = (ConnectedCamera)e.ClickedItem;
            MainMenu.IsPaneOpen = false;
            MenuFrame.Content = null;
            MenuFrame.Visibility = Visibility.Collapsed;
        }

        private void OpenFrame(Type type, object model)
        {
            MenuFrame.Navigate(type);
            if (MenuFrame.Content is FrameworkElement frameworkElement)
            {
                frameworkElement.DataContext = model;
            }

            MenuFrame.Visibility = Visibility.Visible;
            if (MainMenu.ActualWidth < 640)
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
    }
}