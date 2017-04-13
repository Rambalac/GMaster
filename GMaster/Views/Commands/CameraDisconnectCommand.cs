namespace GMaster.Views.Commands
{
    using System.Linq;
    using System.Threading.Tasks;
    using Models;
    using Tools;

    public class CameraDisconnectCommand : AbstractParameterModelCommand<MainPageModel, ConnectedCamera>
    {
        protected override bool InternalCanExecute(ConnectedCamera parameter) => true;

        protected override async Task InternalExecute(ConnectedCamera parameter)
        {
            await parameter.Camera.Disconnect();
        }
    }
}