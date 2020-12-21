// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Notifo.Infrastructure.Json
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(ReadOnlySpan<byte> data);

        T Deserialize<T>(byte[] data);

        string SerializeToString<T>(T data);

        byte[] SerializeToBytes<T>(T data);
    }
}
