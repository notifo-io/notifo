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
    public class DomainObjectNotFoundException : DomainObjectException
    {
        public DomainObjectNotFoundException(string id, Exception? inner = null)
            : base(FormatMessage(id), id, inner)
        {
        }

        protected DomainObjectNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string FormatMessage(string id)
        {
            return $"Domain object \'{id}\' not found.";
        }
    }
}
