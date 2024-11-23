// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Notifo.Domain.Channels.Email;

[Serializable]
public class EmailFormattingException(IEnumerable<EmailFormattingError> errors) : Exception(BuildErrorMessage(errors))
{
    public IReadOnlyList<EmailFormattingError> Errors { get; } = errors.ToList();

    private static string BuildErrorMessage(IEnumerable<EmailFormattingError> errors)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Failed to parse template");

        foreach (var error in errors)
        {
            sb.Append(" * ");
            sb.Append("Position: ");
            sb.Append(error.Error.LineNumber);
            sb.Append(", ");
            sb.Append(error.Error.LinePosition);
            sb.Append(", ");
            sb.Append("Template: ");
            sb.Append(error.Template);
            sb.Append(", ");
            sb.Append(error.Error.Message);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
