// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.Channels.Email
{
    public sealed class NoopEmailServer : IEmailServer
    {
        public Task SendAsync(EmailMessage message, CancellationToken ct = default)
        {
            throw new DomainException("EMails are not configured.");
        }

        public Task RemoveEmailAddressAsync(string emailAddress, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task<EmailVerificationStatus> AddEmailAddressAsync(string emailAddress, CancellationToken ct = default)
        {
            return Task.FromResult(EmailVerificationStatus.Verified);
        }

        public Task<Dictionary<string, EmailVerificationStatus>> GetStatusAsync(HashSet<string> emailAddresses, CancellationToken ct = default)
        {
            var result = new Dictionary<string, EmailVerificationStatus>();

            foreach (var address in emailAddresses)
            {
                result[address] = EmailVerificationStatus.Verified;
            }

            return Task.FromResult(result);
        }
    }
}
