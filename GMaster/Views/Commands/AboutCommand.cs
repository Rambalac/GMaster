using System;

namespace GMaster.Views.Commands
{
    public class AboutCommand : AbstractSimpleCommand
    {
        protected override async void InternalExecute()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/Rambalac/GMaster"));
        }
    }
}