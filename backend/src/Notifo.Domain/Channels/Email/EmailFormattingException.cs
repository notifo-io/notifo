﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
