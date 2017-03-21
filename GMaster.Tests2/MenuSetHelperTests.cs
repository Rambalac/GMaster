
using System;
using System.IO;
using System.Xml.Serialization;
using GMaster.Camera;
using GMaster.Camera.LumixResponces;
using Xunit;

namespace GMasterTests
{
    public class MenuSetHelperTests
    {
        private MenuSetHelper menuset;

        public MenuSetHelperTests()
        {
            var serializer = new XmlSerializer(typeof(MenuSetRuquestResult));
            using (var stream = File.OpenRead("TestMenuSet.xml"))
            {
                var result = (MenuSetRuquestResult)serializer.Deserialize(stream);
                menuset = new MenuSetHelper(result.MenuSet);
            }
        }

        [Fact]
        public void TestMethod1()
        {

        }
    }
}
