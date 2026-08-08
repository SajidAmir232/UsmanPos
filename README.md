# UsmanPos Startup Guide

This repository contains the UsmanPos ASP.NET Core Razor POS web application.

## Website Run Commands

The website can be started directly from the repository root with the following command:

```bash
cd /workspaces/UsmanPos
export POSAPP_DB_PATH=/workspaces/UsmanPos/POSApp.Web/data/pos_local.db
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://localhost:5000
mkdir -p /workspaces/UsmanPos/POSApp.Web/data
dotnet run --project POSApp.Web/POSApp.Web.csproj --no-launch-profile
```

After that, the app is available at:

http://localhost:5000/

## Existing Startup Script

The repository also contains a startup script at [start.sh](start.sh):

```bash
cd /workspaces/UsmanPos
./start.sh
```

This script restores the app, publishes it to `/app/publish`, and then runs `dotnet POSApp.Web.dll`.

In this container, the publish command fails with permission errors because `/app` is not writable by the current user, so the direct `dotnet run` command above is the working startup path in this environment.

## Optional Docker Run

The app can also be started with Docker Compose from the project root:

```bash
docker compose up --build
```

The compose file maps the web app to port `5000`.
