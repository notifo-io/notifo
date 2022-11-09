// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Hosting;
using Xunit;

namespace Notifo.Domain.Utils;

public class ImageFormatterTests
{
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly IImageFormatter sut;

    public ImageFormatterTests()
    {
        A.CallTo(() => urlGenerator.BuildUrl())
            .Returns("https://notifo.io");

        A.CallTo(() => urlGenerator.BuildUrl(A<string>._, false))
            .ReturnsLazily(x => $"https://notifo.io{x.GetArgument<string>(0)!}");

        sut = new ImageFormatter(urlGenerator);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid/relative/url")]
    [InlineData("https:/invalid")]
    [InlineData("httpx://invalid")]
    [InlineData("HTTPX://invalid")]
    public void Should_not_add_invalid_url_to_proxy(string url)
    {
        var result = sut.AddProxy(url);

        Assert.Null(result);
    }

    [Fact]
    public void Should_add_url_to_proxy()
    {
        var url = "https://other.com/path";

        var result = sut.AddProxy(url);

        Assert.Equal($"https://notifo.io/api/assets/proxy?url={Uri.EscapeDataString(url)}", result);
    }

    [Fact]
    public void Should_not_add_url_to_proxy_if_same_host()
    {
        var url = "https://notifo.io/path";

        var result = sut.AddProxy(url);

        Assert.Equal($"{url}?emptyOnFailure=true", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid/relative/url")]
    [InlineData("https:/invalid")]
    [InlineData("httpx://invalid")]
    [InlineData("HTTPX://invalid")]
    public void Should_not_add_preset_to_invalid_url(string url)
    {
        var result = sut.AddPreset(url, "Email");

        Assert.Equal(url, result);
    }

    [Fact]
    public void Should_add_preset_to_url()
    {
        var url = "https://other.com/path";

        var result = sut.AddPreset(url, "Email");

        Assert.Equal("https://other.com/path?preset=Email", result);
    }

    [Fact]
    public void Should_add_preset_to_url_with_query()
    {
        var url = "https://other.com/path?ttl=0";

        var result = sut.AddPreset(url, "Email");

        Assert.Equal("https://other.com/path?ttl=0&preset=Email", result);
    }
}
