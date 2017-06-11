using GMaster.Core.Camera.Panasonic.LumixData;

namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Core.Tools;
    using Models;
    using Tools;

    public class RecCommand : AbstractModelCommand<CameraViewModel>
    {
        protected override bool InternalCanExecute() =>
            Model?.RecState != null && Model.RecState != RecState.Unknown && Model.RecState != RecState.StopNotSupported;

        protected override async Task InternalExecute()
        {
            var lumix = Model.SelectedCamera;
            if (lumix == null)
            {
                return;
            }

            try
            {
                switch (lumix.Camera.LumixState.RecState)
                {
                    case RecState.Stopped:
                        await lumix.Camera.RecStart();
                        break;
                    case RecState.Started:
                        await lumix.Camera.RecStop();
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}