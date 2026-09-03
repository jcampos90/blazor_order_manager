# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app
COPY ["src/OrderManager.Web/OrderManager.Web.csproj", "src/OrderManager.Web/"]
RUN dotnet restore "src/OrderManager.Web/OrderManager.Web.csproj"

COPY . .
WORKDIR "/app/src/OrderManager.Web"
RUN dotnet publish "OrderManager.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Minimal Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "OrderManager.Web.dll"]