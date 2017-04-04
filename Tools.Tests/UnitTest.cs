namespace Tools.Tests
{
    using System.Collections.Generic;
    using Xunit;

    internal enum TestEnum
    {
        A = 1,

        [EnumException(5)]
        B = 2
    }

    public class EnumTests
    {
        [Fact]
        public void TestEnumException()
        {
            Assert.Equal(TestEnum.B, 2.ToEnum<TestEnum>());
            Assert.Equal(TestEnum.B, 5.ToEnum<TestEnum>());
            Assert.Throws<KeyNotFoundException>(() => 10.ToEnum<TestEnum>());
        }
    }
}
