// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Xunit;

namespace Notifo.SDK.Tests
{
    public class NotifoExceptionTests
    {
        [Fact]
        public void Should_format_exception_with_dto()
        {
            var exception =
                new NotifoException<ErrorDto>(
                    "Error",
                    404,
                    string.Empty,
                    null,
                    new ErrorDto
                    {
                        Message = "My Message",
                    },
                    null);

            var formatted = exception.ToString();

            Assert.Contains("My Message", formatted);
        }

        [Fact]
        public void Should_format_exception_with_dto_and_details()
        {
            var exception =
                new NotifoException<ErrorDto>(
                    "Error",
                    404,
                    string.Empty,
                    null,
                    new ErrorDto
                    {
                        Message = "My Message",
                        Details =
                        [
                            "Error1",
                            "Error2"
                        ]
                    },
                    null);

            var formatted = exception.ToString();

            Assert.Contains("My Message: Error1. Error2.", formatted);
        }

        [Fact]
        public void Should_format_exception_with_dto_and_details_and_dots()
        {
            var exception =
                new NotifoException<ErrorDto>(
                    "Error",
                    404,
                    string.Empty,
                    null,
                    new ErrorDto
                    {
                        Message = "My Message",
                        Details =
                        [
                            "Error1.",
                            "Error2 "
                        ]
                    },
                    null);

            var formatted = exception.ToString();

            Assert.Contains("My Message: Error1. Error2.", formatted);
        }
    }
}
