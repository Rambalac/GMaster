namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Core.Camera;
    using Core.Tools;
    using Core.Camera.Panasonic;
    using Core.Camera.Panasonic.LumixData;
    using Models;
    using Tools;

    public class FocusPosResetCommand : AbstractModelCommand<CameraViewModel>
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
                var floatPoint = new FloatPoint(0.5f, 0.5f);
                if (lumix.Camera.LumixState.FocusMode == FocusMode.MF)
                {
                    await lumix.Camera.MfAssistMove(PinchStage.Single, floatPoint);
                }
                else
                {
                    await lumix.Camera.FocusPointMove(floatPoint);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}