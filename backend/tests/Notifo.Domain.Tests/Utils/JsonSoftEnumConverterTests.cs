// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Utils;

public class JsonSoftEnumConverterTests
{
    public enum MyEnum
    {
        A,
        B
    }

    [Theory]
    [InlineData(null, MyEnum.A)]
    [InlineData("A", MyEnum.A)]
    [InlineData("B", MyEnum.B)]
    [InlineData("C", MyEnum.A)]
    [InlineData("", MyEnum.A)]
    public void Should_deserialize_from_string(string source, MyEnum expected)
    {
        var serialized = Infrastructure.TestHelpers.Deserialize<MyEnum>(source, new JsonSoftEnumConverter<MyEnum>());

        Assert.Equal(expected, serialized);
    }
}
