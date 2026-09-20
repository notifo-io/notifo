// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;

namespace Notifo.Domain.Users;

[Table("Users")]
[Index(nameof(AppId), nameof(UserId))]
[Index(nameof(ApiKey), IsUnique = true)]
public sealed class EFUserEntity : EFEntity<User>, IEFCounterEntity
{
    [MaxLength(FieldLengths.AppId)]
    public string AppId { get; set; }

    [MaxLength(FieldLengths.Id)]
    public string UserId { get; set; }

    [MaxLength(FieldLengths.Id)]
    public string? ApiKey { get; set; }

    [MaxLength(FieldLengths.Text)]
    public string? FullName { get; set; }

    [MaxLength(FieldLengths.Text)]
    public string? EmailAddress { get; set; }

    [Json]
    [MaxLength(FieldLengths.LongText)]
    public CounterMap? Counters { get; set; }

    public long CountersVersion { get; set; }

    public static string CreateId(string appId, string id)
    {
        return $"{appId}_{id}";
    }

    public static EFUserEntity FromUser(User user)
    {
        return new EFUserEntity
        {
            ApiKey = user.ApiKey,
            AppId = user.AppId,
            Counters = user.Counters,
            CountersVersion = 0,
            Doc = user,
            DocId = CreateId(user.AppId, user.Id),
            EmailAddress = user.EmailAddress.ToMaxLength(FieldLengths.Text),
            Etag = GenerateEtag(),
            FullName = user.FullName.ToMaxLength(FieldLengths.Text),
            UserId = user.Id,
        };
    }

    public User ToUser()
    {
        return Doc with { Counters = Counters ?? [] };
    }
}
