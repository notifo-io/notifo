// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Pipeline;

public sealed class AutorizeAppUserAttribute(string role, params string[] extraRoles) 
    : AuthorizeUserAttribute
{
    public string[] RequiredAppRoles { get; } = new string[] { role }.Concat(extraRoles).ToArray();
}
