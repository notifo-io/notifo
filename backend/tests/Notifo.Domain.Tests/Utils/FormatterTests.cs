// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Notifo.Infrastructure.Texts;
using Xunit;

namespace Notifo.Domain.Utils
{
    public class FormatterTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_return_same_object_if_string_not_valid(string input)
        {
            var properties = new Dictionary<string, string?>
            {
                ["var"] = "123"
            };

            var result = input.Format(properties);

            Assert.Same(input, result);
        }

        [Fact]
        public void Should_format_simple_var()
        {
            var input = "{{var}}";

            var properties = new Dictionary<string, string?>
            {
                ["var"] = "123"
            };

            var result = input.Format(properties);

            Assert.Equal("123", result);
        }

        [Fact]
        public void Should_format_simple_var_ignoring_case()
        {
            var input = "{{Var}}";

            var properties = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["var"] = "123"
            };

            var result = input.Format(properties);

            Assert.Equal("123", result);
        }

        [Fact]
        public void Should_format_var_with_dot()
        {
            var input = "{{var.value}}";

            var properties = new Dictionary<string, string?>
            {
                ["var.value"] = "123"
            };

            var result = input.Format(properties);

            Assert.Equal("123", result);
        }

        [Fact]
        public void Should_format_simple_var_in_text()
        {
            var input = "Hello to {{app}}, what's up?";

            var properties = new Dictionary<string, string?>
            {
                ["app"] = "Notifo"
            };

            var result = input.Format(properties);

            Assert.Equal("Hello to Notifo, what's up?", result);
        }

        [Fact]
        public void Should_format_simple_var_in_text2()
        {
            var input = "Hello to {{ app }}, what's up?";

            var properties = new Dictionary<string, string?>
            {
                ["app"] = "Notifo"
            };

            var result = input.Format(properties);

            Assert.Equal("Hello to Notifo, what's up?", result);
        }

        [Fact]
        public void Should_format_multiple_vars_in_text()
        {
            var input = "Hello {{user}} to {{app}}, what's up?";

            var properties = new Dictionary<string, string?>
            {
                ["user"] = "Sebastian", ["app"] = "Notifo"
            };

            var result = input.Format(properties);

            Assert.Equal("Hello Sebastian to Notifo, what's up?", result);
        }

        [Fact]
        public void Should_return_empty_string_if_variable_not_found()
        {
            var input = "Hello {{user}} to {{app}}, what's up?";

            var properties = new Dictionary<string, string?>
            {
                ["user"] = "Sebastian"
            };

            var result = input.Format(properties);

            Assert.Equal("Hello Sebastian to , what's up?", result);
        }

        [Fact]
        public void Should_return_fallback_if_variable_not_found()
        {
            var input = "Hello {{user}} to {{app ? App}}, what's up?";

            var properties = new Dictionary<string, string?>
            {
                ["user"] = "Sebastian"
            };

            var result = input.Format(properties);

            Assert.Equal("Hello Sebastian to App, what's up?", result);
        }

        [Theory]
        [InlineData("{{user | lower}}", "sebastian")]
        [InlineData("{{user | upper}}", "SEBASTIAN")]
        public void Should_transform_variable(string input, string expected)
        {
            var properties = new Dictionary<string, string?>
            {
                ["user"] = "Sebastian"
            };

            var result = input.Format(properties);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_format_localized_text()
        {
            var input = new LocalizedText
            {
                ["en"] = "Hello {{user}}",
                ["de"] = "Hallo {{user}}"
            };

            var properties = new Dictionary<string, string?>
            {
                ["user"] = "Sebastian"
            };

            var result = input.Format(properties);

            Assert.Equal("Hello Sebastian", result["en"]);
            Assert.Equal("Hallo Sebastian", result["de"]);
        }
    }
}
