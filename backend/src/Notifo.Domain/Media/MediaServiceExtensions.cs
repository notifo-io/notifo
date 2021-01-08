// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Media;
using Notifo.Domain.Media.MongoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MediaServiceExtensions
    {
        public static void AddMyMedia(this IServiceCollection services)
        {
            services.AddSingletonAs<DefaultMediaFileStore>()
                .As<IMediaFileStore>();

            services.AddSingletonAs<MediaStore>()
                .As<IMediaStore>();

            services.AddSingletonAs<ImageMediaMetadataSource>()
                .As<IMediaMetadataSource>();
        }

        public static void AddMyMongoMedia(this IServiceCollection services)
        {
            services.AddSingletonAs<MongoDbMediaRepository>()
                .As<IMediaRepository>();
        }
    }
}
