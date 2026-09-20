# Configuration

Notifo is configured through [`backend/src/Notifo/appsettings.json`](../backend/src/Notifo/appsettings.json),
which is the authoritative list — it carries a comment on nearly every setting.
This page covers what a self-hoster actually has to decide, and the rules that
constrain those decisions.

## How settings are supplied

Standard ASP.NET Core configuration, in increasing order of precedence:

1. `appsettings.json` baked into the image
2. `appsettings.Production.json`, if you mount one
3. environment variables
4. command-line arguments

Environment variables are the usual choice for containers. A nested setting maps
to an environment variable by joining the path with a **double underscore** and
upper-casing it:

```json
"storage": {
    "mongoDB": {
        "connectionString": "mongodb://localhost"
    }
}
```

```
STORAGE__MONGODB__CONNECTIONSTRING=mongodb://localhost
```

Array entries use a numeric segment: `IDENTITY__USERS__0__EMAIL`.

Notifo writes its effective configuration to the log at startup, which is the
quickest way to confirm that a variable arrived. Values are **not** redacted, so
connection strings, client secrets and the web push private key appear there —
treat the startup log as sensitive and keep it out of shared log sinks if that
matters to you.

## Things you must set

| Setting | Why |
|---|---|
| `urls:baseUrl` | The public URL, exactly as browsers see it. Used for the OpenID Connect issuer, tracking URLs, media URLs and links in emails. Wrong value means broken logins and broken links. |
| `storage:*` | Where the data lives. |
| `webPush:vapid*` | The defaults are published in this repository. See [self-hosting.md](self-hosting.md#replace-the-web-push-keys--required). |
| `identity:githubClient` etc. | The defaults point at notifo.io's OAuth apps and will not work on your domain. Replace or blank them. |

## Storage

```json
"storage": {
    "type": "MongoDB",
    "mongoDB": { "connectionString": "mongodb://localhost", "databaseName": "Notifications" },
    "sql": { "provider": "Postgres", "connectionString": "...", "version": "", "runMigration": true }
}
```

`type` is `MongoDB` or `Sql`. For `Sql`, `provider` is `MySql`, `Postgres` or
`SqlServer`.

- Migrations run at startup. Set `runMigration` to `false` to apply them out of
  band, but then a deploy with pending migrations will fail at runtime.
- `version` is only for MySQL, and is auto-detected when left empty.
- MySQL also needs the server started with `--local-infile=1` and
  `AllowLoadLocalInfile=true` in the connection string, because bulk inserts use
  `LOAD DATA LOCAL INFILE`.

## Messaging

```json
"messaging": { "type": "Scheduler" }
```

This is the transport that carries events between the publisher and the
consumers. **Two combinations are rejected at startup**, with an error naming the
setting:

| `storage:type` | Allowed `messaging:type` |
|---|---|
| `MongoDB` | `Scheduler` (default), `RabbitMq`, `GooglePubSub` |
| `Sql` | `Sql`, `RabbitMq`, `GooglePubSub` |

`Scheduler` and `Sql` store the queues in the application database, so a small
deployment needs no broker at all.

`Kafka` appears in `appsettings.json` but is compiled only into **Debug** builds.
It is not present in the official Docker image.

RabbitMQ needs `messaging:rabbitMq:uri`; Google Pub/Sub needs
`messaging:googlePubSub:projectId` and a `prefix`.

## Asset store

```json
"assetStore": { "type": "Folder", "folder": { "path": "Assets" } }
```

`Folder`, `MongoDb` (GridFS), `AmazonS3`, `AzureBlob`, `GoogleCloud` or `FTP`.

- `Folder` is the default and writes inside the container — mount a volume at
  `/app/Assets`, or media disappears on every restart.
- `MongoDb` is rejected when `storage:type` is `Sql`.
- Run more than one instance and local disk stops working; use a cloud bucket.

### Image resizing

```json
"assets": { "resizerUrl": "" }
```

Resizing happens in-process with ImageSharp. Set `resizerUrl` to delegate it to a
remote resizer service (for example `squidex/resizer`), with the in-process
generator as the fallback.

Two things to know: the URL must be reachable **from inside the container**, so
inside Docker use the service's internal port rather than a published host port;
and if the resizer is unreachable, Notifo logs the failure and serves the
*original, unresized* image with a `200`. A misconfigured resizer is therefore
silent from the client's point of view.

## Clustering

```json
"clustering": { "type": "None", "redis": { "connectionString": "localhost" } }
```

`None` is correct for a single instance. Set `Redis` before running more than
one — it provides the SignalR backplane and the cache invalidation bus. See
[architecture.md](architecture.md#running-more-than-one-instance).

## Identity

```json
"identity": {
    "allowPasswordAuth": true,
    "adminClientId": "",
    "adminClientSecret": "",
    "githubClient": "...", "githubSecret": "...",
    "googleClient": "...", "googleSecret": "...",
    "oidcName": "OIDC", "oidcAuthority": "", "oidcClient": "", "oidcSecret": "",
    "users": [ { "email": "admin@notifo.io", "password": "" } ]
}
```

| Setting | Notes |
|---|---|
| `allowPasswordAuth` | `false` disables local accounts, leaving only external login. Verify SSO works first. |
| `adminClientId` / `adminClientSecret` | Creates an OAuth client-credentials client holding the host-admin role. Useful for automation, dangerous to leak. Leave empty if unused. |
| `githubClient`, `googleClient` + secrets | Social login. The committed defaults belong to notifo.io and will not work for your domain. These two are the only built-in social providers; anything else goes through `oidc*`. |
| `oidcAuthority`, `oidcClient`, `oidcSecret` | Your own identity provider. `oidcScopes`, `oidcResponseType` (`id_token` or `code`) and `oidcGetClaimsFromUserInfoEndpoint` cover the usual variations. |
| `users` | Seed accounts created at startup — `email`, `password`, optional `role`, optional `passwordReset`. An alternative to the `/account/setup` wizard. The first user in the installation gets the host-admin role automatically; later ones need `role` set. `passwordReset: true` rewrites the password of an existing account on every start. |

If no user exists, any UI request is redirected to `/account/setup` to create the
first administrator.

## Web channel and SignalR

```json
"web": { "signalR": { "enabled": true, "sticky": false, "pollingInterval": 5000 } }
```

- `enabled: false` turns off WebSockets; the SDK polls every `pollingInterval`
  milliseconds instead.
- `sticky: true` tells Notifo that the load balancer pins a client to one
  instance, which lets SignalR work across replicas without a Redis backplane.
  It does not fix cache invalidation.

## Web push

```json
"webPush": { "subject": "https://notifo.io", "vapidPublicKey": "...", "vapidPrivateKey": "..." }
```

Generate your own pair — `npx web-push generate-vapid-keys` — and set `subject`
to your own URL or a `mailto:` address. Changing the public key invalidates every
existing browser subscription, so do it before anyone subscribes.

## Outbound request protection (SSRF)

```json
"ssrf": {
    "enableDnsRebindingProtection": true,
    "allowedSchemes": [ "http", "https" ],
    "blockedIpAddresses": [ "169.254.169.254" ],
    "whiteListedHosts": [],
    "allowAutoRedirect": false
}
```

Webhooks and remote media make Notifo fetch URLs that users control, so these
defaults are deliberately strict. Keep `whiteListedHosts` empty in production;
add entries only for internal hosts you deliberately want reachable. The test
compose files set it to `*`, which is fine for a disposable container and wrong
anywhere else.

## Logging and telemetry

```json
"logging": {
    "human": false,
    "otlp": { "enabled": false, "endpoint": "", "sampling": 1.0 },
    "applicationInsights": { "enabled": false, "connectionString": "" },
    "stackdriver": { "enabled": false }
}
```

- `human: true` prints readable logs instead of JSON — useful locally, noisy for
  log shippers.
- `otlp` exports OpenTelemetry traces. `sampling: 0.5` keeps every second trace.
- Standard `logging:logLevel` entries work as in any ASP.NET Core app.

## Diagnostics

```json
"diagnostics": {
    "dumpTriggerInMB": 0,
    "gcumpTriggerInMB": 0,
    "gc": { "threshold": 8192 }
}
```

The dotnet-dump and dotnet-gcdump tools are installed in the official image and
their paths are pre-set. A non-zero trigger writes a dump to the asset store the
first time the process exceeds that many megabytes. `gc:threshold` is the memory
ceiling above which `/healthz` reports unhealthy — raise it if your container
limit is higher than 8 GB, lower it if it is smaller.

## Host-level integrations

```json
"email": { "amazonSES": { ... } },
"sms":   { "messageBird": { ... } }
```

These configure providers that the *operator* pays for, so apps can use them
without supplying their own credentials. For a private installation, leave them
empty and configure integrations per app in the management UI instead.

## A worked example

A production-shaped deployment on PostgreSQL with S3 media, Redis clustering and
OIDC login:

```yml
environment:
  - URLS__BASEURL=https://notifo.example.com

  - STORAGE__TYPE=Sql
  - STORAGE__SQL__PROVIDER=Postgres
  - STORAGE__SQL__CONNECTIONSTRING=Server=db;Port=5432;Database=notifo;Username=notifo;Password=${DB_PASSWORD}
  - MESSAGING__TYPE=Sql

  - ASSETSTORE__TYPE=AmazonS3
  - ASSETSTORE__AMAZONS3__BUCKET=notifo-assets
  - ASSETSTORE__AMAZONS3__REGIONNAME=eu-central-1
  - ASSETSTORE__AMAZONS3__ACCESSKEY=${S3_KEY}
  - ASSETSTORE__AMAZONS3__SECRETKEY=${S3_SECRET}

  - CLUSTERING__TYPE=Redis
  - CLUSTERING__REDIS__CONNECTIONSTRING=redis:6379

  - WEBPUSH__SUBJECT=https://notifo.example.com
  - WEBPUSH__VAPIDPUBLICKEY=${VAPID_PUBLIC}
  - WEBPUSH__VAPIDPRIVATEKEY=${VAPID_PRIVATE}

  - IDENTITY__ALLOWPASSWORDAUTH=false
  - IDENTITY__OIDCNAME=Corporate SSO
  - IDENTITY__OIDCAUTHORITY=https://login.example.com
  - IDENTITY__OIDCCLIENT=${OIDC_CLIENT}
  - IDENTITY__OIDCSECRET=${OIDC_SECRET}
  - IDENTITY__GITHUBCLIENT=
  - IDENTITY__GITHUBSECRET=
  - IDENTITY__GOOGLECLIENT=
  - IDENTITY__GOOGLESECRET=
```
