// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Notifo.Domain.Media;

namespace Notifo.Shared;

public abstract class MediaRepositoryTests
{
    private readonly Instant now = Instant.FromUtc(2024, 1, 2, 10, 30, 0);
    private readonly string appId = Guid.NewGuid().ToString();

    protected abstract Task<IMediaRepository> CreateSutAsync();

    [Fact]
    public async Task Should_insert_and_get_media()
    {
        var sut = await CreateSutAsync();

        var media = CreateMedia("image.png");

        await sut.UpsertAsync(media);

        var result = await sut.GetAsync(appId, media.FileName);

        result.Should().BeEquivalentTo(media);
    }

    [Fact]
    public async Task Should_return_null_if_media_not_found()
    {
        var sut = await CreateSutAsync();

        var result = await sut.GetAsync(appId, "unknown.png");

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_replace_existing_media()
    {
        var sut = await CreateSutAsync();

        var media = CreateMedia("image.png");

        await sut.UpsertAsync(media);
        await sut.UpsertAsync(media with { FileSize = 2048 });

        var result = await sut.GetAsync(appId, media.FileName);

        Assert.Equal(2048, result!.FileSize);
    }

    [Fact]
    public async Task Should_delete_media()
    {
        var sut = await CreateSutAsync();

        var media = CreateMedia("image.png");

        await sut.UpsertAsync(media);
        await sut.DeleteAsync(appId, media.FileName);

        var result = await sut.GetAsync(appId, media.FileName);

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_not_delete_media_of_other_app()
    {
        var sut = await CreateSutAsync();

        var media = CreateMedia("image.png");

        await sut.UpsertAsync(media);
        await sut.DeleteAsync(Guid.NewGuid().ToString(), media.FileName);

        var result = await sut.GetAsync(appId, media.FileName);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_query_media_by_file_name()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateMedia("logo.png"));
        await sut.UpsertAsync(CreateMedia("banner.png"));

        var result = await sut.QueryAsync(appId, new MediaQuery { Query = "LOGO" });

        Assert.Equal(["logo.png"], result.Select(x => x.FileName));
    }

    [Fact]
    public async Task Should_query_media_sorted_by_last_update()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateMedia("image1.png") with { LastUpdate = now.Plus(Duration.FromMinutes(1)) });
        await sut.UpsertAsync(CreateMedia("image2.png") with { LastUpdate = now.Plus(Duration.FromMinutes(3)) });
        await sut.UpsertAsync(CreateMedia("image3.png") with { LastUpdate = now.Plus(Duration.FromMinutes(2)) });

        var result = await sut.QueryAsync(appId, new MediaQuery());

        Assert.Equal(["image2.png", "image3.png", "image1.png"], result.Select(x => x.FileName));
    }

    [Fact]
    public async Task Should_query_media_with_paging_and_total()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateMedia("image1.png"));
        await sut.UpsertAsync(CreateMedia("image2.png"));
        await sut.UpsertAsync(CreateMedia("image3.png"));

        var result = await sut.QueryAsync(appId, new MediaQuery { Take = 2 });

        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public async Task Should_not_query_media_of_other_app()
    {
        var sut = await CreateSutAsync();

        await sut.UpsertAsync(CreateMedia("image.png") with { AppId = Guid.NewGuid().ToString() });

        var result = await sut.QueryAsync(appId, new MediaQuery());

        Assert.Empty(result);
    }

    private Media CreateMedia(string fileName)
    {
        return new Media(appId, fileName, now)
        {
            FileInfo = "Info",
            FileSize = 1024,
            LastUpdate = now,
            Metadata = new MediaMetadata().SetPixelWidth(100).SetPixelHeight(50),
            MimeType = "image/png",
            Type = MediaType.Image
        };
    }
}
