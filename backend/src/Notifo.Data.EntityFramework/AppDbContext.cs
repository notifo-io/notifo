// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Apps;
using Notifo.Domain.ChannelTemplates;
using Notifo.Domain.Events;
using Notifo.Domain.Log;
using Notifo.Domain.Media;
using Notifo.Domain.Subscriptions;
using Notifo.Domain.Templates;
using Notifo.Domain.Topics;
using Notifo.Domain.UserNotifications;
using Notifo.Domain.Users;
using Notifo.Identity;
using Notifo.Infrastructure;
using Notifo.Infrastructure.KeyValueStore;
using Notifo.Infrastructure.Scheduling.Implementation.TimerBased;
using OpenIddict.EntityFrameworkCore.Models;

namespace Notifo;

public abstract class AppDbContext(DbContextOptions options, JsonSerializerOptions jsonOptions) : IdentityDbContext(options)
{
    public DbSet<EFAppApiKeyEntity> AppApiKeys { get; set; }

    public DbSet<EFAppContributorEntity> AppContributors { get; set; }

    public DbSet<EFAppEntity> Apps { get; set; }

    public DbSet<EFChannelTemplateEntity> ChannelTemplates { get; set; }

    public DbSet<EFConfigurationEntity> Configurations { get; set; }

    public DbSet<EFEventEntity> Events { get; set; }

    public DbSet<EFKeyEntity> Keys { get; set; }

    public DbSet<EFKeyValueEntity> KeyValues { get; set; }

    public DbSet<EFLogEntity> Log { get; set; }

    public DbSet<EFMediaEntity> Media { get; set; }

    public DbSet<EFSchedulerEntity> Scheduler { get; set; }

    public DbSet<EFSubscriptionEntity> Subscriptions { get; set; }

    public DbSet<EFTemplateEntity> Templates { get; set; }

    public DbSet<EFTopicEntity> Topics { get; set; }

    public DbSet<EFUserNotificationEntity> UserNotifications { get; set; }

    public DbSet<EFUserPropertyEntity> UserProperties { get; set; }

    public DbSet<EFUserEntity> AppUsers { get; set; }

    public DbSet<EFXmlEntity> XmlElements { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.UseInstantAsDateTimeOffset();

        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must be the first step, so that the JSON properties are not mapped as related entities.
        builder.UseJsonAttributes(jsonOptions);

        base.OnModelCreating(builder);

        builder.UseMessagingTransport();
        builder.UseOpenIddict();

        builder.Entity<OpenIddictEntityFrameworkCoreToken>(b =>
        {
            // OpenIddict 7 stores token type URNs with up to 57 characters, but the fork still limits the column to 50.
            b.Property(x => x.Type).HasMaxLength(150);

            if (Database.IsMySql())
            {
                // Keeps the index below the maximum key length of 3072 bytes with utf8mb4. Token types are unique within 50 characters.
                b.HasIndex(x => new { x.ApplicationId, x.Status, x.Subject, x.Type }).HasPrefixLength(0, 0, 0, 50);
            }
        });
    }
}
