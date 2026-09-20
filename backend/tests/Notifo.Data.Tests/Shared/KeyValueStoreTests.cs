// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.KeyValueStore;

namespace Notifo.Shared;

public abstract class KeyValueStoreTests
{
    private readonly string key = Guid.NewGuid().ToString();

    protected abstract Task<IKeyValueStore> CreateSutAsync();

    [Fact]
    public async Task Should_return_null_if_key_not_found()
    {
        var sut = await CreateSutAsync();

        var result = await sut.GetAsync(key);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_set_and_get_value()
    {
        var sut = await CreateSutAsync();

        await sut.SetAsync(key, "value1");

        var result = await sut.GetAsync(key);

        Assert.Equal("value1", result);
    }

    [Fact]
    public async Task Should_overwrite_value()
    {
        var sut = await CreateSutAsync();

        await sut.SetAsync(key, "value1");

        var updated = await sut.SetAsync(key, "value2");

        var result = await sut.GetAsync(key);

        Assert.True(updated);
        Assert.Equal("value2", result);
    }

    [Fact]
    public async Task Should_set_value_if_not_exists()
    {
        var sut = await CreateSutAsync();

        var result = await sut.SetIfNotExistsAsync(key, "value1");

        Assert.Equal("value1", result);
        Assert.Equal("value1", await sut.GetAsync(key));
    }

    [Fact]
    public async Task Should_return_existing_value_if_already_set()
    {
        var sut = await CreateSutAsync();

        await sut.SetIfNotExistsAsync(key, "value1");

        var result = await sut.SetIfNotExistsAsync(key, "value2");

        Assert.Equal("value1", result);
        Assert.Equal("value1", await sut.GetAsync(key));
    }

    [Fact]
    public async Task Should_remove_value()
    {
        var sut = await CreateSutAsync();

        await sut.SetAsync(key, "value1");

        var removed = await sut.RemvoveAsync(key);

        Assert.True(removed);
        Assert.Null(await sut.GetAsync(key));
    }

    [Fact]
    public async Task Should_return_false_if_removed_key_not_found()
    {
        var sut = await CreateSutAsync();

        var removed = await sut.RemvoveAsync(key);

        Assert.False(removed);
    }
}
