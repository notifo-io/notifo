// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Topics;

namespace Notifo.Areas.Api.Controllers.EmailPreferences;

public sealed class EmailPreferencesVM
{
    public string AppName { get; set; }

    public Dictionary<Topic, bool> Topics { get; set; }

    public bool EmailsDisabled { get; set; }
}
