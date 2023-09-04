// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Channels.Email;
using Notifo.Infrastructure.Reflection;

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
    public int LineNumber { get; set; }

    /// <summary>
    /// The line column.
    /// </summary>
    public int LinePosition { get; set; }

    public static EmailPreviewErrorDto FromDomainObject(EmailFormattingError source)
    {
        return SimpleMapper.Map(source.Error, new EmailPreviewErrorDto());
    }
}
