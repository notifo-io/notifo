// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.Validation;

namespace Notifo.Domain.Integrations;

public class IntegrationPropertyTests
{
    public class Strings
    {
        [Theory]
        [InlineData(PropertyType.Text)]
        [InlineData(PropertyType.Password)]
        [InlineData(PropertyType.MultilineText)]
        public void Should_get_string_value(PropertyType type)
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = "value"
            };

            var property = new IntegrationProperty("key", type);

            Assert.Equal("value", property.GetString(source));
        }

        [Fact]
        public void Should_fail_if_value_retrieved_from_invalid_type()
        {
            var property = new IntegrationProperty("key", PropertyType.Boolean);

            Assert.Throws<ValidationException>(() => property.GetNumber(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_fail_if_value_is_required(string value)
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = value
            };

            var property = new IntegrationProperty("key", PropertyType.Text)
            {
                IsRequired = true
            };

            Assert.Throws<ValidationException>(() => property.GetNumber(source));
        }

        [Fact]
        public void Should_fail_if_value_is_too_short()
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = "1234"
            };

            var property = new IntegrationProperty("key", PropertyType.Text)
            {
                MinLength = 5
            };

            Assert.Throws<ValidationException>(() => property.GetString(source));
        }

        [Fact]
        public void Should_fail_if_value_is_too_long()
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = "123456"
            };

            var property = new IntegrationProperty("key", PropertyType.Text)
            {
                MaxLength = 5
            };

            Assert.Throws<ValidationException>(() => property.GetString(source));
        }

        [Fact]
        public void Should_fail_if_value_has_wrong_pattern()
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = "abcdef"
            };

            var property = new IntegrationProperty("key", PropertyType.Text)
            {
                Pattern = "^[0-9]$"
            };

            Assert.Throws<ValidationException>(() => property.GetString(source));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_not_fail_if_undefined_value_does_not_follow_pattern(string input)
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = input
            };

            var property = new IntegrationProperty("key", PropertyType.Text)
            {
                Pattern = "^[0-9]$"
            };

            Assert.Equal(string.Empty, property.GetString(source));
        }

        [Fact]
        public void Should_fail_if_value_has_not_allowed_value()
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = "123456"
            };

            var property = new IntegrationProperty("key", PropertyType.Text)
            {
                AllowedValues = new[] { "allowed" }
            };

            Assert.Throws<ValidationException>(() => property.GetString(source));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_not_fail_if_undefined_value_is_not_an_allowed_value(string input)
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = input
            };

            var property = new IntegrationProperty("key", PropertyType.Text)
            {
                AllowedValues = new[] { "allowed" }
            };

            Assert.Equal("allowed", property.GetString(source));
        }

        [Fact]
        public void Should_get_value_if_all_requirements_are_met()
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = "1234"
            };

            var property = new IntegrationProperty("key", PropertyType.Text)
            {
                IsRequired = true,
                MaxLength = 4,
                MinLength = 4,
                AllowedValues = new[] { "1234" },
                Pattern = "^[0-9]+$"
            };

            property.GetString(source);
        }
    }

    public class Booleans
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("1", true)]
        [InlineData("0", false)]
        [InlineData("2", false)]
        [InlineData("true", true)]
        [InlineData("True", true)]
        [InlineData("false", false)]
        [InlineData("False", false)]
        public void Should_get_boolean_from_properties(string value, bool expected)
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = value
            };

            var property = new IntegrationProperty("key", PropertyType.Boolean);

            Assert.Equal(expected, property.GetBoolean(source));
        }

        [Fact]
        public void Should_fail_if_value_retrieved_from_invalid_type()
        {
            var property = new IntegrationProperty("key", PropertyType.Number);

            Assert.Throws<ValidationException>(() => property.GetBoolean(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_fail_if_value_is_required(string value)
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = value
            };

            var property = new IntegrationProperty("key", PropertyType.Boolean)
            {
                IsRequired = true
            };

            Assert.Throws<ValidationException>(() => property.GetBoolean(source));
        }
    }

    public class Numbers
    {
        [Theory]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        [InlineData(" ", 0)]
        [InlineData("0", 0)]
        [InlineData("1", 1)]
        [InlineData("2", 2)]
        public void Should_get_number_from_properties(string value, int expected)
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = value
            };

            var property = new IntegrationProperty("key", PropertyType.Number);

            Assert.Equal(expected, property.GetNumber(source));
        }

        [Fact]
        public void Should_fail_if_value_retrieved_from_invalid_type()
        {
            var property = new IntegrationProperty("key", PropertyType.Boolean);

            Assert.Throws<ValidationException>(() => property.GetNumber(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Should_fail_if_value_is_required(string value)
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = value
            };

            var property = new IntegrationProperty("key", PropertyType.Number)
            {
                IsRequired = true
            };

            Assert.Throws<ValidationException>(() => property.GetNumber(source));
        }

        [Fact]
        public void Should_fail_if_value_is_too_small()
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = "4"
            };

            var property = new IntegrationProperty("key", PropertyType.Number)
            {
                MinValue = 5
            };

            Assert.Throws<ValidationException>(() => property.GetNumber(source));
        }

        [Fact]
        public void Should_fail_if_value_is_too_big()
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = "6"
            };

            var property = new IntegrationProperty("key", PropertyType.Number)
            {
                MaxValue = 5
            };

            Assert.Throws<ValidationException>(() => property.GetNumber(source));
        }

        [Fact]
        public void Should_get_value_if_all_requirements_are_met()
        {
            var source = new Dictionary<string, string>
            {
                ["key"] = "4"
            };

            var property = new IntegrationProperty("key", PropertyType.Number)
            {
                IsRequired = true,
                MaxValue = 4,
                MinValue = 4
            };

            property.GetNumber(source);
        }
    }
}
