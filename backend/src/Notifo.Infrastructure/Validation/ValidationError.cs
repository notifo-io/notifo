// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;

namespace Notifo.Infrastructure.Validation
{
    [Serializable]
    public sealed class ValidationError
    {
        private readonly string message;
        private readonly string[] propertyNames;

        public string Message
        {
            get { return message; }
        }

        public IEnumerable<string> PropertyNames
        {
            get { return propertyNames; }
        }

        public ValidationError(string message, params string[] propertyNames)
        {
            Guard.NotNullOrEmpty(message, nameof(message));

            this.message = message;

            this.propertyNames = propertyNames ?? Array.Empty<string>();
        }
    }
}
