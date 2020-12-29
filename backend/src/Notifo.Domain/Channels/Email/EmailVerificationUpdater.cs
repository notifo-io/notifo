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
using Notifo.Domain.Apps;
using Notifo.Infrastructure.Timers;
using Squidex.Hosting;
using Squidex.Log;

namespace Notifo.Domain.Channels.Email
{
    public sealed class EmailVerificationUpdater : IInitializable
    {
        private readonly IAppStore appStore;
        private readonly IEmailServer emailServer;
        private readonly ISemanticLog log;
        private CompletionTimer timer;

        public EmailVerificationUpdater(IAppStore appStore, IEmailServer emailServer, ISemanticLog log)
        {
            this.appStore = appStore;
            this.emailServer = emailServer;

            this.log = log;
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            timer = new CompletionTimer(5000, CheckAsync, 5000);

            return Task.CompletedTask;
        }

        public async Task ReleaseAsync(CancellationToken ct = default)
        {
            if (timer != null)
            {
                await timer.StopAsync();
            }
        }

        public async Task CheckAsync(CancellationToken ct)
        {
            try
            {
                var pending = await appStore.QueryNonConfirmedEmailAddressesAsync(ct);

                if (pending.Count > 0)
                {
                    var emails = pending.Select(x => x.EmailAddress).ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var statuses = await emailServer.GetStatusAsync(emails!, ct);

                    foreach (var (emailAddress, status) in statuses)
                    {
                        foreach (var app in pending.Where(x => string.Equals(x.EmailAddress, emailAddress, StringComparison.OrdinalIgnoreCase)))
                        {
                            var currentStatus = status;

                            if (app.EmailVerificationStatus != currentStatus)
                            {
                                if (currentStatus == EmailVerificationStatus.Failed)
                                {
                                    if (!string.IsNullOrWhiteSpace(app.EmailAddress))
                                    {
                                        await emailServer.AddEmailAddressAsync(app.EmailAddress, ct);

                                        currentStatus = EmailVerificationStatus.Pending;
                                    }
                                }

                                var update = new UpdateAppEmailVerificationStatus
                                {
                                    Status = currentStatus
                                };

                                await appStore.UpsertAsync(app.Id, update, ct);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "CheckEmailAddresses")
                    .WriteProperty("status", "Failed"));
            }
        }
    }
}
