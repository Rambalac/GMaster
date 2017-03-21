namespace GMaster.Views.Commands
{
    using System.Diagnostics;
    using Windows.UI.Xaml.Controls;

    public class SelectCameraCommand
    {
        private readonly MainPageModel model;

        public SelectCameraCommand(MainPageModel model)
        {
            this.model = model;
        }

        public void Handler(object sender, ItemClickEventArgs args)
        {
            var select = args.ClickedItem as ConnectedCamera;
            Debug.Assert(select != null, "select != null");
            model.View1.SelectedCamera = @select.Camera;
            model.IsMainMenuOpened = false;
        }
    }
}