// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GMaster.Views
{
    using System.Linq;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media;

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            AddHandler(PointerPressedEvent, new PointerEventHandler(PressHandler), true);
            Model.Dispatcher = Dispatcher;
            CheckOrientation();
        }

        private MainPageModel Model => DataContext as MainPageModel;

        private void HideMenu()
        {
            MenuFrame.Content = null;
            MenuFrame.Visibility = Visibility.Collapsed;
            MainMenu.IsPaneOpen = false;
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
                HideMenu();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Model.Init();
        }

        private void PressHandler(object sender, PointerRoutedEventArgs args)
        {
            if (MainMenu.IsPaneOpen)
            {
                var el1 = VisualTreeHelper.FindElementsInHostCoordinates(args.GetCurrentPoint(MainMenu).Position, MainMenu.Pane, false).Any();
                var el2 = VisualTreeHelper.FindElementsInHostCoordinates(args.GetCurrentPoint(MainMenu).Position, MainMenu.Content, false).Any();
                if (!el1 && !el2)
                {
                    HideMenu();
                }
            }
        }

        private void ViewsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CheckOrientation();
        }

        private void CheckOrientation()
        {
            var currentView = ApplicationView.GetForCurrentView();

            Model.IsLandscape = currentView.Orientation == ApplicationViewOrientation.Landscape;
        }
    }
}