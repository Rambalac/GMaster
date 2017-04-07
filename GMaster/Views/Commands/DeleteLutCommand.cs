namespace GMaster.Views.Commands
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Logger;
    using Tools;
    using Windows.Storage;

    public class DeleteLutCommand : AbstractParameterModelCommand<MainPageModel, LutInfo>
    {
        protected override bool InternalCanExecute(LutInfo parameter) => true;

        protected override async Task InternalExecute(LutInfo parameter)
        {
            var folder = await App.GetLutsFolder();
            var files = await folder.GetFilesAsync();
            foreach (var file in files.Where(f => f.DisplayName == parameter.Id))
            {
                try
                {
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            Model.InstalledLuts.Remove(parameter);
        }
    }
}