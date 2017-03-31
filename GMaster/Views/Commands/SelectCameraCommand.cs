using Windows.UI.Xaml.Controls;

namespace GMaster.Views.Commands
{
    public class SelectCameraCommand
    {
        private readonly MainPageModel model;

        public SelectCameraCommand(MainPageModel model)
        {
            this.model = model;
        }

        public void Handler(object sender, ItemClickEventArgs args)
        {
            model.View1.SelectedCamera = args.ClickedItem as ConnectedCamera;
            model.IsMainMenuOpened = false;
        }
    }
}