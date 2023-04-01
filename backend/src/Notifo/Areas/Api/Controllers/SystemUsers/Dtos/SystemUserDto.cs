// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Identity;
using Notifo.Infrastructure.Reflection;

namespace Notifo.Areas.Api.Controllers.SystemUsers.Dtos;

public sealed class SystemUserDto
{
    /// <summary>
    /// The id of the user.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The email of the user. Unique value.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Determines if the user is locked.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Additional role for the user.
    /// </summary>
    public IReadOnlySet<string> Roles { get; set; }

    /// <summary>
    /// True if the user can be updated.
    /// </summary>
    public bool CanUpdate { get; set; }

    public static SystemUserDto FromDomainObject(IUser source, bool canUpdate)
    {
        var result = SimpleMapper.Map(source, new SystemUserDto());

        result.CanUpdate = canUpdate;

        return result;
    }
}
