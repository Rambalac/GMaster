namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Core.Camera;
    using Core.Tools;
    using Models;
    using Tools;

    public class RecCommand : AbstractModelCommand<CameraViewModel>
    {
        protected override bool InternalCanExecute()
            => Model?.RecState != null && Model.RecState != RecState.Unknown && Model.RecState != RecState.StopNotSupported;

        protected override async Task InternalExecute()
        {
            var lumix = Model.SelectedCamera;
            if (lumix == null)
            {
                return;
            }

            try
            {
                if (lumix.Camera.RecState == RecState.Stopped)
                {
                    await lumix.Camera.RecStart();
                }
                else if (lumix.Camera.RecState == RecState.Started)
                {
                    await lumix.Camera.RecStop();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}