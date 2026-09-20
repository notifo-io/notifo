// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Domain.Topics;

namespace Microsoft.Extensions.DependencyInjection;

public static class TopicsServiceExtensions
{
    public static void AddMyTopics(this IServiceCollection services)
    {
        services.AddSingletonAs<TopicStore>()
            .As<ITopicStore>().As<ICounterTarget>();

        services.AddRequestHandler<TopicStore, TopicCommand, Topic?>();
    }
}
