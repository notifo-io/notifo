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

namespace Notifo.Identity;

[Table("Identity_Configuration")]
[Index(nameof(Expires))]
public sealed class EFConfigurationEntity
{
    [Key]
    [MaxLength(FieldLengths.Key)]
    public string Id { get; set; }

    [Json]
    public string Value { get; set; }

    public DateTimeOffset Expires { get; set; }
}
