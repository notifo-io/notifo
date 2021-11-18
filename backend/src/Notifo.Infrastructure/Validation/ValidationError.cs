// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure.Validation
{
    [Serializable]
    public sealed class ValidationError
    {
        private readonly string message;
        private readonly string[] propertyNames;

        public string Message
        {
            get => message;
        }

        public IEnumerable<string> PropertyNames
        {
            get => propertyNames;
        }

        public ValidationError(string message, params string[] propertyNames)
        {
            Guard.NotNullOrEmpty(message, nameof(message));

            this.message = message;

            this.propertyNames = propertyNames ?? Array.Empty<string>();
        }
    }
}
