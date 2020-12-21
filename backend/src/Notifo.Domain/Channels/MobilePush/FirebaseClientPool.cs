// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Notifo.Domain.Apps;

namespace Notifo.Domain.Channels.MobilePush
{
    public sealed class FirebaseClientPool
    {
        private readonly IMemoryCache memoryCache;

        public FirebaseClientPool()
        {
            memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }

        public FirebaseMessaging GetMessaging(App app)
        {
            var messaging = memoryCache.GetOrCreate(app.FirebaseProject, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var firebaseApp = FirebaseApp.GetInstance(app.Id);

                if (firebaseApp == null)
                {
                    var appOptions = new AppOptions
                    {
                        Credential = GoogleCredential.FromJson(app.FirebaseCredential)
                    };

                    appOptions.ProjectId = app.FirebaseProject;

                    firebaseApp = FirebaseApp.Create(appOptions);
                }

                entry.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (key, value, reason, state) =>
                    {
                        firebaseApp.Delete();
                    }
                });

                return FirebaseMessaging.GetMessaging(firebaseApp);
            });

            return messaging;
        }
    }
}
