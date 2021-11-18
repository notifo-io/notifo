// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;

namespace Notifo.Infrastructure
{
    [Serializable]
    public class DomainObjectDeletedException : DomainObjectException
    {
        public DomainObjectDeletedException(string id, Exception? inner = null)
            : base(FormatMessage(id), id, inner)
        {
        }

        protected DomainObjectDeletedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string FormatMessage(string id)
        {
            return $"Domain object \'{id}\' has been deleted.";
        }
    }
}
