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
        public string AppId { get; set; }

        public string MimeType { get; set; }

        public string FileName { get; set; }

        public string FileInfo { get; set; }

        public long FileSize { get; set; }

        public MediaType Type { get; set; }

        public MediaMetadata Metadata { get; set; }
    }
}
