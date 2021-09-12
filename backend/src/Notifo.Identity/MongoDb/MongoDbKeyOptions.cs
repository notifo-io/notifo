// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Notifo.Infrastructure;
using OpenIddict.Server;

namespace Notifo.Identity.MongoDb
{
    public sealed class MongoDbKeyOptions : IConfigureOptions<OpenIddictServerOptions>
    {
        private readonly IMongoDatabase database;

        public MongoDbKeyOptions(IMongoDatabase database)
        {
            this.database = database;
        }

        public void Configure(OpenIddictServerOptions options)
        {
            var collection = database.GetCollection<MongoDbKey>("Identity_Key6");

            var key = collection.Find(x => x.Id == "Default").FirstOrDefault();

            RsaSecurityKey securityKey;

            if (key == null)
            {
                securityKey = new RsaSecurityKey(RSA.Create(2048))
                {
                    KeyId = RandomHash.New()
                };

                key = new MongoDbKey { Id = "Default", Key = securityKey.KeyId };

                if (securityKey.Rsa != null)
                {
                    var parameters = securityKey.Rsa.ExportParameters(true);

                    key.Parameters = MongoDbKeyParameters.Create(parameters);
                }
                else
                {
                    key.Parameters = MongoDbKeyParameters.Create(securityKey.Parameters);
                }

                try
                {
                    collection.InsertOne(key);
                }
                catch (MongoWriteException ex)
                {
                    if (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                    {
                        key = collection.Find(x => x.Id == "Default").FirstOrDefault();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (key == null)
            {
                throw new InvalidOperationException("Cannot read key.");
            }

            securityKey = new RsaSecurityKey(key.Parameters.ToParameters())
            {
                KeyId = key.Key
            };

            options.SigningCredentials.Add(
                new SigningCredentials(securityKey,
                    SecurityAlgorithms.RsaSha256));

            options.EncryptionCredentials.Add(new EncryptingCredentials(securityKey,
                SecurityAlgorithms.RsaOAEP,
                SecurityAlgorithms.Aes256CbcHmacSha512));
        }
    }
}
