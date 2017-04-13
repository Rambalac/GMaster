namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Core.Tools;
    using Models;
    using Tools;
    using Windows.Storage.Pickers;

    public class AddLutCommand : AbstractModelCommand<MainPageModel>
    {
        protected override bool InternalCanExecute() => true;

        protected override async Task InternalExecute()
        {
            try
            {
                var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.Downloads };
                picker.FileTypeFilter.Add(".cube");

                // picker.FileTypeFilter.Add(".3dl");
                var files = await picker.PickMultipleFilesAsync();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        var lutinfo = new LutInfo
                        {
                            Title = file.DisplayName,
                            Id = Guid.NewGuid().ToString()
                        };
                        await lutinfo.SaveToFile(await App.GetLutsFolder(), file);

                        Model.InstalledLuts.Add(lutinfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}