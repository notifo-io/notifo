// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;

namespace Notifo.Infrastructure.Json;

public sealed class SystemTextJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions options;

    public SystemTextJsonSerializer(JsonSerializerOptions options)
    {
        this.options = options;
    }

    public T Deserialize<T>(ReadOnlySpan<byte> data)
    {
        return JsonSerializer.Deserialize<T>(data, options)!;
    }

    public T Deserialize<T>(byte[] data)
    {
        return JsonSerializer.Deserialize<T>(data, options)!;
    }

    public string SerializeToString<T>(T data)
    {
        return JsonSerializer.Serialize(data, options);
    }

    public byte[] SerializeToBytes<T>(T data)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data, options);
    }
}
