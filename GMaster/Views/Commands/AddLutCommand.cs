namespace GMaster.Views.Commands
{
    using System;
    using System.Threading.Tasks;
    using Logger;
    using Tools;
    using Windows.Storage;
    using Windows.Storage.Pickers;

    public class AddLutCommand : AbstractModelCommand<MainPageModel>
    {
        public AddLutCommand(MainPageModel model)
            : base(model)
        {
        }

        protected override bool InternalCanExecute() => true;

        protected override async Task InternalExecute()
        {
            try
            {
                var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.Downloads };
                picker.FileTypeFilter.Add(".cube");
                picker.FileTypeFilter.Add(".3dl");

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    var folder = await App.GetLutsFolder();
                    var newFile = await folder.CreateFileAsync(file.Name, CreationCollisionOption.ReplaceExisting);
                    await file.CopyAndReplaceAsync(newFile);

                    Model.InstalledLuts.Add(newFile);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}