// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Infrastructure.Scheduling
{
    public interface IScheduleHandler<T>
    {
        Task<bool> HandleAsync(List<T> jobs, bool isLastAttempt, CancellationToken ct);

        Task HandleExceptionAsync(List<T> jobs, Exception ex)
        {
            return Task.CompletedTask;
        }
    }
}
