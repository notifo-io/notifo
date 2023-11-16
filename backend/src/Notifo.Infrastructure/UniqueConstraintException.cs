// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure;

[Serializable]
public class UniqueConstraintException : Exception
{
    public UniqueConstraintException()
    {
    }

    public UniqueConstraintException(string message)
        : base(message)
    {
    }

    public UniqueConstraintException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
