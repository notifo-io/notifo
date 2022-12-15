// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

using Notifo.Domain.Utils;

namespace Notifo.Domain.Channels.Email;

[Serializable]
public sealed record EmailFormattingError(EmailTemplateType Template, TemplateError Error)
{
    public EmailFormattingError(EmailTemplateType template, string error)
        : this(template, new TemplateError(error))
    {
    }
}
