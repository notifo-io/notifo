// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain.Templates;

public abstract class TemplateCommand : AppCommandBase<Template>
{
    public string TemplateCode { get; set; }
}
