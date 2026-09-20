// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Notifo.Infrastructure.ObjectPool;

public static class DefaultPools
{
    public static readonly ObjectPool<StringBuilder> StringBuilder =
        new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
}
