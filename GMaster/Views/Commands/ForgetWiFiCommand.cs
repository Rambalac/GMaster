namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Tools;

    public class ForgetWiFiCommand : AbstractParameterModelCommand<MainPageModel, string>
    {
        protected override bool InternalCanExecute(string parameter) => true;

        protected override Task InternalExecute(string parameter)
        {
            Model.Wifi.AutoconnectAccessPoints.Remove(parameter);
            return Task.CompletedTask;
        }
    }
}