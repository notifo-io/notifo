// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Areas.Api.Controllers.EmailPreferences;

public sealed class EmailPreferencesModel
{
    public Dictionary<string, bool>? Topics { get; set; }

    public bool All { get; set; }
}
