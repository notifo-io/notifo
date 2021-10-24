// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Counters;
using Notifo.Domain.Integrations;
using Notifo.Infrastructure.Collections;

namespace Notifo.Domain.Apps
{
    public sealed record App
    {
        private static readonly ReadonlyList<string> DefaultLanguages = ReadonlyList.Create("en");

        public string Id { get; private init; }

        public string Name { get; init; }

        public string Language => Languages[0];

        public string? ConfirmUrl { get; init; }

        public ReadonlyList<string> Languages { get; init; } = DefaultLanguages;

        public ReadonlyDictionary<string, string> ApiKeys { get; init; } = ReadonlyDictionary.Empty<string, string>();

        public ReadonlyDictionary<string, string> Contributors { get; init; } = ReadonlyDictionary.Empty<string, string>();

        public ReadonlyDictionary<string, ConfiguredIntegration> Integrations { get; init; } = ReadonlyDictionary.Empty<string, ConfiguredIntegration>();

        public CounterMap? Counters { get; init; } = new CounterMap();

        public static App Create(string appId)
        {
            var app = new App
            {
                Id = appId
            };

            return app;
        }
    }
}
