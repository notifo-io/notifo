// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Media;

public interface IMediaMetadataSource
{
    Task EnhanceAsync(MetadataRequest request);

    IEnumerable<string> Format(MetadataRequest request);
}
