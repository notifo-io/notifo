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

namespace Notifo.Domain.Users;

[Table("UserProperties")]
[PrimaryKey(nameof(UserDocId), nameof(Key))]
[Index(nameof(AppId), nameof(Key), nameof(Value))]
public sealed class EFUserPropertyEntity
{
    public const int MaxKeyLength = FieldLengths.Id;
    public const int MaxValueLength = FieldLengths.Key;

    [MaxLength(FieldLengths.Key)]
    public string UserDocId { get; set; }

    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(MaxKeyLength)]
    public string Key { get; set; }

    [MaxLength(MaxValueLength)]
    public string Value { get; set; }
}
