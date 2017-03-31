using System.Diagnostics;
using GMaster.Camera;

namespace GMaster.Views.Commands
{
    public class CameraDisconnectCommand : AbstractSimpleParameterCommand
    {
        public override async void Execute(object parameter)
        {
            var lumix = parameter as Lumix;
            Debug.Assert(lumix != null, "lumix != null");
            await lumix.Disconnect();
        }
    }
}