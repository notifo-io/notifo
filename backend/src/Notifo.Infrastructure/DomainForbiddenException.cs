// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Notifo.Infrastructure
{
    [Serializable]
    public class DomainForbiddenException : DomainException
    {
        public DomainForbiddenException(string message)
            : base(message)
        {
        }

        public DomainForbiddenException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DomainForbiddenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
