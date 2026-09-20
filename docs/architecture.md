# Architecture

This document explains how Notifo is put together and how a notification travels
through the system. It is aimed at people who want to host, extend or debug it.

## The big picture

Notifo is a **single ASP.NET Core application**. The API, the management UI, the
identity server, the background workers and the SignalR hub all run inside the
same process. There is no separate worker service to deploy — scaling out means
running more copies of the same container.

```
                  +---------------------------------------------+
  your backend -> |  POST /api/apps/{appId}/events              |
                  |                                             |
  your website -> |  /build/notifo-sdk.js  ·  /hub (SignalR)    |
                  |                                             |
  your team    -> |  Management UI  ·  /account (OpenID Connect)|
                  +----------------------+----------------------+
                                         |  one process
                  +----------------------+----------------------+
                  |   event pipeline -> user events -> channels |
                  +---+---------------+--------------+----------+
                      |               |              |
                 +----+----+   +------+------+  +----+---------+
                 | storage |   |  messaging  |  | asset store  |
                 | Mongo / |   |  transport  |  | folder / S3 /|
                 | SQL     |   |             |  | Blob / GCS   |
                 +---------+   +-------------+  +--------------+
```

## Projects

The backend solution is `backend/Notifo.slnx`. Dependencies point downwards only.

| Project | Responsibility |
|---|---|
| `Notifo.Infrastructure` | Generic building blocks with no domain knowledge: mediator, messaging, scheduling, asset stores, validation, telemetry. |
| `Notifo.Domain.Integrations.Abstractions` | The contracts a channel provider implements (`IIntegration`, `IEmailSender`, `ISmsSender`, ...) and the `Providers` constants. |
| `Notifo.Domain.Integrations` | The concrete providers — SMTP, Amazon SES, Mailjet, Twilio, Firebase, Telegram, webhooks and so on. |
| `Notifo.Domain` | The product itself: apps, users, topics, subscriptions, templates, media, logs, and the event to notification pipeline. |
| `Notifo.Identity` | Users, roles, external logins and the OpenIddict-based token server. |
| `Notifo.Data.MongoDb` | Repository implementations for MongoDB. |
| `Notifo.Data.EntityFramework` | Repository implementations for MySQL, PostgreSQL and SQL Server. |
| `Notifo` | The API host: controllers, Razor account pages, middleware, DI wiring, static frontend. |

**The core projects never reference a database.** `Notifo.Domain` defines
repository interfaces (`IUserRepository`, `ITopicRepository`, ...) and the two data
projects implement them. Whichever you configure is registered at startup; every
other layer is unaware of the choice.

## How a notification is delivered

This is the path that matters most when something goes wrong.

### 1. Your backend publishes an event

```
POST /api/apps/{appId}/events
{ "requests": [ { "topic": "projects/123/tasks/abc", "preformatted": { ... } } ] }
```

The topic is a path. You invent the scheme; Notifo only does prefix matching on it.

`EventPublisher` validates the message, stamps it with an id and a timestamp,
rejects anything older than one hour, and puts it on the **messaging transport**.
The HTTP request returns as soon as the message is queued — delivery is
asynchronous, which is why the API tests poll for results.

### 2. The event fans out to users

`UserEventConsumer` picks the message up and `UserEventPublisher` resolves it:

- A topic of `users/<id>` (or `user/<id>`) targets exactly that user.
  `users/all` and `users/*` target everyone in the app.
- Any other topic is matched against **subscriptions**. A subscription to
  `projects/123` matches an event on `projects/123/tasks/abc`, because matching
  walks up the path. The most specific matching subscription wins, and its
  per-channel settings are merged with the user's and the app's.
- If the event names a template (`templateCode`), the template supplies the
  formatting; otherwise the `preformatted` payload is used.

One event becomes N **user events**, each of which goes back on the transport.

### 3. Channels do the sending

`UserNotificationService` turns each user event into a `UserNotification` and asks
every registered channel whether it wants it. A channel is skipped unless the
merged settings say `send`, and unless the user has something to send *to* — an
email address, a mobile token, a web push subscription.

The channels are `web`, `webpush`, `mobilepush`, `email`, `sms`, `messaging` and
`webhook`. Each one resolves a configured **integration** for the app and hands
the notification to it.

Most channels derive from `SchedulingChannelBase`, which means the send is put on
a scheduler rather than executed inline. That is what makes two features work:

- **Delays.** A channel can be configured to wait before sending.
- **Confirmation modes.** With `Explicit` or `Seen`, a queued send is dropped if
  the user confirms or sees the notification first — so someone who reads a
  message in the web UI never gets the follow-up email.

Every attempt is written to the log store (visible under *Log* in the UI) and
tracked on the notification, which is where the delivery, seen and confirmed
counters come from.

### 4. Tracking comes back

Notifications carry tracking tokens and tracking URLs. Confirming or seeing one —
via the SDK, a tracking pixel, or the tracking endpoints — updates the
notification and can cancel channel sends that are still queued.

## Pluggable pieces

Four things are chosen by configuration at startup. Getting these combinations
right is most of the work of hosting Notifo, so each is covered in detail in
[configuration.md](configuration.md).

### Storage (`storage:type`)

`MongoDB` or `Sql`. The SQL provider is MySQL, PostgreSQL or SQL Server through
Entity Framework; migrations run on startup by default. Both stores implement the
same repository interfaces, and the API test suite runs against all four databases.

### Messaging (`messaging:type`)

How events travel between the publisher and the consumers, both inside one
instance and between instances.

| Type | Requires | Notes |
|---|---|---|
| `Scheduler` | MongoDB storage | The default. Queues live in the database — no extra infrastructure. |
| `Sql` | SQL storage | The SQL equivalent of `Scheduler`. |
| `RabbitMq` | RabbitMQ | |
| `GooglePubSub` | A GCP project | |
| `Kafka` | Kafka | **Debug builds only** — not compiled into the official Docker image. |

The database-backed transports are deliberately the default: a small deployment
needs nothing but Notifo and its database.

### Asset store (`assetStore:type`)

Where uploaded media lives: `Folder`, `MongoDb` (GridFS), `AmazonS3`, `AzureBlob`,
`GoogleCloud` or `FTP`. The default `Folder` writes to `Assets` inside the
container, so it needs a volume.

Image resizing is done in-process with ImageSharp. If you set
`assets:resizerUrl`, resizing is delegated to a remote resizer service instead,
with the in-process generator as the fallback.

### Clustering (`clustering:type`)

`None` or `Redis`. This is the switch that decides whether you can run more than
one instance — see below.

## Running more than one instance

Notifo keeps a replicated in-memory cache of apps and users, and SignalR holds
web-channel connections in process. Both need a backplane when there is more than
one replica.

- **One instance:** `clustering:type=None` is fine. This is the default.
- **Several instances:** set `clustering:type=Redis`. That wires up the Redis
  SignalR backplane *and* the cache invalidation bus. Without it, replicas serve
  stale apps and users, and web notifications only reach the clients that happen
  to be connected to the instance which handled the event.
- If you cannot use Redis but must run several instances, enable sticky sessions
  at the proxy and set `web:signalR:sticky=true`, or turn SignalR off
  (`web:signalR:enabled=false`) and let the SDK fall back to polling. You still
  lose cache invalidation, so this is a workaround rather than a recommendation.

## Integrations

An integration is a provider for a channel — SMTP for email, Twilio for SMS,
Firebase for mobile push. They are **configured per app** in the management UI,
not in `appsettings.json`: each app picks which providers it uses and supplies its
own credentials.

Implementing one means adding a class in `Notifo.Domain.Integrations` that
exposes an `IntegrationDefinition` (id, title, icon, the properties the UI should
prompt for, and the `Providers.*` it serves) and implements the matching sender
interface. Registering it in `Startup.ConfigureIntegrations` is the last step.

`OpenNotifications` is a bridge to externally hosted providers that speak the
[OpenNotifications](https://github.com/notifo-io/open-notifications) protocol, so
a provider can live outside this repository.

A handful of settings in `appsettings.json` (`email:amazonSES`, `sms:messageBird`)
configure *host-level* integrations, where the operator pays for the provider and
apps use it without supplying their own credentials. Leave them empty unless you
are running Notifo as a service for other people.

## Frontend

`frontend/` is a Vite + React app that builds three bundles into `wwwroot/build/`:

- **the management UI** — the SPA served at `/`,
- **`notifo-sdk.js`** — the embeddable widget for your own website,
- **`notifo-sdk-worker.js`** — the service worker that receives web push.

The SDK talks to the same API with a **user API key**, through the `/api/me/*`
endpoints. Because it registers a service worker, the site embedding it must be
served over HTTPS.

## Where to look when debugging

| Symptom | Look at |
|---|---|
| Event accepted but nothing happens | *Log* in the UI, then the transport — is a consumer running? |
| Notification created but no email or SMS | *Log*, the channel settings on user, subscription and app, and whether the integration is enabled. |
| Web notifications only reach some users | The SignalR backplane — `clustering:type`. |
| Images are served unresized | `assets:resizerUrl` points somewhere unreachable; failures fall back to the original silently. |
| Health | `GET /healthz` — a JSON report, currently a process-memory check only, not a database probe. |
| API reference | `GET /api/docs` (ReDoc) and `/api/openapi.json`. |
