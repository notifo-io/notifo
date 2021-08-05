// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Notifo.Infrastructure;

namespace Notifo.Domain.Users
{
    public sealed class UpdateMobileWakeupTime : ICommand<User>
    {
        public string Token { get; set; }

        public Instant Timestamp { get; set; }

        public Task<bool> ExecuteAsync(User target, IServiceProvider serviceProvider,
            CancellationToken ct)
        {
            var token = target.MobilePushTokens.FirstOrDefault(x => x.Token == Token);

            if (token != null && token.LastWakeup < Timestamp)
            {
                token.LastWakeup = Timestamp;

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
