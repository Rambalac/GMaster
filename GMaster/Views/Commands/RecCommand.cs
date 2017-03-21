namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Camera;

    public class RecCommand : AbstractModelCommand<CameraViewModel>
    {
        public RecCommand(CameraViewModel model)
            : base(model)
        {
        }

        protected override bool InternalCanExecute()
            => Model.SelectedCamera != null && Model.SelectedCamera.RecState != RecState.Unknown;

        protected override async Task InternalExecute()
        {
            var lumix = Model.SelectedCamera;
            if (lumix == null)
            {
                return;
            }

            try
            {
                if (lumix.RecState == RecState.Stopped)
                {
                    await lumix.RecStart();
                }
                else if (lumix.RecState == RecState.Started)
                {
                    await lumix.RecStop();
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}