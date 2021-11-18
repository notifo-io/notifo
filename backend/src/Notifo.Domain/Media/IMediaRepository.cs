// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;

namespace Notifo.Domain.Media
{
    public interface IMediaRepository
    {
        Task<IResultList<Media>> QueryAsync(string appId, MediaQuery query,
            CancellationToken ct = default);

        Task<Media?> GetAsync(string appId, string fileName,
            CancellationToken ct = default);

        Task UpsertAsync(Media media,
            CancellationToken ct = default);

        Task DeleteAsync(string appId, string fileName,
            CancellationToken ct = default);
    }
}
