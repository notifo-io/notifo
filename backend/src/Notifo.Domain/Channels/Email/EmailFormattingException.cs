// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using System.Text;

namespace Notifo.Domain.Channels.Email
{
    [Serializable]
    public class EmailFormattingException : Exception
    {
        public IReadOnlyList<EmailFormattingError> Errors { get; }

        public EmailFormattingException(string template, IEnumerable<EmailFormattingError> errors)
            : base(BuildErrorMessage(errors, template))
        {
            Errors = errors.ToList();
        }

        protected EmailFormattingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Errors = info.GetValue(nameof(Errors), typeof(List<EmailFormattingError>)) as List<EmailFormattingError> ?? new List<EmailFormattingError>();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Errors), Errors.ToList());
        }

        private static string BuildErrorMessage(IEnumerable<EmailFormattingError> errors, string template)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Failed to parse template");

            foreach (var error in errors)
            {
                sb.Append(" * ");
                sb.AppendLine(error.Message);
            }

            sb.AppendLine();
            sb.AppendLine("Template:");
            sb.AppendLine(template);

            return sb.ToString();
        }
    }
}
