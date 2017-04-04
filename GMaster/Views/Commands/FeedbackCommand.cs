namespace GMaster.Views.Commands
{
    using System;
    using Tools;

    public class FeedbackCommand : AbstractSimpleCommand
    {
        public override bool CanExecute(object parameter)
            => Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported();

        protected override async void InternalExecute()
        {
            await Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
        }
    }
}