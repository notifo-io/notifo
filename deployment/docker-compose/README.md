# Docker Compose

## Run it on localhost

Notifo needs https. This is a little bit tricky on localhost.

### 1. Configure Caddy

Caddy is used as a reverse proxy and also creates the certificates. Usually lets-encrypt is used for that, but for localhost we need to switch to a local certificate authority.

Therefore you have to comment out the following line in `docker compose.yml`:

```yml
# - SITE_SETTINGS="tls internal"
```

### Step 2 Download the root certificate

Caddy usually installs the root certificate, but this is not available on docker. Therefore we have to install the root certificate manually. First download the certificate from the docker container:

docker cp docker compose-notifo_proxy-1:/data/caddy/pki/authorities/local/root.crt .

Next install the certificate to the **trusted root authorities store**. You might need to restart chrome after that.