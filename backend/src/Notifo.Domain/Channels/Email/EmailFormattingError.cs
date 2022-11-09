// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Notifo.Domain.Channels.Email;

[Serializable]
public sealed record EmailFormattingError(string Message, EmailTemplateType Template, int Line = -1, int Column = -1);
