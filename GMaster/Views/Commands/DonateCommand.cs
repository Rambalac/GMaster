using System;
using Windows.UI.Popups;

namespace GMaster.Views.Commands
{
    using System.Threading.Tasks;
    using Tools;

    public class DonateCommand : AbstractParameterModelCommand<MainPageModel, string>
    {
        public DonateCommand(MainPageModel model)
            : base(model)
        {
        }

        protected override bool InternalCanExecute(string parameter) => true;

        protected override async Task InternalExecute(string parameter)
        {
            var messageid = await Model.Donations.PurchaseAddOn(parameter) ? "Donate_Thankyou" : "Donate_Error";
            var dialog = new MessageDialog(App.GetString(messageid));
            await dialog.ShowAsync();
        }
    }
}