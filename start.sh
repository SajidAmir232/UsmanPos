#!/bin/sh
set -e

cd POSApp.Web

dotnet restore POSApp.Web.csproj

dotnet publish POSApp.Web.csproj -c Release -o /app/publish --no-restore

cd /app/publish

export ASPNETCORE_URLS="http://+:${PORT:-5000}"

dotnet POSApp.Web.dll
