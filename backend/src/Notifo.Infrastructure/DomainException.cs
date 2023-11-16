// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure;

[Serializable]
public class DomainException : Exception
{
    public string? ErrorCode { get; }

    public DomainException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }

    public DomainException(string message, string? errorCode, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}
