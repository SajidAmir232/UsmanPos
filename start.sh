#!/bin/sh
set -e

cd POSApp.Web

dotnet restore POSApp.Web.csproj

dotnet publish POSApp.Web.csproj -c Release -o /app/publish --no-restore

cd /app/publish

dotnet POSApp.Web.dll
