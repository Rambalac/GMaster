namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;

    public class CaptureCommand : AbstractModelCommand<CameraViewModel>
    {
        public CaptureCommand(CameraViewModel model)
            : base(model)
        {
        }

        protected override bool InternalCanExecute() => Model.SelectedCamera != null;

        protected override async Task InternalExecute()
        {
            var lumix = Model.SelectedCamera;
            if (lumix == null)
            {
                return;
            }

            try
            {
                await lumix.Capture();
            }
            catch (Exception e)
            {
                
            }
        }
    }
}