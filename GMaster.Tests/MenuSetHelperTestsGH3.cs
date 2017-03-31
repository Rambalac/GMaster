using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using GMaster.Camera;
using GMaster.Camera.LumixResponces;
using Xunit;

namespace GMasterTests
{
    public class MenuSetHelperTestsGh3
    {
        private MenuSet menuset;

        public async Task Load(string filename)
        {
            var file = await Package.Current.InstalledLocation.GetFileAsync(filename);
            using (var stream = await file.OpenSequentialReadAsync())
            {
                var serializer = new XmlSerializer(typeof(MenuSetRuquestResult));
                var result = (MenuSetRuquestResult)serializer.Deserialize(stream.AsStreamForRead());
                menuset = MenuSet.TryParseMenuSet(result.MenuSet, "en");
            }
        }

        [Theory]
        [InlineData("TestMenuSetGH3.xml")]
        [InlineData("TestMenuSetGH3_M.xml")]
        [InlineData("TestMenuSetGH3_S.xml")]
        public async Task TestLiveviewQualiyty(string filename)
        {
            await Load(filename);
            Assert.Equal(2, menuset.LiveviewQuality.Count);
            Assert.True(menuset.LiveviewQuality.Any(q => q.Value == "vga"));
        }
    }
}
