namespace GMasterTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using GMaster.Camera;
    using GMaster.Camera.LumixData;
    using Windows.ApplicationModel;
    using Xunit;

    public class MenuSetHelperTestsGh4
    {
        private MenuSet menuset;

        public MenuSetHelperTestsGh4()
        {
            Load().GetAwaiter().GetResult();
        }

        public async Task Load()
        {
            var file = await Package.Current.InstalledLocation.GetFileAsync("TestMenuSetGH4.xml");
            using (var stream = await file.OpenSequentialReadAsync())
            {
                var serializer = new XmlSerializer(typeof(MenuSetRuquestResult));
                var result = (MenuSetRuquestResult)serializer.Deserialize(stream.AsStreamForRead());
                CameraParser.TryParseMenuSet(result.MenuSet, "en", out menuset);
            }
        }

        [Fact]
        public void TestLiveviewQualiyty()
        {
            Assert.Equal(2, menuset.LiveviewQuality.Count);
            Assert.True(menuset.LiveviewQuality.Any(q => q.Value == "vga"));
        }
    }
}
