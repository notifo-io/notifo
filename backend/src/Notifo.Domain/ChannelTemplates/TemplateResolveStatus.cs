// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.ChannelTemplates;

public enum TemplateResolveStatus
{
    Resolved,
    ResolvedWithFallback,
    NotFound,
    LanguageNotFound
}
