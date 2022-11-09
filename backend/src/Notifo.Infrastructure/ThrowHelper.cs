// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using MongoDB.Bson;

namespace Notifo.Infrastructure;

public static class ThrowHelper
{
    public static void ArgumentException(string message, string? paramName)
    {
        throw new ArgumentException(message, paramName);
    }

    public static void ArgumentNullException(string? paramName)
    {
        throw new ArgumentNullException(paramName);
    }

    public static void KeyNotFoundException(string? message = null)
    {
        throw new KeyNotFoundException(message);
    }

    public static void InvalidOperationException(string? message = null)
    {
        throw new InvalidOperationException(message);
    }

    public static void JsonException(string? message = null)
    {
        throw new JsonException(message);
    }

    public static void InvalidCastException(string? message = null)
    {
        throw new InvalidCastException(message);
    }

    public static void NotSupportedException(string? message = null)
    {
        throw new NotSupportedException(message);
    }

    public static void BsonSerializationException(string? message = null)
    {
        throw new BsonSerializationException(message);
    }
}
