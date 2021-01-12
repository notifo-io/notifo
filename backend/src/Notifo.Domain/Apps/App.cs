// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Counters;

namespace Notifo.Domain.Apps
{
    public sealed class App
    {
        private const string DefaultAppLanguage = "en";

        public string Id { get; private set; }

        public string Name { get; set; }

        public string[] Languages { get; set; } = new[] { DefaultAppLanguage };

        public string? EmailAddress { get; set; } = string.Empty;

        public string? EmailName { get; set; } = string.Empty;

        public string? FirebaseProject { get; set; } = string.Empty;

        public string? FirebaseCredential { get; set; } = string.Empty;

        public string? WebhookUrl { get; set; } = string.Empty;

        public string? ConfirmUrl { get; set; } = string.Empty;

        public bool AllowEmail { get; set; }

        public bool AllowSms { get; set; }

        public EmailVerificationStatus EmailVerificationStatus { get; set; }

        public List<AppApiKey> ApiKeys { get; set; } = new List<AppApiKey>();

        public Dictionary<string, EmailTemplate> EmailTemplates { get; set; } = new Dictionary<string, EmailTemplate>();

        public Dictionary<string, string> Contributors { get; set; } = new Dictionary<string, string>();

        public CounterMap? Counters { get; set; } = new CounterMap();

        public string Language => Languages[0];

        public static App Create(string appId)
        {
            var app = new App { Id = appId };

            return app;
        }
    }
}
