// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Domain;

public abstract class AppCommandBase<T> : CommandBase<T> where T : class
{
    public string AppId { get; set; }
}
