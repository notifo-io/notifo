// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Notifo.Infrastructure;

namespace Notifo.Domain.Media
{
    public interface IMediaRepository
    {
        Task<IResultList<Media>> QueryAsync(string appId, MediaQuery query, CancellationToken ct);

        Task<Media?> GetAsync(string appId, string fileName, CancellationToken ct);

        Task UpsertAsync(Media media, CancellationToken ct);

        Task DeleteAsync(string appId, string fileName, CancellationToken ct);
    }
}
