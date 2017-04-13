using System.Linq;
using GMaster.Core.Camera;
using GMaster.Core.Camera.LumixData;
using GMaster.Core.Tools;
using Newtonsoft.Json;

namespace Gmaster.Core.Camera
{
    using Xunit;
    using System.IO;
    using System.Xml.Serialization;

    public class MenuSetHelperTests
    {
        private MenuSet menuset;

        public void Load(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                var serializer = new XmlSerializer(typeof(MenuSetRequestResult));
                var result = (MenuSetRequestResult)serializer.Deserialize(stream);
                CameraParser.TryParseMenuSet(result.MenuSet, "en", out menuset);
            }
        }

        [Theory]
        [InlineData("TestMenuSetGH3.xml")]
        [InlineData("TestMenuSetGH3_M.xml")]
        [InlineData("TestMenuSetGH3_S.xml")]
        [InlineData("TestMenuSetGH4.xml")]
        [InlineData("TestMenuSetGH5.xml")]
        public void TestLiveviewQualiyty(string filename)
        {
            Load(filename);
            Assert.Equal(2, menuset.LiveviewQuality.Count);
            Assert.True(menuset.LiveviewQuality.Any(q => q.Value == "vga"));
        }

        [Fact]
        public void TestGH5()
        {
            using (var stream = File.OpenRead("TestMenuSetGH5.xml"))
            {
                var serializer = new XmlSerializer(typeof(MenuSetRequestResult));
                var result = (MenuSetRequestResult)serializer.Deserialize(stream);
                CameraParser.TryParseMenuSet(result.MenuSet, "en", out menuset, new CameraParser[] { new GH4Parser() });
            }

            Assert.Equal(2, menuset.LiveviewQuality.Count);
            Assert.True(menuset.LiveviewQuality.Any(q => q.Value == "vga"));
        }
        private class LogglyMessage
        {
            [JsonProperty(Order = 2)]
            public string Data { get; set; }

            [JsonProperty(Order = 1)]
            public string Message { get; set; }
        }

        [Theory]
        [InlineData("TestMenuSetG80.json")]
        public void TestJsons(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                var reader = new StreamReader(stream);
                var str = reader.ReadToEnd();
                var obj = JsonConvert.DeserializeObject<LogglyMessage>(str);
                var serializer = new XmlSerializer(typeof(MenuSetRequestResult));
                var result = (MenuSetRequestResult)serializer.Deserialize(new StringReader(obj.Data));
                CameraParser.TryParseMenuSet(result.MenuSet, "en", out menuset, new CameraParser[] { new GH4Parser() });
            }

            Assert.Equal(2, menuset.LiveviewQuality.Count);
            Assert.True(menuset.LiveviewQuality.Any(q => q.Value == "vga"));

        }
    }
}