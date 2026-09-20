// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Notifo.Infrastructure;

namespace Notifo.Domain.Apps;

[Table("AppContributors")]
[PrimaryKey(nameof(AppId), nameof(ContributorId))]
[Index(nameof(ContributorId))]
public sealed class EFAppContributorEntity
{
    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(FieldLengths.Id)]
    public string ContributorId { get; set; }
}
