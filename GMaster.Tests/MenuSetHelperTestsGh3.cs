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

    public class MenuSetHelperTests
    {
        private MenuSet menuset;

        public async Task Load(string filename)
        {
            var file = await Package.Current.InstalledLocation.GetFileAsync(filename);
            using (var stream = await file.OpenSequentialReadAsync())
            {
                var serializer = new XmlSerializer(typeof(MenuSetRuquestResult));
                var result = (MenuSetRuquestResult)serializer.Deserialize(stream.AsStreamForRead());
                CameraParser.TryParseMenuSet(result.MenuSet, "en", out menuset);
            }
        }

        [Theory]
        [InlineData("TestMenuSetGH3.xml")]
        [InlineData("TestMenuSetGH3_M.xml")]
        [InlineData("TestMenuSetGH3_S.xml")]
        [InlineData("TestMenuSetGH4.xml")]
        [InlineData("TestMenuSetGH5.xml")]
        public async Task TestLiveviewQualiyty(string filename)
        {
            await Load(filename);
            Assert.Equal(2, menuset.LiveviewQuality.Count);
            Assert.True(menuset.LiveviewQuality.Any(q => q.Value == "vga"));
        }

        [Fact]
        public async Task TestGH5()
        {
            var file = await Package.Current.InstalledLocation.GetFileAsync("TestMenuSetGH5.xml");
            using (var stream = await file.OpenSequentialReadAsync())
            {
                var serializer = new XmlSerializer(typeof(MenuSetRuquestResult));
                var result = (MenuSetRuquestResult)serializer.Deserialize(stream.AsStreamForRead());
                CameraParser.TryParseMenuSet(result.MenuSet, "en", out menuset, new CameraParser[] { new GH4Parser() });
            }

            Assert.Equal(2, menuset.LiveviewQuality.Count);
            Assert.True(menuset.LiveviewQuality.Any(q => q.Value == "vga"));
        }
    }
}