// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using System.Text;

namespace Notifo.Domain.Channels.Email;

[Serializable]
public class EmailFormattingException : Exception
{
    public IReadOnlyList<EmailFormattingError> Errors { get; }

    public EmailFormattingException(IEnumerable<EmailFormattingError> errors)
        : base(BuildErrorMessage(errors))
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

    private static string BuildErrorMessage(IEnumerable<EmailFormattingError> errors)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Failed to parse template");

        foreach (var error in errors)
        {
            sb.Append(" * ");
            sb.Append("Line: ");
            sb.Append(error.Line);
            sb.Append(", ");
            sb.Append("Template: ");
            sb.Append(error.Template);
            sb.Append(", ");
            sb.Append(error.Message);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
