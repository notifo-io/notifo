// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Channels.Email
{
    public sealed class AmazonSESOptions
    {
        public string Host { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string AwsAccessKeyId { get; set; }

        public string AwsSecretAccessKey { get; set; }

        public bool Secure { get; set; } = true;

        public int Timeout { get; set; } = 2000;

        public int Port { get; set; } = 587;
    }
}
