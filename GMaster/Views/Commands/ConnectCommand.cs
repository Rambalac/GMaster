namespace GMaster.Views.Commands
{
    using System.Threading.Tasks;
    using Tools;

    public class ConnectCommand : AbstractModelCommand<MainPageModel>
    {
        public ConnectCommand(MainPageModel model)
            : base(model)
        {
        }

        protected override bool InternalCanExecute() => Model.SelectedDevice != null;

        protected override async Task InternalExecute()
        {
            await Model.ConnectCamera(Model.SelectedDevice);
        }
    }
}