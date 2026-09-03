#!/bin/sh
set -e


echo "listing files..."
ls -la

echo "Applying migrations..."
./migrate-app --connection "$ConnectionStrings__DefaultConnection"


echo "Migrations up to date. Starting API..."
exec dotnet OrderManager.Web.dll