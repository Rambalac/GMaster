using System;
using System.Windows.Input;
using Windows.UI.ViewManagement;

namespace GMaster.Views
{
    public class SwitchFullScreenCommand : ICommand
    {
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            var view = ApplicationView.GetForCurrentView();
            if (!view.IsFullScreenMode)
                view.TryEnterFullScreenMode();
            else
                view.ExitFullScreenMode();
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}