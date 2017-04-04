namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Tools;

    public class ZoomCommand : AbstractParameterModelCommand<CameraViewModel, int>
    {
        public ZoomCommand(CameraViewModel model)
            : base(model)
        {
        }

        protected override bool InternalCanExecute(int parameter) => true;

        protected override Task InternalExecute(int parameter)
        {
            throw new NotImplementedException();
        }
    }
}