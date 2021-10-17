// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.MessageBird.Implementation;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class IntegratedMessageBirdIntegration : IIntegration
    {
        public IntegrationDefinition Definition { get; } =
            new IntegrationDefinition(
                "MessageBirdIntegrated",
                Texts.MessageBirdIntegrated_Name,
                "./integrations/messagebird.svg",
                new List<IntegrationProperty>(),
                new HashSet<string>
                {
                    Providers.Sms
                })
            {
                Description = Texts.MessageBirdIntegrated_Description
            };

        public bool CanCreate(Type serviceType, string id, ConfiguredIntegration configured)
        {
            return serviceType == typeof(ISmsSender);
        }

        public object? Create(Type serviceType, string id, ConfiguredIntegration configured, IServiceProvider serviceProvider)
        {
            if (CanCreate(serviceType, id, configured))
            {
                return new MessageBirdSmsSender(
                    serviceProvider.GetRequiredService<MessageBirdClient>(),
                    serviceProvider.GetRequiredService<ISmsCallback>(),
                    serviceProvider.GetRequiredService<ISmsUrl>(),
                    id);
            }

            return null;
        }
    }
}
