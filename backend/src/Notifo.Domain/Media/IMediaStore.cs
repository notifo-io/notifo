// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure;
using Squidex.Assets;

namespace Notifo.Domain.Media
{
    public interface IMediaStore
    {
        Task<IResultList<Media>> QueryAsync(string appId, MediaQuery query,
            CancellationToken ct = default);

        Task<Media?> GetAsync(string appId, string fileName,
            CancellationToken ct = default);

        Task<Media> UploadAsync(string appId, AssetFile file,
            CancellationToken ct = default);

        Task DeleteAsync(string appId, string fileName,
            CancellationToken ct = default);
    }
}
