# Self-hosting Notifo

Everything you need to run your own Notifo instance. If you only want to try the
product, use the hosted version at <https://app.notifo.io> first — hosting adds a
database, TLS and a handful of secrets to look after.

- [What you need](#what-you-need)
- [Quick start on your machine](#quick-start-on-your-machine)
- [A production deployment](#a-production-deployment)
- [First run](#first-run)
- [Before you go live](#before-you-go-live)
- [Connecting your application](#connecting-your-application)
- [Operating it](#operating-it)
- [Upgrading](#upgrading)
- [Troubleshooting](#troubleshooting)

## What you need

| | |
|---|---|
| **Notifo** | The `squidex/notifo` Docker image. It listens on port 80 inside the container and contains the API, the UI and the background workers. |
| **A database** | MongoDB, PostgreSQL, MySQL or SQL Server. Nothing else is required — the job queues live in the database by default. The bundled Compose file uses `mongo:5`; CI additionally runs the API suite against `postgres:16`, MySQL and SQL Server. |
| **A domain with TLS** | Effectively not optional. Web push needs a service worker, and browsers only run service workers in a secure context, so the embeddable SDK and web push do not work over plain HTTP. |
| **Somewhere for uploaded media** | A volume, or S3 / Azure Blob / Google Cloud Storage. |
| **Redis** | Only if you run more than one instance. See [Scaling out](#scaling-out). |

Sizing depends entirely on your notification volume, so measure rather than
guess. Two things are worth knowing up front: the work is I/O-bound, so the
database is usually what needs attention first, and `/healthz` reports unhealthy
once the process passes `diagnostics:gc:threshold`, which defaults to 8 GB —
lower it if your container limit is smaller, or the probe will never fire.

## Quick start on your machine

This gets you a working instance on `https://localhost` with a local certificate.

```bash
cd deployment/docker-compose
```

Edit `.env` and set the domain:

```
NOTIFO_DOMAIN=localhost
```

In `docker-compose.yml`, uncomment the line that tells Caddy to issue its own
certificate instead of asking Let's Encrypt (which cannot validate `localhost`):

```yml
- SITE_SETTINGS="tls internal"
```

Start it:

```bash
docker compose up -d
```

Caddy generates a local certificate authority, but it cannot install the root
certificate for you from inside a container. Export it:

```bash
docker cp docker-compose-notifo_proxy-1:/data/caddy/pki/authorities/local/root.crt .
```

The container name is derived from the directory the compose file runs in, so
check `docker ps` if that name does not match.

Then add `root.crt` to your machine's **trusted root certificate authorities**
and restart your browser. Open <https://localhost> and continue at
[First run](#first-run).

> `docker-compose-noproxy.yml` leaves out Caddy and publishes Notifo directly.
> Use it when TLS is terminated somewhere else — an external load balancer, or a
> proxy you already run. It still expects to be reached over HTTPS, so
> `URLS__BASEURL` stays an `https://` URL.

## A production deployment

The compose file in `deployment/docker-compose` is a reasonable starting point:
MongoDB, Notifo and a Caddy reverse proxy that obtains Let's Encrypt certificates
for the domain in `.env`.

```bash
cp -r deployment/docker-compose /opt/notifo
cd /opt/notifo
$EDITOR .env          # set NOTIFO_DOMAIN to your real domain
docker compose up -d
```

Point the domain's DNS at the host before starting, so Caddy can complete the
ACME challenge on ports 80 and 443.

### What that compose file does, and what it leaves to you

It mounts three host paths so nothing important lives inside a container:

| Host path | Holds |
|---|---|
| `/etc/notifo/mongo/db` | The database. |
| `/etc/notifo/assets` | Uploaded media. |
| `/etc/notifo/caddy` | Certificates and proxy state. |

It does **not** set the secrets described under
[Before you go live](#before-you-go-live). Read that section before you let
anyone else use the instance.

### Using PostgreSQL instead of MongoDB

Swap the storage and messaging settings. The `Scheduler` transport is
MongoDB-only, so SQL storage must also switch the transport — Notifo refuses to
start otherwise, with an error naming the setting.

```yml
environment:
  - STORAGE__TYPE=Sql
  - STORAGE__SQL__PROVIDER=Postgres
  - STORAGE__SQL__CONNECTIONSTRING=Server=db;Port=5432;Database=notifo;Username=notifo;Password=...
  - MESSAGING__TYPE=Sql
```

`Provider` is `Postgres`, `MySql` or `SqlServer`. Migrations run automatically on
startup; set `STORAGE__SQL__RUNMIGRATION=false` if you would rather apply them
yourself.

MySQL needs two extra things for bulk inserts to work: start the server with
`--local-infile=1` and add `AllowLoadLocalInfile=true` to the connection string.

### Behind your own reverse proxy

If you already run nginx, Traefik or an ingress controller, drop the Caddy
service and proxy to the Notifo container's port 80. Two rules matter:

- **`URLS__BASEURL` must be the exact public URL**, including the scheme. It is
  used for the OpenID Connect issuer, tracking URLs, media URLs and email links.
  If it is wrong, logins fail and links in emails point at the wrong host.
- **Forward the standard headers.** Notifo honours `X-Forwarded-Proto`,
  `X-Forwarded-For` and `X-Forwarded-Host`. Without `X-Forwarded-Proto: https`
  it will believe it is on HTTP and generate broken redirects.

WebSockets must be allowed through for the `/hub` endpoint, or the web channel
silently degrades to polling.

### Kubernetes

There is no official chart. The container is an ordinary stateless web app, so a
`Deployment` plus a `Service` and an `Ingress` is enough:

- put every setting from [configuration.md](configuration.md) into a `ConfigMap`,
  and the secrets into a `Secret`,
- set the readiness and liveness probes to `GET /healthz`,
- use an external asset store (S3, Blob, GCS) rather than a volume, so pods stay
  interchangeable,
- if `replicas > 1`, set `CLUSTERING__TYPE=Redis` — see [Scaling out](#scaling-out).

## First run

Open the site. Because no user exists yet, Notifo redirects you to
`/account/setup`, where you create the first administrator account. That account
gets the host-admin role and can create apps and invite others.

You can create users from configuration instead, which is handy for automated
deployments. They are created at startup if they do not exist:

```yml
- IDENTITY__USERS__0__EMAIL=admin@example.com
- IDENTITY__USERS__0__PASSWORD=...
- IDENTITY__USERS__0__ROLE=ADMIN
```

The very first user in the installation always gets the host-admin role, whether
it comes from the wizard or from configuration. For any user after that, set
`ROLE` explicitly. Adding `PASSWORDRESET=true` makes Notifo reset the password of
an existing account on every start, which is a way back in if you lock yourself
out — remove it again afterwards.

Then:

1. **Create an app.** Everything else belongs to an app: users, topics,
   templates, integrations.
2. **Add an integration.** Nothing can be delivered until at least one is
   configured — start with SMTP under *Integrations*.
3. **Grab an API key.** Each app has API keys per role, shown on the app page.
   Your backend uses one of these in the `X-ApiKey` header.

## Before you go live

These defaults ship in `appsettings.json` so that a fresh checkout runs. Every
one of them is public, and several are actively unsafe to keep.

### Replace the web push keys — required

`webPush:vapidPublicKey` and `webPush:vapidPrivateKey` have default values that
are committed to this repository. Anyone can read the private key. Generate your
own pair:

```bash
npx web-push generate-vapid-keys
```

```yml
- WEBPUSH__VAPIDPUBLICKEY=...
- WEBPUSH__VAPIDPRIVATEKEY=...
- WEBPUSH__SUBJECT=https://your-domain.example
```

Change these **before** any browser subscribes. Existing web push subscriptions
are bound to the public key and stop working when it changes.

### Replace or remove the social login credentials — required

`identity:githubClient`, `identity:googleClient` and their secrets default to
notifo.io's own OAuth applications. They will not work for your domain, because
the callback URL will not match. Register your own applications and set the
credentials, or leave the values empty to hide those buttons:

```yml
- IDENTITY__GITHUBCLIENT=
- IDENTITY__GITHUBSECRET=
- IDENTITY__GOOGLECLIENT=
- IDENTITY__GOOGLESECRET=
```

The bundled compose file already wires these to `.env` variables, which are empty
by default — so if you use it unchanged, you are fine.

### Decide how people log in

| Setting | Effect |
|---|---|
| `identity:allowPasswordAuth` | Set to `false` to disable local accounts and force SSO. Do this only once SSO works, or you will lock yourself out. |
| `identity:oidc*` | Point at your own identity provider. `oidcAuthority`, `oidcClient` and `oidcSecret` are the minimum. |
| `identity:adminClientId` / `adminClientSecret` | Creates a machine client with full admin rights for automation. Leave both empty if you do not need it — anyone holding the secret owns the installation. |

### Lock down outbound requests

Webhooks and remote media make Notifo fetch URLs that users supply, so it ships
with SSRF protection on: redirects are disabled, link-local metadata endpoints
are blocked, and DNS is validated twice. Keep `ssrf:whiteListedHosts` empty in
production. The test compose files set it to `*`, which is fine for a throwaway
container and wrong for a real one.

### Checklist

- [ ] `URLS__BASEURL` is the exact public HTTPS URL
- [ ] Own VAPID key pair
- [ ] Own (or empty) GitHub / Google / OIDC credentials
- [ ] `identity:adminClientSecret` empty, or a strong generated value
- [ ] Database not reachable from outside the Docker network
- [ ] `ssrf:whiteListedHosts` empty
- [ ] Backups configured for the database and the asset store

## Connecting your application

### Publish events from your backend

```bash
curl -X POST https://your-domain.example/api/apps/{appId}/events \
  -H "X-ApiKey: <app api key>" \
  -H "Content-Type: application/json" \
  -d '{
        "requests": [{
          "topic": "users/user-42",
          "preformatted": { "subject": { "en": "Hello" } }
        }]
      }'
```

There are official clients for [.NET](../tools/sdk-dotnet) and
[TypeScript](../tools/sdk-ts), both generated from the OpenAPI document at
`/api/openapi.json`. The full API reference is at `/api/docs`.

### Add the notification widget to your website

```html
<script src="https://your-domain.example/build/notifo-sdk.js"></script>
<script>
  var notifo = window.notifo || (window.notifo = []);
  notifo.push(['init', {
    apiUrl: 'https://your-domain.example',
    userToken: '<user api key>'
  }]);
  notifo.push(['show-notifications', document.getElementById('bell')]);
</script>
```

The user API key comes from the user object your backend creates through the
API — it authenticates one end user and must not be confused with the app key.
If you would rather let the widget create the user itself, pass the app key as
`apiKey` together with `userEmail` and `userName` instead of `userToken`.

For web push the service worker must be served **from your own origin**, because
a service worker can only control the origin it is served from. Download
`notifo-sdk-worker.js` from your Notifo instance, host it at `/notifo-sw.js` on
your site (that is the path the SDK expects), or point `serviceWorkerUrl` at
wherever you put it:

```js
notifo.push(['init', {
  apiUrl: 'https://your-domain.example',
  userToken: '<user api key>',
  serviceWorkerUrl: '/static/notifo-sw.js'
}]);
```

## Operating it

### Health and observability

- `GET /healthz` returns a JSON report with one entry per check. Today the only
  check is process memory against `diagnostics:gc:threshold`, so it tells you the
  process is alive and not thrashing — it does **not** probe the database. Use it
  for liveness and readiness probes, and monitor the database separately.
- Logs are structured JSON on stdout. Set `logging:human=true` for readable
  output while debugging.
- OpenTelemetry traces: `logging:otlp:enabled=true` plus an `endpoint`.
  Application Insights and Stackdriver have their own sections.

### Backups

Two things hold state:

1. **The database** — everything except media. Use your database's normal backup
   tooling.
2. **The asset store** — uploaded media. With `assetStore:type=Folder` this is the
   mounted directory; with S3, Blob or GCS it is the bucket.

Nothing else in the container needs to survive a restart.

### Scaling out

The default configuration assumes one instance. Before adding replicas, set:

```yml
- CLUSTERING__TYPE=Redis
- CLUSTERING__REDIS__CONNECTIONSTRING=redis:6379
```

This gives SignalR a backplane and lets instances invalidate each other's caches.
Also move the asset store off local disk so every replica sees the same media.
[architecture.md](architecture.md#running-more-than-one-instance) explains what
breaks without it.

## Upgrading

The image is tagged by major version (`squidex/notifo:1`) and by exact version.
Pinning the exact version and upgrading deliberately is the safer habit.

```bash
docker compose pull
docker compose up -d
```

Database migrations run automatically at startup — MongoDB migrations always, EF
migrations unless you set `storage:sql:runMigration=false`. **Back up first**, and
read [CHANGELOG.md](../CHANGELOG.md) before a major bump.

Rolling back after a migration has run is not supported, so on a significant
upgrade the safe sequence is: snapshot the database, upgrade, verify, and restore
the snapshot if you need to go back.

## Troubleshooting

**Notifo will not start and logs a configuration error.** Configuration errors
name the exact setting. The common ones are `messaging:type=Scheduler` with SQL
storage (use `Sql`), and `assetStore:type=MongoDb` with SQL storage (use `Folder`
or a cloud bucket).

**Login redirects in a loop, or fails with a redirect URI error.** `urls:baseUrl`
does not match the URL in the browser, or the proxy is not sending
`X-Forwarded-Proto: https`.

**Events return 200 but nothing arrives.** Delivery is asynchronous. Check *Log*
in the management UI first — a skipped channel is recorded there with a reason.
Usual causes: no integration configured for the channel, the channel not enabled
in the user's or subscription's settings, or the user having no email address,
phone number or device token.

**Web notifications reach some users but not others.** More than one instance
without `clustering:type=Redis`.

**Web push never arrives.** The site must be HTTPS, `notifo-sdk-worker.js` must
be served from the root of your own origin, and the VAPID keys must not have
changed since the browser subscribed.

**Images are not resized.** Resizing falls back to serving the original on
failure, without an error to the client. Check `assets:resizerUrl` — if it is set,
it must be reachable from the container, using the *internal* port of the resizer
service rather than a published host port.

**MySQL errors about `LOAD DATA LOCAL INFILE`.** Start the server with
`--local-infile=1` and add `AllowLoadLocalInfile=true` to the connection string.

---

Every setting mentioned here, and many more, are documented in
[configuration.md](configuration.md). The internals are in
[architecture.md](architecture.md).
