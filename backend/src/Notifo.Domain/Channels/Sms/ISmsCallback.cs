// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Domain.Channels.Sms
{
    public interface ISmsCallback
    {
        Task HandleCallbackAsync(string to, string token, SmsResult result,
            CancellationToken ct);
    }
}
