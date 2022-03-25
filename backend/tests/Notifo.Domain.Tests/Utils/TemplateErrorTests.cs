// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Notifo.Domain.Utils
{
    public class TemplateErrorTests
    {
        [Fact]
        public void Should_return_null_on_parse_when_message_is_null()
        {
            var result = TemplateError.Parse(null);

            Assert.Null(result);
        }

        [Fact]
        public void Should_return_null_on_parse_when_message_is_empty()
        {
            var result = TemplateError.Parse(string.Empty);

            Assert.Null(result);
        }

        [Fact]
        public void Should_extract_line_numbers()
        {
            var result = TemplateError.Parse("Error at (10:20)");

            Assert.Equal(new TemplateError("Error.", 10, 20), result);
        }

        [Fact]
        public void Should_extract_line_number_only()
        {
            var result = TemplateError.Parse("Error at (10)");

            Assert.Equal(new TemplateError("Error.", 10), result);
        }

        [Fact]
        public void Should_parse_without_line_numbers()
        {
            var result = TemplateError.Parse("Error Message");

            Assert.Equal(new TemplateError("Error Message."), result);
        }

        [Fact]
        public void Should_parse_with_invalid_line_numbers()
        {
            var result = TemplateError.Parse("Error Message at (Test Value)");

            Assert.Equal(new TemplateError("Error Message."), result);
        }

        [Fact]
        public void Should_parse_with_open_line_numbers()
        {
            var result = TemplateError.Parse("Error Message at (10:20");

            Assert.Equal(new TemplateError("Error Message at (10:20."), result);
        }
    }
}
