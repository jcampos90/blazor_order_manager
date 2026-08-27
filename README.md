# Order Manager

Aplicación Blazor Web App (.NET 10) para gestionar encargos de pan y otros productos.
Calcula **cuándo iniciar cada preparación** (`entrega − horas de preparación`) para
entregar a tiempo.

## Requisitos

- .NET SDK 10
- Docker (para Postgres local)
- Cuenta de Clerk con una instancia y un OAuth application (ver Autenticación)

## Puesta en marcha

```bash
# 1. Levantar Postgres (puerto 5432)
docker compose up -d db

# 2. Crear/aplicar migraciones
dotnet ef database update --project src/OrderManager.Web

# 3. Configurar las claves de Clerk (ver Autenticación)
dotnet user-secrets init --project src/OrderManager.Web
dotnet user-secrets set "Auth:Oidc:Authority" "..." --project src/OrderManager.Web
dotnet user-secrets set "Auth:Oidc:ClientId" "..." --project src/OrderManager.Web
dotnet user-secrets set "Auth:Oidc:ClientSecret" "..." --project src/OrderManager.Web

# 4. Ejecutar
dotnet run --project src/OrderManager.Web
```

La app inicia en `https://localhost:7049` (y `http://localhost:5195`; ver
`Properties/launchSettings.json`) y, en la primera ejecución, siembra productos de ejemplo.

## Autenticación

La app está protegida por **Clerk** (OpenID Connect): un visitante sin sesión es redirigido
automáticamente al sign-in alojado en Clerk; tras iniciar sesión vuelve a la app con su
identidad disponible en el servidor. No se usa JavaScript en el cliente.

**Claim del primer inicio de sesión.** El primer usuario que inicia sesión con
`Auth:AllowClaim` habilitado (por defecto en todas las configuraciones; deshabilitarlo en
producción es un paso manual) queda registrado como propietario de la app (tabla de una sola
fila `AppOwners`). A partir de entonces solo ese usuario de Clerk (su claim `sub`) es
admitido; cualquier otro usuario autenticado es desconectado y ve una página de acceso
denegado. Para producción, deshabilita el claim (`Auth:AllowClaim=false`) una vez que el
propietario esté registrado; la app falla al arrancar si está deshabilitado y no hay
propietario.

**Cerrar sesión.** El encabezado muestra el nombre (o correo) del usuario con un botón
**Cerrar sesión** que termina la sesión de Clerk (end-session remoto) y limpia la cookie local.

**Configurar las claves de desarrollo.** Con las claves del OAuth application de Clerk
(desde `src/OrderManager.Web`):

```bash
dotnet user-secrets set "Auth:Oidc:Authority"      "https://<instancia>.clerk.accounts.dev"
dotnet user-secrets set "Auth:Oidc:ClientId"       "<oauth client id>"
dotnet user-secrets set "Auth:Oidc:ClientSecret"   "<oauth client secret>"
dotnet user-secrets set "Auth:Oidc:PublishableKey" "<pk_test_...>"   # informativa, no se usa
```

La app falla al arrancar (fast-fail) si faltan `Authority`, `ClientId` o `ClientSecret`.
Además, el OAuth application de Clerk debe registrar las URIs de redirección de los perfiles
de lanzamiento — sign-in y post-logout:

- `https://localhost:7049/signin-oidc` y `http://localhost:5195/signin-oidc`
- `https://localhost:7049/signout-callback-oidc` y `http://localhost:5195/signout-callback-oidc`

## Funcionalidades

- **Hoy** (`/`): dashboard de producción — cada renglón pendiente con su hora límite de
  inicio, marcando en rojo los atrasados.
- **Encargos** (`/encargos`): lista con filtros (hoy, mañana, fecha), alta/edición con
  renglones y monto calculado, y estado (`Pendiente / En producción / Entregado`).
- **Productos** (`/productos`): nombre, precio y horas de preparación.
- **Clientes** (`/clientes`): nombre y teléfono, con búsqueda al cargar un encargo.

## Comandos

```
Build:  dotnet build
Test:   dotnet test
DB up:  docker compose up -d db
```

## Stack

Blazor Web App (render server interactivo) · EF Core 10 + Npgsql · PostgreSQL 16 · Clerk
(OIDC) · xUnit.

Las fechas se guardan como `timestamp without time zone` (hora local del panadero); el
tiempo de inicio de preparación se calcula, no se persiste.
