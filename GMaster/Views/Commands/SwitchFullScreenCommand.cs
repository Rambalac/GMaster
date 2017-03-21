namespace GMaster.Views.Commands
{
    using Windows.UI.ViewManagement;

    public class SwitchFullScreenCommand : AbstractSimpleCommand
    {
        protected override void InternalExecute()
        {
            var view = ApplicationView.GetForCurrentView();
            if (!view.IsFullScreenMode)
            {
                view.TryEnterFullScreenMode();
            }
            else
            {
                view.ExitFullScreenMode();
            }
        }
    }
}