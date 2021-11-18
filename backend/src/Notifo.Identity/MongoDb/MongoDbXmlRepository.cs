// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Identity.MongoDb
{
    public sealed class MongoDbXmlRepository : MongoDbRepository<MongoDbXmlEntity>, IXmlRepository
    {
        public MongoDbXmlRepository(IMongoDatabase database)
            : base(database)
        {
            InitializeAsync(default).Wait();
        }

        protected override string CollectionName()
        {
            return "Xml";
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var documents = Collection.Find(new BsonDocument()).ToList();

            var elements = documents.Select(x => XElement.Parse(x.Xml)).ToList();

            return elements;
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            var document = new MongoDbXmlEntity
            {
                FriendlyName = friendlyName
            };

            document.Xml = element.ToString();

            Collection.ReplaceOne(x => x.FriendlyName == friendlyName, document, UpsertReplace);
        }
    }
}
