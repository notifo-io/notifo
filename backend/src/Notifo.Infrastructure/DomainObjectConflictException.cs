// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure;

[Serializable]
public class DomainObjectConflictException : DomainObjectException
{
    private const string ValidationError = "OBJECT_CONFLICT";

    public DomainObjectConflictException(string id, Exception? inner = null)
        : base(FormatMessage(id), id, ValidationError, inner)
    {
    }

    private static string FormatMessage(string id)
    {
        return $"Domain object \'{id}\' already exists";
    }
}
