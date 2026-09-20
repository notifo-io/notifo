# Notifo - Notification Service

[![Release](https://github.com/notifo-io/notifo/actions/workflows/release.yml/badge.svg)](https://github.com/notifo-io/notifo/actions/workflows/release.yml)
[![Docker Image Version (latest by date)](https://img.shields.io/docker/v/squidex/notifo?sort=date)](https://hub.docker.com/r/squidex/notifo)

Notifo is a multi-channel notification service for collaboration tools, e-commerce, news, magazines and everybody who wants to notify their users.

Try it out at <https://app.notifo.io>, or [host it yourself](docs/self-hosting.md).

![Notifo Tour](https://github.com/notifo-io/notifo/raw/main/media/tour/Notifo.gif "Notifo Tour")

## Documentation

| | |
|---|---|
| [**Self-hosting**](docs/self-hosting.md) | Run your own instance: requirements, Docker Compose, TLS, the secrets you must replace, backups and upgrades. |
| [**Configuration**](docs/configuration.md) | Every setting that matters, how environment variables map to it, and which combinations are not allowed. |
| [**Architecture**](docs/architecture.md) | How the pieces fit together and how a notification travels from your API call to a user's inbox. |

The API reference of a running instance is at `/api/docs`, generated from
`/api/openapi.json`.

## Features

* Powerful and rich REST API with OpenAPI documentation.
* Management UI for notification templates, users, subscriptions, apps, settings and email templates.
* Email templates with MJML and Liquid templates.
* Rich notifications with a lot of formatting options like small and large images.
* Abstraction over multiple channels and providers.
* Reliable through retry mechanisms and message queues for all notifications and channels.
* Tracking which notification has been delivered, seen or confirmed.
* Embeddable JavaScript SDK that adds a notification overlay to your web application.

### Channels and providers

| Channel | Providers |
|---|---|
| **Email** | SMTP, Amazon SES, Mailjet, Mailchimp |
| **SMS** | Twilio, MessageBird, Seven, Telekom |
| **Mobile push** | Firebase |
| **Messaging** | Telegram, Discord, Threema, WhatsApp (via MessageBird) |
| **Web** | SignalR / WebSockets, with polling as a fallback |
| **Web push** | Built in, using VAPID |
| **Webhook** | Any HTTP endpoint |

> Telekom sends SMS, but currently lists itself under *Messaging* in the
> management UI, so look for it there.

More providers can be added without touching this repository through
[OpenNotifications](https://github.com/notifo-io/open-notifications).

![Integrations](https://github.com/notifo-io/notifo/raw/main/media/Integrations.png "Integrations")

## How it works

* **Users** subscribe to topics that are defined by a path such as `clothes/shoes/nike`. It is your job to provide a good UI for that.
* **Your backend** creates events using very specific topic paths, such as `clothes/shoes/nike/<model>`.
* **Notifo** creates user events based on the matching subscriptions. Subscriptions are either for specific paths or parent paths as in the example above, each with individual notification preferences.
* **Queues** and schedulers are responsible for sending notifications to users when the notification has not been confirmed yet.

This allows a wide range of scenarios:

* In a task management system you can automatically subscribe users to a project, e.g. `project/123`, and use a notification preference to only send web notifications or web push notifications. When a user manually subscribes to a specific task, e.g. `project/123/tasks/abc`, you can create that subscription with a preference to send out emails as well.

* Notifications can have a confirmation preference (None, Explicit, Seen). Only unconfirmed notifications are sent through a channel, and you can configure a delay before sending. This means that a user does not receive a notification when he or she has already explicitly confirmed it (**Explicit** mode) or has seen it (**Seen** mode). This avoids spamming your users with notifications they no longer need, and it lets you track who has seen or confirmed urgent and important notifications.

Have a look at the [presentation](https://github.com/notifo-io/notifo/raw/main/media/notifo!.pdf) for a visual explanation, or read [docs/architecture.md](docs/architecture.md) for the technical one.

## How to run it

The published Docker image is <https://hub.docker.com/r/squidex/notifo>. The
fastest path is the Compose setup in [`deployment/docker-compose`](deployment/docker-compose):

```bash
cd deployment/docker-compose
```

Set `NOTIFO_DOMAIN` in `.env`, then:

```bash
docker compose up -d
```

Open the domain and create the first administrator account.

**Before anyone else uses the instance**, read
[Before you go live](docs/self-hosting.md#before-you-go-live) — the shipped
defaults include a web push key pair that is public in this repository, and
social login credentials that belong to notifo.io.

### How to configure it

Everything in [`appsettings.json`](backend/src/Notifo/appsettings.json) can be set
with an environment variable by joining the path with a double underscore:

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

See [docs/configuration.md](docs/configuration.md) for the settings that matter
and the combinations that are rejected at startup.

## Tech stack

### Server

* ASP.NET Core on .NET 10
* SignalR for web sockets
* MongoDB, or MySQL / PostgreSQL / SQL Server through Entity Framework
* OpenID Connect via OpenIddict
* Optional Redis for clustering

### Frontend

* React, Redux Toolkit and React Router
* TypeScript, built with Vite
* Bootstrap with custom Sass and the [Argon Design](https://www.creative-tim.com/product/argon-design-system) theme
* ...many more libraries.

## Development

```bash
# Backend: solution at backend/Notifo.slnx
cd backend && dotnet test --filter "Category!=Dependencies&Category!=TestContainer"
```

```bash
# Frontend
cd frontend && npm install && npm start
```

API integration tests live in [`tools/TestSuite`](tools/TestSuite) and run against
a Docker image of Notifo, with one compose file per supported database. See
[CLAUDE.md](CLAUDE.md) for the conventions this repository follows.

## Where is it used?

It was originally developed for the Squidex Headless CMS (<https://squidex.io>), and is also used in a few other commercial applications.

## How to contribute?

There is still a lot to do:

* Other email providers.
* Other SMS providers.
* Test application for mobile push (iOS and Android).
* Hardening scheduling and message queues.
* More channels (e.g. voice).
* Testing and tests: automated API and UI tests, and more tests in general.

## Sponsors

Notifo is sponsored and used by the following companies.

[![Squidex](media/logos/squidex.png)](https://squidex.io/) [![Squidex](media/logos/easierlife.png)](https://easierlife.de/)
