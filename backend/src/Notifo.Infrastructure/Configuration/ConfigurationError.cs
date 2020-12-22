// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Infrastructure.Configuration
{
    [Serializable]
    public sealed class ConfigurationError
    {
        public string Message { get; }

        public string? Path { get; }

        public ConfigurationError(string message, string? path = null)
        {
            Guard.NotNullOrEmpty(message, nameof(message));

            Path = path;

            Message = message;
        }

        public override string ToString()
        {
            var result = Message;

            if (!string.IsNullOrWhiteSpace(Path))
            {
                result = $"{Path.ToLowerInvariant()}: {Message}";
            }

            if (!result.EndsWith('.'))
            {
                result += ".";
            }

            return result;
        }
    }
}
