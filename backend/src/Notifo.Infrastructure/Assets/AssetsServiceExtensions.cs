// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FluentFTP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Squidex.Assets;
using Squidex.Assets.ImageSharp;
using Squidex.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AssetsServiceExtensions
    {
        public static void AddMyAssets(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingletonAs<ImageSharpAssetThumbnailGenerator>()
                .As<IAssetThumbnailGenerator>();

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
                    var bucketName = config.GetRequiredValue("assetStore:googleCloud:bucket");

                    services.AddSingletonAs(c => new GoogleCloudAssetStore(bucketName))
                        .As<IAssetStore>();
                },
                ["AzureBlob"] = () =>
                {
                    var connectionString = config.GetRequiredValue("assetStore:azureBlob:connectionString");
                    var containerName = config.GetRequiredValue("assetStore:azureBlob:containerName");

                    services.AddSingletonAs(c => new AzureBlobAssetStore(connectionString, containerName))
                        .As<IAssetStore>();
                },
                ["AmazonS3"] = () =>
                {
                    var amazonS3Options = config.GetSection("assetStore:amazonS3").Get<AmazonS3Options>();

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

                    var path = config.GetOptionalValue("assetStore:ftp:path", "/");

                    services.AddSingletonAs(c =>
                    {
                        var factory = new Func<FtpClient>(() => new FtpClient(serverHost, serverPort, username, password));

                        return new FTPAssetStore(factory, path, c.GetRequiredService<ILogger<FTPAssetStore>>());
                    })
                    .As<IAssetStore>();
                }
            });

            services.AddSingletonAs<ImageSharpAssetThumbnailGenerator>()
                .As<IAssetThumbnailGenerator>();

            services.AddSingletonAs(c => new DelegateInitializer(
                    c.GetRequiredService<IAssetStore>().GetType().FullName!,
                    c.GetRequiredService<IAssetStore>().InitializeAsync))
                .As<IInitializable>();
        }
    }
}
