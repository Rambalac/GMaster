namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Core.Camera.LumixData;
    using Core.Tools;
    using Models;
    using Tools;

    public class ManualFocusAfCommand : AbstractModelCommand<CameraViewModel>
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
                if (lumix.Camera.LumixState.FocusMode == FocusMode.Manual)
                {
                    await lumix.Camera.MfAssistAf();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    public class MFAssistPinpCommand : AbstractModelCommand<CameraViewModel>
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
                if (lumix.Camera.LumixState.FocusMode == FocusMode.Manual)
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