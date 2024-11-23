// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure;

[Serializable]
public class DomainForbiddenException(string message, Exception? inner = null) : DomainException(message, ValidationError, inner)
{
    private const string ValidationError = "FORBIDDEN";
}
