namespace GMaster.Views.Commands
{
    using System.Threading.Tasks;
    using Models;
    using Tools;

    public class ConnectCommand : AbstractModelCommand<MainPageModel>
    {
        protected override bool InternalCanExecute() => Model.SelectedDevice != null;

        protected override async Task InternalExecute()
        {
            await Model.ConnectionsManager.ManuallyConnect(Model.SelectedDevice);
        }
    }
}