// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Notifo.Domain.Integrations.Firebase;

public sealed class FirebaseMessagingWrapper : IDisposable
{
    private readonly FirebaseApp app;

    public FirebaseMessaging Messaging { get; }

    public FirebaseMessagingWrapper(string projectId, string credentials)
    {
        var appOptions = new AppOptions
        {
            ProjectId = projectId,
            // Credentials are provided as a JSON string.
            Credential = GoogleCredential.FromJson(credentials),
        };

        app = FirebaseApp.Create(appOptions, Guid.NewGuid().ToString());

        Messaging = FirebaseMessaging.GetMessaging(app);
    }

    public void Dispose()
    {
        app.Delete();
    }
}
