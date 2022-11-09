// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting.Configuration;

namespace Notifo.Pipeline;

public sealed class SignalROptions : IValidatableOptions
{
    public bool Enabled { get; set; }

    public bool Sticky { get; set; }

    public int PollingInterval { get; set; } = 5000;

    public IEnumerable<ConfigurationError> Validate()
    {
        if (PollingInterval < 1000)
        {
            yield return new ConfigurationError("Value must be greater or equals than 1000ms.", nameof(PollingInterval));
        }
    }
}
