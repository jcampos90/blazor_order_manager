# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app
COPY ["src/OrderManager.Web/OrderManager.Web.csproj", "src/OrderManager.Web/"]
RUN dotnet restore "src/OrderManager.Web/OrderManager.Web.csproj"

COPY . .
WORKDIR "/app/src/OrderManager.Web"
RUN dotnet publish "OrderManager.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Migration bundles: one per module context ---
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
ENV ASPNETCORE_ENVIRONMENT=Production
RUN dotnet ef migrations bundle \
      --context AppDbContext \
      --project OrderManager.Web.csproj \
      --startup-project OrderManager.Web.csproj \
      --self-contained -r linux-x64 -o /app/publish/migrate-app

# Stage 2: Minimal Runtime
# NOTE: must NOT be the `-alpine` variant. The migration bundle above is built
# with `--self-contained -r linux-x64` on the Debian-based sdk image, so it
# links against glibc and embeds /lib64/ld-linux-x86-64.so.2 as its ELF
# interpreter. Alpine ships musl libc, so `./migrate-app` would fail at exec
# with "not found" even though the file is present. Keep this Debian/glibc.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh migrate-app
#USER app
ENTRYPOINT ["./entrypoint.sh"]