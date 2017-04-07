namespace GMaster
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Logger;
    using Views;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.ApplicationModel.Resources;
    using Windows.Storage;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    public partial class App : Application
    {
        private static readonly StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
        private static readonly ResourceLoader Strings = new ResourceLoader();
        private static CoreDispatcher dispatcher;

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
        }

        public MainPageModel MainModel { get; private set; }

        public static async Task<StorageFolder> GetLutsFolder()
        {
            return await LocalFolder.CreateFolderAsync("Luts", CreationCollisionOption.OpenIfExists);
        }

        public static string GetString(string id) => Strings.GetString(id);

        public static Task RunAsync(DispatchedHandler action)
        {
            try
            {
                return dispatcher.RunAsync(CoreDispatcherPriority.Normal, action).AsTask();
            }
            catch (Exception)
            {
                // throw;
                return Task.CompletedTask;
            }
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            MainModel = Resources[nameof(MainModel)] as MainPageModel;
            if (MainModel != null)
            {
                await MainModel.StartListening();
            }

#if DEBUG
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif
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

            dispatcher = Window.Current.Dispatcher;
            if (dispatcher == null)
            {
                throw new NullReferenceException("Null dispatcher");
            }
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception);
#if !DEBUG
            e.Handled = true;
#endif
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
            var deferral = e.SuspendingOperation.GetDeferral();
            MainModel.StopListening();
            deferral.Complete();
        }
    }
}