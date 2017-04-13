namespace GMaster.Views
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Core.Tools;
    using Newtonsoft.Json;
    using Windows.Storage;
    using Windows.Storage.Streams;

    public class LutInfo : IStringIdItem
    {
        private static readonly Dictionary<string, ILutParser> LutParsers = new Dictionary<string, ILutParser>
        {
            { ".cube", new CubeLutParser() }
        };

        public string Id { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public static async Task<LutInfo> LoadfromFile(IRandomAccessStreamReference file)
        {
            using (var reader = new StreamReader((await file.OpenReadAsync()).AsStreamForRead()))
            {
                return JsonConvert.DeserializeObject<LutInfo>(reader.ReadToEnd());
            }
        }

        public async Task<Lut> LoadLut()
        {
            try
            {
                var storageFolder = await App.GetLutsFolder();
                var lutFile = await storageFolder.GetFileAsync(Id + Type);
                if (lutFile == null)
                {
                    return null;
                }

                if (LutParsers.TryGetValue(lutFile.FileType, out var parser))
                {
                    using (var stream = await lutFile.OpenReadAsync())
                    {
                        return await parser.Parse(stream.AsStreamForRead());
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        public async Task SaveToFile(StorageFolder folder, StorageFile lut = null)
        {
            var file = await folder.CreateFileAsync(Id + ".info", CreationCollisionOption.ReplaceExisting);
            var trans = await file.OpenTransactedWriteAsync();

            if (lut != null)
            {
                Type = lut.FileType;
                var lutFile = await folder.CreateFileAsync(Id + lut.FileType, CreationCollisionOption.ReplaceExisting);
                await lut.CopyAndReplaceAsync(lutFile);
            }

            if (Type == null)
            {
                throw new Exception("Lut is not set");
            }

            using (var reader = new StreamWriter(trans.Stream.AsStreamForWrite()))
            {
                await reader.WriteAsync(JsonConvert.SerializeObject(this));
            }

            await trans.CommitAsync();
        }

        public override string ToString() => Title;
    }
}