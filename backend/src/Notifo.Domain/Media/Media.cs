// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Media
{
    public sealed class Media
    {
        public string AppId { get; init; }

        public string MimeType { get; init; }

        public string FileName { get; init; }

        public string FileInfo { get; init; }

        public long FileSize { get; init; }

        public MediaType Type { get; init; }

        public MediaMetadata Metadata { get; init; }
    }
}
