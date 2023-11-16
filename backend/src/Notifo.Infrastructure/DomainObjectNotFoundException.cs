// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure;

[Serializable]
public class DomainObjectNotFoundException : DomainObjectException
{
    private const string ValidationError = "OBJECT_NOTFOUND";

    public DomainObjectNotFoundException(string id, Exception? inner = null)
        : base(FormatMessage(id), id, ValidationError, inner)
    {
    }

    private static string FormatMessage(string id)
    {
        return $"Domain object \'{id}\' not found.";
    }
}
