using GMaster.Core.Camera.Panasonic.LumixData;

namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Core.Tools;
    using Models;
    using Tools;

    public class MfAssistPinpCommand : AbstractModelCommand<CameraViewModel>
    {
        protected override bool InternalCanExecute() => true;

        protected override async Task InternalExecute()
        {
            var lumix = Model.SelectedCamera;
            if (lumix == null)
            {
                return;
            }

            try
            {
                if (lumix.Camera.LumixState.FocusMode == FocusMode.MF)
                {
                    await lumix.Camera.MfAssistPinp(false);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}