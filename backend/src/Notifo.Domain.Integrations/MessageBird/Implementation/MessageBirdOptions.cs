// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting.Configuration;

namespace Notifo.Domain.Integrations.MessageBird.Implementation;

public sealed class MessageBirdOptions : IValidatableOptions
{
    public string AccessKey { get; set; }

    public string PhoneNumber { get; set; }

    public Dictionary<string, string>? PhoneNumbers { get; set; }

    public bool IsValid()
    {
        return !Validate().Any();
    }

    public IEnumerable<ConfigurationError> Validate()
    {
        if (string.IsNullOrWhiteSpace(AccessKey))
        {
            yield return new ConfigurationError("Value is required.", nameof(AccessKey));
        }

        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            yield return new ConfigurationError("Value is required.", nameof(PhoneNumber));
        }
    }
}
