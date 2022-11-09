// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentFTP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Squidex.Assets;
using Squidex.Assets.Remote;
using Squidex.Hosting;

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
                var path = config.GetRequiredValue("assetStore:folder:path");

                services.AddSingletonAs(c => new FolderAssetStore(path, c.GetRequiredService<ILogger<FolderAssetStore>>()))
                    .As<IAssetStore>();
            },
            ["GoogleCloud"] = () =>
            {
                var options = new GoogleCloudAssetOptions
                {
                    BucketName = config.GetRequiredValue("assetStore:googleCloud:bucket")
                };

                services.AddSingletonAs(c => new GoogleCloudAssetStore(options))
                    .As<IAssetStore>();
            },
            ["AzureBlob"] = () =>
            {
                var options = new AzureBlobAssetOptions
                {
                    ConnectionString = config.GetRequiredValue("assetStore:azureBlob:connectionString"),
                    ContainerName = config.GetRequiredValue("assetStore:azureBlob:containerName")
                };

                services.AddSingletonAs(c => new AzureBlobAssetStore(options))
                    .As<IAssetStore>();
            },
            ["AmazonS3"] = () =>
            {
                var amazonS3Options = config.GetSection("assetStore:amazonS3").Get<AmazonS3AssetOptions>() ?? new AmazonS3AssetOptions();

                services.AddSingletonAs(c => new AmazonS3AssetStore(amazonS3Options))
                    .As<IAssetStore>();
            },
            ["MongoDb"] = () =>
            {
                var mongoGridFsBucketName = config.GetRequiredValue("assetStore:mongoDb:bucket");

                services.AddSingletonAs(c =>
                {
                    var mongoDatabase = c.GetRequiredService<IMongoDatabase>();

                    var gridFsbucket = new GridFSBucket<string>(mongoDatabase, new GridFSBucketOptions
                    {
                        BucketName = mongoGridFsBucketName
                    });

                    return new MongoGridFsAssetStore(gridFsbucket);
                })
                .As<IAssetStore>();
            },
            ["Ftp"] = () =>
            {
                var serverHost = config.GetRequiredValue("assetStore:ftp:serverHost");
                var serverPort = config.GetOptionalValue<int>("assetStore:ftp:serverPort", 21);

                var username = config.GetRequiredValue("assetStore:ftp:username");
                var password = config.GetRequiredValue("assetStore:ftp:password");

                var options = new FTPAssetOptions
                {
                    Path = config.GetOptionalValue("assetStore:ftp:path", "/")!
                };

                services.AddSingletonAs(c =>
                {
                    var factory = new Func<AsyncFtpClient>(() => new AsyncFtpClient(serverHost, username, password, serverPort));

                    return new FTPAssetStore(factory, options, c.GetRequiredService<ILogger<FTPAssetStore>>());
                })
                .As<IAssetStore>();
            }
        });

        services.AddSingletonAs(c => new DelegateInitializer(
                c.GetRequiredService<IAssetStore>().GetType().Name!,
                c.GetRequiredService<IAssetStore>().InitializeAsync))
            .As<IInitializable>();
    }
}
