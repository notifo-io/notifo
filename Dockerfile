#
# Stage 1, Build Backend
#
FROM mcr.microsoft.com/dotnet/sdk:6.0 as backend

ARG NOTIFO__VERSION=1.0.0

WORKDIR /src

# Copy nuget project files.
COPY backend/*.sln ./

# Copy the main source project files
COPY backend/src/*/*.csproj ./

RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done

# Copy the test project files
COPY backend/tests/*/*.csproj ./

RUN for file in $(ls *.csproj); do mkdir -p tests/${file%.*}/ && mv $file tests/${file%.*}/; done

RUN dotnet restore

COPY backend .
 
# Test Backend
RUN dotnet test --no-restore --filter Category!=Dependencies

# Publish
RUN dotnet publish src/Notifo/Notifo.csproj --output /build/ --configuration Release -p:version=$NOTIFO__VERSION

#
# Stage 2, Build Frontend
#
FROM buildkite/puppeteer:5.2.1 as frontend

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
FROM mcr.microsoft.com/dotnet/aspnet:6.0.0-bullseye-slim

RUN apt-get update \
 && apt-get install -y curl

# Default AspNetCore directory
WORKDIR /app

# Copy from build stages
COPY --from=backend /build/ .
COPY --from=frontend /build/ wwwroot/build/

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Notifo.dll"]
