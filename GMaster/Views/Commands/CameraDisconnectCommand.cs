namespace GMaster.Views.Commands
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Camera;
    using Tools;

    public class CameraDisconnectCommand : AbstractSimpleParameterCommand<Lumix>
    {
        public override async Task InternalExecute(Lumix parameter)
        {
            Debug.Assert(parameter != null, "lumix != null");
            await parameter.Disconnect();
        }
    }
}