// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Notifo.Infrastructure.TestHelpers;
using Xunit;

namespace Notifo.Infrastructure.Collections
{
    public class ReadonlyDictionaryTests
    {
        [Fact]
        public void Should_return_empty_instance_for_empty_source()
        {
            var result = new Dictionary<int, int>().ToImmutableDictionary();

            Assert.Same(ReadonlyDictionary.Empty<int, int>(), result);
        }

        [Fact]
        public void Should_return_empty_instance_for_empty_source_and_key_selector()
        {
            var result = Enumerable.Empty<int>().ToImmutableDictionary(x => x);

            Assert.Same(ReadonlyDictionary.Empty<int, int>(), result);
        }

        [Fact]
        public void Should_return_empty_instance_for_empty_source_and_value_selector()
        {
            var result = Enumerable.Empty<int>().ToImmutableDictionary(x => x, x => x);

            Assert.Same(ReadonlyDictionary.Empty<int, int>(), result);
        }

        [Fact]
        public void Should_make_correct_object_equal_comparisons()
        {
            var obj1a = new Dictionary<int, int>
            {
                [1] = 1
            }.ToImmutableDictionary();

            var obj1b = new Dictionary<int, int>
            {
                [1] = 1
            }.ToImmutableDictionary();

            var dictionaryOtherValue = new Dictionary<int, int>
            {
                [1] = 2
            }.ToImmutableDictionary();

            var dictionaryOtherKey = new Dictionary<int, int>
            {
                [2] = 1
            }.ToImmutableDictionary();

            var dictionaryOtherCount = new Dictionary<int, int>
            {
                [1] = 1,
                [2] = 2
            }.ToImmutableDictionary();

            Assert.Equal(obj1a, obj1b);
            Assert.Equal(obj1a.GetHashCode(), obj1b.GetHashCode());
            Assert.True(obj1a.Equals((object)obj1b));

            Assert.NotEqual(obj1a, dictionaryOtherValue);
            Assert.NotEqual(obj1a.GetHashCode(), dictionaryOtherValue.GetHashCode());
            Assert.False(obj1a.Equals((object)dictionaryOtherValue));

            Assert.NotEqual(obj1a, dictionaryOtherKey);
            Assert.NotEqual(obj1a.GetHashCode(), dictionaryOtherKey.GetHashCode());
            Assert.False(obj1a.Equals((object)dictionaryOtherKey));

            Assert.NotEqual(obj1a, dictionaryOtherCount);
            Assert.NotEqual(obj1a.GetHashCode(), dictionaryOtherCount.GetHashCode());
            Assert.False(obj1a.Equals((object)dictionaryOtherCount));
        }

        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var sut = new Dictionary<int, int>
            {
                [11] = 1,
                [12] = 2,
                [13] = 3
            }.ToImmutableDictionary();

            var serialized = sut.SerializeAndDeserialize();

            Assert.Equal(sut, serialized);
        }

        [Fact]
        public void Should_serialize_and_deserialize_bson()
        {
            var sut = new Dictionary<string, int>
            {
                ["11"] = 1,
                ["12"] = 2,
                ["13"] = 3
            }.ToImmutableDictionary();

            var serialized = sut.SerializeAndDeserializeBson();

            Assert.Equal(sut, serialized);
        }
    }
}
