// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Domain.Channels.Email
{
    public interface IEmailServer
    {
        Task<EmailVerificationStatus> AddEmailAddressAsync(string emailAddress, CancellationToken ct = default);

        Task RemoveEmailAddressAsync(string emailAddress, CancellationToken ct = default);

        Task SendAsync(EmailMessage message, CancellationToken ct = default);

        Task<Dictionary<string, EmailVerificationStatus>> GetStatusAsync(HashSet<string> emailAddresses, CancellationToken ct = default);
    }
}
