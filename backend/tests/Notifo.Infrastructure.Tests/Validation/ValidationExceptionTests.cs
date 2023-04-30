// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Validation;

public class ValidationExceptionTests
{
    [Fact]
    public void Should_format_single_error()
    {
        var ex = new ValidationException("Single Error.");

        Assert.Equal("Single Error.", ex.Message);
    }

    [Fact]
    public void Should_format_single_error_without_dot()
    {
        var ex = new ValidationException("Single Error");

        Assert.Equal("Single Error.", ex.Message);
    }

    [Fact]
    public void Should_format_single_error_with_property()
    {
        var ex = new ValidationException(
            new ValidationError("Single Error.", "Property1"));

        Assert.Equal("Property1: Single Error.", ex.Message);
    }

    [Fact]
    public void Should_format_single_error_with_properties()
    {
        var ex = new ValidationException(
            new ValidationError("Single Error.", "Property1", "Property2"));

        Assert.Equal("Property1, Property2: Single Error.", ex.Message);
    }

    [Fact]
    public void Should_format_multiple_errors()
    {
        var ex = new ValidationException(
            new List<ValidationError>
            {
                new ValidationError("Error1."),
                new ValidationError("Error2.")
            });

        Assert.Equal("Error1. Error2.", ex.Message);
    }
}
