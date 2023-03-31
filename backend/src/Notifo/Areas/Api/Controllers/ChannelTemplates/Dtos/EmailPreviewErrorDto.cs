// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Email;

namespace Notifo.Areas.Api.Controllers.ChannelTemplates.Dtos;

public sealed class EmailPreviewErrorDto
{
    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// The line number.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// The line column.
    /// </summary>
    public int Column { get; set; }

    public static EmailPreviewErrorDto FromDomainObject(EmailFormattingError source)
    {
        var error = source.Error;

        return new EmailPreviewErrorDto { Message = error.Message, Column = error.Column, Line = error.Line };
    }
}
