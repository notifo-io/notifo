// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Notifo.Domain
{
    public class TrackingTokenTests
    {
        [Fact]
        public void Should_parse_from_formatted_token()
        {
            var sourceToken = new TrackingToken(Guid.NewGuid(), "push", "123");
            var sourceString = sourceToken.ToParsableString();

            var result = TrackingToken.Parse(sourceString);

            Assert.Equal(result, sourceToken);
        }

        [Fact]
        public void Should_parse_from_formatted_token_without_device_identifier()
        {
            var sourceToken = new TrackingToken(Guid.NewGuid(), "push");
            var sourceString = sourceToken.ToParsableString();

            var result = TrackingToken.Parse(sourceString);

            Assert.Equal(result, sourceToken);
        }

        [Fact]
        public void Should_parse_from_formatted_token_without_channel()
        {
            var sourceToken = new TrackingToken(Guid.NewGuid());
            var sourceString = sourceToken.ToParsableString();

            var result = TrackingToken.Parse(sourceString);

            Assert.Equal(result, sourceToken);
        }

        [Fact]
        public void Should_parse_from_formatted_token_with_complex_device_identifier()
        {
            var sourceToken = new TrackingToken(Guid.NewGuid(), "web", "a|very|complex|token");
            var sourceString = sourceToken.ToParsableString();

            var result = TrackingToken.Parse(sourceString);

            Assert.Equal(result, sourceToken);
        }

        [Fact]
        public void Should_parse_from_guid()
        {
            var sourceToken = Guid.NewGuid();
            var sourceString = sourceToken.ToString();

            var result = TrackingToken.Parse(sourceString);

            Assert.Equal(result, new TrackingToken(sourceToken));
        }

        [Fact]
        public void Should_parse_and_override_channel_and_device_identifier_if_not_set()
        {
            var sourceToken = new TrackingToken(Guid.NewGuid());
            var sourceString = sourceToken.ToParsableString();

            var result = TrackingToken.Parse(sourceString, "push", "123");

            Assert.Equal(result, new TrackingToken(sourceToken.Id, "push", "123"));
        }

        [Fact]
        public void Should_not_override_channel_device_identifier()
        {
            var sourceToken = new TrackingToken(Guid.NewGuid(), "push", "123");
            var sourceString = sourceToken.ToParsableString();

            var result = TrackingToken.Parse(sourceString, "web", "456");

            Assert.Equal(result, sourceToken);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_not_throw_if_null_or_empty(string value)
        {
            var result = TrackingToken.Parse(value);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("1a")]
        public void Should_not_throw_if_invalid_value(string value)
        {
            var result = TrackingToken.Parse(value);

            Assert.False(result.IsValid);
        }
    }
}
