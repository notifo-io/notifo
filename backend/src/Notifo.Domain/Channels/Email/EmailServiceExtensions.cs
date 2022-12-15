// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Mjml.Net;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Channels.Email.Formatting;
using Notifo.Domain.ChannelTemplates;
using Notifo.Infrastructure.Scheduling;

namespace Microsoft.Extensions.DependencyInjection;

public static class EmailServiceExtensions
{
    public static void AddMyEmailChannel(this IServiceCollection services)
    {
        services.AddSingletonAs<EmailChannel>()
            .As<ICommunicationChannel>().As<IScheduleHandler<EmailJob>>();

        services.AddSingletonAs<EmailFormatterLiquid>()
            .As<IEmailFormatter>().As<IChannelTemplateFactory<EmailTemplate>>();

        services.AddChannelTemplates<EmailTemplate>();

        services.AddScheduler<EmailJob>(Providers.Email);
    }
}
