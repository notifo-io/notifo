// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Notifo.Domain.Channels;
using Notifo.Domain.Channels.Sms;
using Notifo.Domain.Integrations.Resources;

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class IntegratedMessageBirdIntegration : IIntegration
    {
        private readonly IntegratedMessageBirdSmsSender smsSender;

        public IntegrationDefinition Definition { get; }
            = new IntegrationDefinition(
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

        public IntegratedMessageBirdIntegration(IntegratedMessageBirdSmsSender smsSender)
        {
            this.smsSender = smsSender;
        }

        public bool CanCreate(Type serviceType, ConfiguredIntegration configured)
        {
            return serviceType == typeof(ISmsSender);
        }

        public object? Create(Type serviceType, ConfiguredIntegration configured)
        {
            if (CanCreate(serviceType, configured))
            {
                return smsSender;
            }

            return null;
        }
    }
}
