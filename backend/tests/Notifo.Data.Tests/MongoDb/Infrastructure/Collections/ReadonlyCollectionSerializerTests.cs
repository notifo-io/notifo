// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Collections;
using Notifo.MongoDb.TestHelpers;

namespace Notifo.MongoDb.Infrastructure.Collections;

public class ReadonlyCollectionSerializerTests
{
    [Fact]
    public void Should_serialize_and_deserialize_list()
    {
        var sut = ReadonlyList.Create(1, 2, 3);

        var serialized = sut.SerializeAndDeserializeBson();

        Assert.Equal(sut, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_dictionary()
    {
        var sut = new Dictionary<string, int>
        {
            ["11"] = 1,
            ["12"] = 2,
            ["13"] = 3
        }.ToReadonlyDictionary();

        var serialized = sut.SerializeAndDeserializeBson();

        Assert.Equal(sut, serialized);
    }
}
