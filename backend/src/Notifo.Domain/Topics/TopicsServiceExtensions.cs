// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Counters;
using Notifo.Domain.Topics.MongoDb;

namespace Notifo.Domain.Topics
{
    public static class TopicsServiceExtensions
    {
        public static void AddMyTopics(this IServiceCollection services)
        {
            services.AddSingletonAs<MongoDbTopicRepository>()
                .As<ITopicRepository>();

            services.AddSingletonAs<TopicStore>()
                .As<ITopicStore>().As<ICounterTarget>();
        }
    }
}
