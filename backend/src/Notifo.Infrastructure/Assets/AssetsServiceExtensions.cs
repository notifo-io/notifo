// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Squidex.Assets;
using Squidex.Assets.Remote;

namespace Microsoft.Extensions.DependencyInjection;

public static class AssetsServiceExtensions
{
    public static void AddMyAssets(this IServiceCollection services, IConfiguration config)
    {
        var thumbnailGenerator = new CompositeThumbnailGenerator(
            new IAssetThumbnailGenerator[]
            {
                new ImageSharpThumbnailGenerator(),
                new ImageMagickThumbnailGenerator()
            });

        var resizerUrl = config.GetValue<string>("assets:resizerUrl");

        if (!string.IsNullOrWhiteSpace(resizerUrl))
        {
            services.AddHttpClient("Resize", options =>
            {
                options.BaseAddress = new Uri(resizerUrl);
            });

            services.AddSingletonAs(c => new RemoteThumbnailGenerator(c.GetRequiredService<IHttpClientFactory>(), thumbnailGenerator))
                .As<IAssetThumbnailGenerator>();
        }
        else
        {
            services.AddSingletonAs(c => thumbnailGenerator)
                .As<IAssetThumbnailGenerator>();
        }

        config.ConfigureByOption("assetStore:type", new Alternatives
        {
            ["Folder"] = () =>
            {
                services.AddFolderAssetStore(config);
            },
            ["FTP"] = () =>
            {
                services.AddFTPAssetStore(config);
            },
            ["GoogleCloud"] = () =>
            {
                services.AddGoogleCloudAssetStore(config);
            },
            ["AzureBlob"] = () =>
            {
                services.AddAzureBlobAssetStore(config);
            },
            ["AmazonS3"] = () =>
            {
                services.AddAmazonS3AssetStore(config);
            },
            ["MongoDb"] = () =>
            {
                var mongoGridFsBucketName = config.GetRequiredValue("assetStore:mongoDb:bucket");

                services.AddMongoAssetStore(c =>
                {
                    var mongoDatabase = c.GetRequiredService<IMongoDatabase>();

                    return new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
                    {
                        BucketName = mongoGridFsBucketName
                    });
                });
            }
        });
    }
}
