// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;

namespace Notifo.Infrastructure.Initialization
{
    public interface IInitializable
    {
        int InitializationOrder => 0;

        Task InitializeAsync(CancellationToken ct = default);

        Task ReleaseAsync(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
