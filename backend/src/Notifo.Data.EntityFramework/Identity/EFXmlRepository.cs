// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Notifo.Infrastructure;

namespace Notifo.Identity;

public sealed class EFXmlRepository<TContext>(IDbContextFactory<TContext> dbContextFactory) : IXmlRepository where TContext : DbContext
{
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        var entities = dbContext.Set<EFXmlEntity>().ToList();

        return entities.Select(x => XElement.Parse(x.Xml)).ToList();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        // Serialize the XML to a string value.
        var xml = element.ToString();

        var updated =
            dbContext.Set<EFXmlEntity>().Where(x => x.FriendlyName == friendlyName)
                .ExecuteUpdate(u => u.SetProperty(x => x.Xml, xml));

        if (updated > 0)
        {
            return;
        }

        try
        {
            dbContext.Set<EFXmlEntity>().Add(new EFXmlEntity { FriendlyName = friendlyName, Xml = xml });
            dbContext.SaveChanges();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            dbContext.ChangeTracker.Clear();

            dbContext.Set<EFXmlEntity>().Where(x => x.FriendlyName == friendlyName)
                .ExecuteUpdate(u => u.SetProperty(x => x.Xml, xml));
        }
    }
}
