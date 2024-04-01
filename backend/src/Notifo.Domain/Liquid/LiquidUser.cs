// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Users;
using Notifo.Infrastructure;

namespace Notifo.Domain.Liquid;

public sealed class LiquidUser(User user)
{
    private readonly User user = user;

    public string? FullName => user.FullName.OrNull();

    public string? EmailAddress => user.EmailAddress.OrNull();

    public string? PhoneNumber => user.PhoneNumber.OrNull();

    public static void Describe(LiquidProperties properties)
    {
        properties.AddString("fullName",
            "The full name of the user.");

        properties.AddString("emailAddress",
            "The email address of the user.");

        properties.AddString("phoneNumber",
            "The phone number of the user.");
    }
}
