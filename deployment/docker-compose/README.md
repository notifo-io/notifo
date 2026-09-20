# Docker Compose

Two compose files:

| File | Use it when |
|---|---|
| `docker-compose.yml` | You want the whole stack: MongoDB, Notifo and a Caddy reverse proxy that obtains a certificate for your domain. |
| `docker-compose-noproxy.yml` | TLS is terminated elsewhere — an external load balancer, or a proxy you already run. |

Both read the domain and the optional social-login credentials from `.env`.

For the full picture — production settings, secrets you must replace, backups
and upgrades — see [docs/self-hosting.md](../../docs/self-hosting.md).

## Run it on a server

1. Point your domain's DNS at the host, so Caddy can complete the ACME challenge
   on ports 80 and 443.
2. Set `NOTIFO_DOMAIN` in `.env`.
3. Start it:

```bash
docker compose up -d
```

Then open your domain and create the first administrator account.

## Run it on localhost

Notifo needs HTTPS, which is a little awkward on localhost. Caddy can issue a
certificate from its own local authority, but it cannot install the root
certificate from inside a container, so you have to do that by hand.

### 1. Configure Caddy

Set `NOTIFO_DOMAIN=localhost` in `.env`, and uncomment this line in
`docker-compose.yml` so Caddy uses its internal certificate authority instead of
Let's Encrypt (which cannot validate `localhost`):

```yml
# - SITE_SETTINGS="tls internal"
```

Start the stack:

```bash
docker compose up -d
```

### 2. Install the root certificate

Copy the root certificate out of the proxy container:

```bash
docker cp docker-compose-notifo_proxy-1:/data/caddy/pki/authorities/local/root.crt .
```

Add `root.crt` to your machine's **trusted root certificate authorities**, then
restart your browser and open <https://localhost>.

> The container name depends on the directory the compose file runs from. Use
> `docker ps` to find the real name if the command above does not match.

## Where state lives

| Host path | Holds |
|---|---|
| `/etc/notifo/mongo/db` | The database. |
| `/etc/notifo/assets` | Uploaded media. |
| `/etc/notifo/caddy` | Certificates and proxy state. |

Back up the first two. Nothing else in the containers needs to survive a restart.
