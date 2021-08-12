// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Notifo.Domain.Counters;
using Notifo.Domain.Integrations;

namespace Notifo.Domain.Apps
{
    public sealed class App
    {
        private const string DefaultAppLanguage = "en";

        public string Id { get; private init; }

        public string Name { get; set; }

        public string[] Languages { get; set; } = { DefaultAppLanguage };

        public string? ConfirmUrl { get; set; } = string.Empty;

        public Dictionary<string, string> ApiKeys { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Contributors { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, ConfiguredIntegration> Integrations { get; set; } = new Dictionary<string, ConfiguredIntegration>();

        public CounterMap? Counters { get; set; } = new CounterMap();

        public string Language => Languages[0];

        public static App Create(string appId)
        {
            var app = new App { Id = appId };

            return app;
        }
    }
}
