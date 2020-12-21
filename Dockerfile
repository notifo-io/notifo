#
# Stage 0, Build SDK
#
FROM mcr.microsoft.com/dotnet/sdk:5.0 as sdk

WORKDIR /src

COPY backend/stylecop.json .
COPY backend/Notifo.ruleset .
COPY backend/sdk sdk/

# Build SDK
RUN dotnet pack sdk/Notifo.SDK/Notifo.SDK.csproj --configuration Release

# Publish to nuget
RUN cd sdk/Notifo.SDK/bin/Release && dotnet nuget push *.nupkg --source 'https://api.nuget.org/v3/index.json' --skip-duplicate -k oy2mrtttnxj6hjax23iqhokcdpcimr6efnchhevcmj7h64

#
# Stage 1, Build Backend
#
FROM mcr.microsoft.com/dotnet/sdk:5.0 as backend

ARG NOTIFO__VERSION=1.0.0

WORKDIR /src

# Copy nuget project files.
COPY backend/**/**/*.csproj /tmp/

# Install nuget packages
RUN bash -c 'pushd /tmp; for p in *.csproj; do dotnet restore $p --verbosity quiet; true; done; popd'

COPY backend .

# Publish
RUN dotnet publish src/Notifo/Notifo.csproj --output /build/ --configuration Release -p:version=$NOTIFO__VERSION

#
# Stage 2, Build Frontend
#
FROM buildkite/puppeteer:latest as frontend

WORKDIR /src

# Copy Node project files.
COPY frontend/package*.json /tmp/

# Install Node packages 
RUN cd /tmp && npm install --loglevel=error

COPY frontend .

# Build Frontend
RUN cp -a /tmp/node_modules . \
 && npm run test:coverage \
 && npm run build

RUN cp -a build /build/


#
# Stage 3, Build runtime
#
FROM mcr.microsoft.com/dotnet/aspnet:5.0.0-buster-slim

RUN apt-get update \
 && apt-get install -y curl
 
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - \
 && apt-get update \
 && apt-get install -y nodejs libc-dev

# Default AspNetCore directory
WORKDIR /app

# Copy from build stages
COPY --from=backend /build/ .
COPY --from=frontend /build/ wwwroot/build/

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Notifo.dll"]
