// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Notifo.Domain.Integrations.Smtp;
using Squidex.Hosting.Configuration;

namespace Notifo.Domain.Integrations.AmazonSES
{
    public sealed class AmazonSESOptions : SmtpOptions
    {
        public string Region { get; set; } = "eu-central-1";

        public string AwsAccessKeyId { get; set; }

        public string AwsSecretAccessKey { get; set; }

        public override IEnumerable<ConfigurationError> Validate()
        {
            foreach (var error in base.Validate())
            {
                yield return error;
            }

            if (string.IsNullOrWhiteSpace(Region))
            {
                yield return new ConfigurationError("Value is required.", nameof(Region));
            }

            if (string.IsNullOrWhiteSpace(AwsAccessKeyId))
            {
                yield return new ConfigurationError("Value is required.", nameof(AwsAccessKeyId));
            }

            if (string.IsNullOrWhiteSpace(AwsSecretAccessKey))
            {
                yield return new ConfigurationError("Value is required.", nameof(AwsSecretAccessKey));
            }
        }
    }
}
