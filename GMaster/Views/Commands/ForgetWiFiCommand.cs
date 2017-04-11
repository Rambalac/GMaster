using GMaster.Views.Models;

namespace GMaster.Views.Commands
{
    using System.Threading.Tasks;
    using Tools;

    public class ForgetWiFiCommand : AbstractParameterModelCommand<MainPageModel, string>
    {
        protected override bool InternalCanExecute(string parameter) => true;

        protected override Task InternalExecute(string parameter)
        {
            Model.WifiAutoconnectAccessPoints.Remove(parameter);
            return Task.CompletedTask;
        }
    }
}