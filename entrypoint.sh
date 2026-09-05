#!/bin/sh
set -e


echo "listing files..."
ls -la

echo "Applying app migrations..."
./migrate-app --connection "$ConnectionStrings__DefaultConnection"

echo "Applying identity migrations..."
./migrate-identity --connection "$ConnectionStrings__DefaultConnection"


echo "Migrations up to date. Starting API..."
exec dotnet OrderManager.Web.dll