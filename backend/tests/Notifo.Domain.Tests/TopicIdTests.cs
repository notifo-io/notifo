// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Notifo.Domain
{
    public class TopicIdTests
    {
        [Fact]
        public void Should_be_valid_for_single_part_topic()
        {
            Assert.True(TopicId.IsValid("topic"));
        }

        [Fact]
        public void Should_be_valid_for_multi_part_topic()
        {
            Assert.True(TopicId.IsValid("part1/part2"));
        }

        [Fact]
        public void Should_be_valid_when_parts_have_whitespaces()
        {
            Assert.True(TopicId.IsValid("part1 plus/part2"));
        }

        [Fact]
        public void Should_be_valid_when_parts_have_special_characters()
        {
            Assert.True(TopicId.IsValid("part-one/part-two"));
        }

        [Fact]
        public void Should_not_be_valid_when_part_has_double_slash()
        {
            Assert.False(TopicId.IsValid("part-one//part-two"));
        }

        [Fact]
        public void Should_not_be_valid_when_part_has_newline()
        {
            Assert.False(TopicId.IsValid("part\none/part-two"));
        }

        [Fact]
        public void Should_not_be_valid_when_part_has_dollar()
        {
            Assert.False(TopicId.IsValid("part$/part-two"));
        }
    }
}
