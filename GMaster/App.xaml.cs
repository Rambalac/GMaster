using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GMaster.Views;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

namespace GMaster
{
    public partial class App : Application
    {
        private static CoreDispatcher dispatcher;

        public static TelemetryClient Telemetry;

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            var config = new TelemetryConfiguration
            {
                InstrumentationKey = "132c2cbe-e02d-4d36-85bf-efe3bc8ee3e6"
                //TelemetryChannel = new MyChannel()
            };

            Telemetry = new TelemetryClient(config);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ApplicationInsightsTraces(Telemetry)
                .CreateLogger();


            InitializeComponent();
            Suspending += OnSuspending;
            UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Telemetry.TrackException(e.Exception);
            Telemetry.Flush();
#if !DEBUG
            e.Handled = true;
#endif
        }

        /// <summary>
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (Debugger.IsAttached)
                DebugSettings.EnableFrameRateCounter = true;
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
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                // Ensure the current window is active
                Window.Current.Activate();
            }
            dispatcher = Window.Current.Dispatcher;
            if (dispatcher == null) throw new NullReferenceException("Null dispatcher");
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
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        public static Task RunAsync(DispatchedHandler action)
        {
            try
            {
                return dispatcher.RunAsync(CoreDispatcherPriority.Normal, action).AsTask();
            }
            catch (Exception)
            {
                //  throw;
                return Task.CompletedTask;
            }
        }
    }

    public class MyChannel : ITelemetryChannel
    {
        public void Dispose()
        {
        }

        public void Send(ITelemetry item)
        {
        }

        public void Flush()
        {
        }

        public bool? DeveloperMode { get; set; }
        public string EndpointAddress { get; set; }
    }
}