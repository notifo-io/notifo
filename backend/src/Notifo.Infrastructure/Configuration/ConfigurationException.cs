// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Notifo.Infrastructure.Configuration
{
    [Serializable]
    public class ConfigurationException : Exception
    {
        public IReadOnlyList<ConfigurationError> Errors { get; }

        public ConfigurationException(ConfigurationError error, Exception? inner = null)
            : this(new List<ConfigurationError> { error }, inner)
        {
        }

        public ConfigurationException(IReadOnlyList<ConfigurationError> errors, Exception? inner = null)
            : base(FormatMessage(errors), inner)
        {
            Errors = errors;
        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Errors = (List<ConfigurationError>)info.GetValue(nameof(Errors), typeof(List<ConfigurationError>))!;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Errors), Errors);

            base.GetObjectData(info, context);
        }

        private static string FormatMessage(IReadOnlyList<ConfigurationError> errors)
        {
            Guard.NotNull(errors, nameof(errors));

            var sb = new StringBuilder();

            foreach (var error in errors)
            {
                sb.AppendLine(error.ToString());
            }

            return sb.ToString();
        }
    }
}
