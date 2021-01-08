// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Domain.Topics;
using Notifo.Domain.Topics.MongoDb;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TopicsServiceExtensions
    {
        public static void AddMyTopics(this IServiceCollection services)
        {
            services.AddSingletonAs<TopicStore>()
                .As<ITopicStore>().As<ICounterTarget>();
        }

        public static void AddMyMongoTopics(this IServiceCollection services)
        {
            services.AddSingletonAs<MongoDbTopicRepository>()
                .As<ITopicRepository>();
        }
    }
}
