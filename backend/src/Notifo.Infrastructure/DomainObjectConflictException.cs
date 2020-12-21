// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.Serialization;

namespace Notifo.Infrastructure
{
    [Serializable]
    public class DomainObjectConflictException : DomainObjectException
    {
        public DomainObjectConflictException(string id, Exception? inner = null)
            : base(FormatMessage(id), id, inner)
        {
        }

        protected DomainObjectConflictException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string FormatMessage(string id)
        {
            return $"Domain object \'{id}\' already exists";
        }
    }
}
