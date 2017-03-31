using Windows.UI.ViewManagement;

namespace GMaster.Views.Commands
{
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