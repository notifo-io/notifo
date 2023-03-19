// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Scheduling;

public interface IScheduleHandler<T>
{
    Task<bool> HandleAsync(List<T> jobs, bool isLastAttempt,
        CancellationToken ct);

    Task HandleExceptionAsync(List<T> jobs, Exception exception);
}
