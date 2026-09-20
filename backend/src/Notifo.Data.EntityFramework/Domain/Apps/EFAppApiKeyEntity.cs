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

[Table("AppApiKeys")]
[Index(nameof(AppId))]
public sealed class EFAppApiKeyEntity
{
    [Key]
    [MaxLength(FieldLengths.Id)]
    public string ApiKey { get; set; }

    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }
}
