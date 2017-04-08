// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GMaster.Views
{
    using Tools;
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
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugCategoryList.Items?.Clear();
                foreach (var name in Debug.Categories.Keys)
                {
                    DebugCategoryList.Items?.Add(new DebugCategoryEnable { Name = name });
                }
            }
        }
    }
}