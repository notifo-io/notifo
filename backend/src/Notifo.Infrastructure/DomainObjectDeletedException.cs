// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure;

[Serializable]
public class DomainObjectDeletedException(string id, Exception? inner = null) : DomainObjectException(FormatMessage(id), id, ValidationError, inner)
{
    private const string ValidationError = "OBJECT_DELETED";

    private static string FormatMessage(string id)
    {
        return $"Domain object \'{id}\' has been deleted.";
    }
}
