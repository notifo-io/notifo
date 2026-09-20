// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.SDK;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class MediaTests : IClassFixture<CreatedAppFixture>
{
    public CreatedAppFixture _ { get; set; }

    public MediaTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_upload_and_query_media()
    {
        // STEP 0: Upload media.
        var fileName = await UploadAsync();


        // STEP 1: Query media.
        var medias = await _.Client.Media.GetMediasAsync(_.AppId, fileName);

        var media = medias.Items.SingleOrDefault(x => x.FileName == fileName);

        Assert.NotNull(media);
        Assert.Equal(MediaType.Image, media.Type);
        Assert.Equal("image/png", media.MimeType);
        Assert.Equal(new FileInfo(AssetPath).Length, media.FileSize);
        Assert.Equal("600", media.Metadata["pixelWidth"]);
        Assert.Equal("135", media.Metadata["pixelHeight"]);
    }

    [Fact]
    public async Task Should_download_and_resize_media()
    {
        // STEP 0: Upload media.
        var fileName = await UploadAsync();


        // STEP 1: Download the original.
        var original = await DownloadAsync(await _.Client.Media.DownloadAsync(_.AppId, fileName));

        Assert.Equal(await File.ReadAllBytesAsync(AssetPath), original);


        // STEP 2: Download a resized version.
        var resized = await DownloadAsync(await _.Client.Media.DownloadAsync(_.AppId, fileName, width: 100, height: 50));

        Assert.NotEmpty(resized);
        Assert.True(resized.Length < original.Length, "Resized image is not smaller than the original.");
    }

    [Fact]
    public async Task Should_delete_media()
    {
        // STEP 0: Upload media.
        var fileName = await UploadAsync();


        // STEP 1: Delete media.
        await _.Client.Media.DeleteAsync(_.AppId, fileName);


        // Get medias.
        var medias = await _.Client.Media.GetMediasAsync(_.AppId, fileName);

        Assert.DoesNotContain(medias.Items, x => x.FileName == fileName);

        var ex = await Assert.ThrowsAsync<NotifoException>(() => _.Client.Media.DownloadAsync(_.AppId, fileName));

        Assert.Equal(404, ex.StatusCode);
    }

    private static string AssetPath => Path.Combine("Assets", "logo-wide.png");

    private async Task<string> UploadAsync()
    {
        var fileName = $"{Guid.NewGuid()}.png";

        await using (var stream = File.OpenRead(AssetPath))
        {
            await _.Client.Media.UploadAsync(_.AppId, new FileParameter(stream, fileName, "image/png"));
        }

        return fileName;
    }

    private static async Task<byte[]> DownloadAsync(FileResponse response)
    {
        using (response)
        {
            using (var buffer = new MemoryStream())
            {
                await response.Stream.CopyToAsync(buffer);

                return buffer.ToArray();
            }
        }
    }
}
