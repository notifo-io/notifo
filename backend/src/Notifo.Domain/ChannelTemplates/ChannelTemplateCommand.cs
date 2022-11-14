// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.ChannelTemplates;

public abstract class ChannelTemplateCommand<T> : AppCommandBase<ChannelTemplate<T>>
{
    public string TemplateCode { get; set; }
}
