// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GMaster.Views
{
    using System.Diagnostics;
    using Core.Tools;
    using Models;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GeneralSettingsPage : Page
    {
        public GeneralSettingsPage()
        {
            InitializeComponent();
        }

        private void DebugCategoryList_Loaded(object sender, RoutedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                DebugCategoryList.Items?.Clear();
                foreach (var name in Core.Tools.Debug.Categories.Keys)
                {
                    DebugCategoryList.Items?.Add(new DebugCategoryEnable { Name = name });
                }
            }
        }

        private void CopyLog_OnClick(object sender, RoutedEventArgs e)
        {
            var str = Log.GetInmemoryMessages();
            var package = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            package.SetText(str);
            Clipboard.SetContent(package);
        }
    }
}