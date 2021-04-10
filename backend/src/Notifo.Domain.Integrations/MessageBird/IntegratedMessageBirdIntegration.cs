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

namespace Notifo.Domain.Integrations.MessageBird
{
    public sealed class IntegratedMessageBirdIntegration : IIntegration
    {
        private readonly ISmsSender smsSender;

        public IntegrationDefinition Definition { get; }
            = new IntegrationDefinition(
                "MessageBird",
                "MessageBird",
                "./integreations/messagebird.svg",
                new List<IntegrationProperty>(),
                new HashSet<string>
                {
                    Providers.Sms
                });

        public IntegratedMessageBirdIntegration(ISmsSender smsSender)
        {
            this.smsSender = smsSender;
        }

        public bool CanCreate<T>(ConfiguredIntegration configured)
        {
            return typeof(T) == typeof(ISmsSender);
        }

        public object? Create(Type implementationType, ConfiguredIntegration configured)
        {
            if (implementationType == typeof(ISmsSender))
            {
                return smsSender;
            }

            return null;
        }
    }
}
