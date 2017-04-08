namespace GMaster.Views.Commands
{
    using System.Threading.Tasks;
    using Camera;
    using Tools;

    public class CameraDisconnectCommand : AbstractSimpleParameterCommand<Lumix>
    {
        public override async Task InternalExecute(Lumix parameter)
        {
            if (parameter != null)
            {
                await parameter.Disconnect();
            }
        }
    }
}