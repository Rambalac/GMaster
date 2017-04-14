namespace GMaster
{
    using System;
    using System.Threading.Tasks;
    using Core.Tools;
    using Views;
    using Views.Models;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.ApplicationModel.Resources;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    public partial class App : Application
    {
        private static readonly StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
        private static readonly ResourceLoader Strings = new ResourceLoader();

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            UnhandledException += App_UnhandledException;

            Debug.AddCategory("OffFrameBytes", false);
        }

        public MainPageModel MainModel { get; private set; }

        public static async Task<StorageFolder> GetLutsFolder()
        {
            return await LocalFolder.CreateFolderAsync("Luts", CreationCollisionOption.OpenIfExists);
        }

        public static string GetString(string id) => Strings.GetString(id);

        [System.Diagnostics.Conditional("DEBUG")]
        public void IfDebug(Action action) => action();

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            var ver = Package.Current.Id.Version;

            Log.Init(new WindowsHttpClient(), "deb4bd35-6ddd-4044-b3e8-ac76330e559b", $"{ver.Major}.{ver.Minor}.{ver.Build}", 500);

            MainModel = Resources[nameof(MainModel)] as MainPageModel;

            if (MainModel != null)
            {
                await MainModel.StartListening();
            }

            IfDebug(() =>
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    DebugSettings.EnableFrameRateCounter = true;
                }
            });

            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            switch (e.Exception)
            {
                case ObjectDisposedException ex:
                    Debug.WriteLine("ObjectDisposedException in " + ex.Source, "UnhandledException");
                    break;

                default:
                    Log.Error(e.Exception, "Unhandled");
                    Log.Flush();
                    break;
            }

            IfDebug(() => e.Handled = true);
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            Log.Stop();

            var deferral = e.SuspendingOperation.GetDeferral();
            MainModel.StopListening();
            deferral.Complete();
        }
    }
}