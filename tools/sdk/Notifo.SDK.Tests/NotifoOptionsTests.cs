// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Xunit;

namespace Notifo.SDK;

public class NotifoOptionsTests
{
    [Fact]
    public void Should_throw_if_empty()
    {
        var sut = new StaticNotifoOptions();

        Assert.ThrowsAny<InvalidOperationException>(() => sut.Validate());
    }

    [Fact]
    public void Should_throw_if_only_client_id_defined()
    {
        var sut = new StaticNotifoOptions { ClientId = "id" };

        Assert.ThrowsAny<InvalidOperationException>(() => sut.Validate());
    }

    [Fact]
    public void Should_throw_if_only_client_secret_defined()
    {
        var sut = new StaticNotifoOptions { ClientSecret = "secret" };

        Assert.ThrowsAny<InvalidOperationException>(() => sut.Validate());
    }

    [Fact]
    public void Should_throw_if_url_is_empty()
    {
        var sut = new StaticNotifoOptions { ApiKey = "key", ApiUrl = null };

        Assert.ThrowsAny<InvalidOperationException>(() => sut.Validate());
    }

    [Fact]
    public void Should_throw_if_url_is_relative()
    {
        var sut = new StaticNotifoOptions { ApiKey = "key", ApiUrl = "/relative" };

        Assert.ThrowsAny<InvalidOperationException>(() => sut.Validate());
    }

    [Fact]
    public void Should_not_throw_if_api_key_defined()
    {
        var sut = new StaticNotifoOptions { ApiKey = "key" };

        sut.Validate();
    }

    [Fact]
    public void Should_not_throw_if_client_id_and_secret_defined()
    {
        var sut = new StaticNotifoOptions { ClientId = "id", ClientSecret = "secret" };

        sut.Validate();
    }
}
