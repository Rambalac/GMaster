using System.Diagnostics;

namespace GMaster.Views.Commands
{
    public class OpenSplitViewCommand : AbstractSimpleParameterCommand
    {
        public override void Execute(object parameter)
        {
            var model = parameter as MainPageModel;
            Debug.Assert(model != null, "splitview != null");
            model.IsMainMenuOpened = !model.IsMainMenuOpened;
        }
    }
}
