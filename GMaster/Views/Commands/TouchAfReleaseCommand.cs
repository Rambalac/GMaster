namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Core.Tools;
    using Models;
    using Tools;

    public class TouchAfReleaseCommand : AbstractModelCommand<CameraViewModel>
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
                await lumix.Camera.ReleaseTouchAF();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}