// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.TestHelpers;
using Xunit;

namespace Notifo.Infrastructure.MongoDb
{
    public class SoftEnumSerializerTests
    {
        public enum MyEnum
        {
            A,
            B
        }

        public SoftEnumSerializerTests()
        {
            SoftEnumSerializer<MyEnum>.Register();
        }

        [Theory]
        [InlineData(null, MyEnum.A)]
        [InlineData("A", MyEnum.A)]
        [InlineData("B", MyEnum.B)]
        [InlineData("C", MyEnum.A)]
        [InlineData("", MyEnum.A)]
        public void Should_deserialize_from_string(string source, MyEnum expected)
        {
            var serialized = source.SerializeAndDeserializeBson<string, MyEnum>();

            Assert.Equal(expected, serialized);
        }
    }
}
