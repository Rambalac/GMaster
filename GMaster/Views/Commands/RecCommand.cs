namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Camera;
    using Tools;

    public class RecCommand : AbstractModelCommand<CameraViewModel>
    {
        public RecCommand(CameraViewModel model)
            : base(model)
        {
        }

        protected override bool InternalCanExecute()
            => Model.SelectedCamera != null && Model.SelectedCamera.Camera.RecState != RecState.Unknown;

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