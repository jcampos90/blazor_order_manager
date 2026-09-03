# Order Manager

Aplicación Blazor Web App (.NET 10) para gestionar encargos de pan y otros productos.
Calcula **cuándo iniciar cada preparación** (`entrega − horas de preparación`) para
entregar a tiempo.

## Requisitos

- .NET SDK 10
- Docker (para Postgres local)

## Puesta en marcha

```bash
# 1. Levantar Postgres (puerto 5432)
docker compose up -d db

# 2. Crear/aplicar migraciones
dotnet ef database update --project src/OrderManager.Web

# 3. Ejecutar
dotnet run --project src/OrderManager.Web
```

La app inicia en `https://localhost:7049` (y `http://localhost:5195`; ver
`Properties/launchSettings.json`) y, en la primera ejecución, siembra productos de ejemplo.

## Autenticación

La app está protegida por **ASP.NET Core Identity** con cookie de autenticación.
Un visitante sin sesión es redirigido automáticamente a `/Identity/Account/Login`.
Tras iniciar sesión vuelve a la app con su identidad disponible en el servidor.
No se usa JavaScript en el cliente.

**Credenciales semilla.** La primera ejecución crea un usuario administrador
con las credenciales:

- **Email / UserName:** `admin@ordermanager.local`
- **Contraseña:** `Admin123!`

Este usuario tiene el rol `Owner` y es el único autorizado a acceder a la app.
Cualquier otro usuario autenticado sin el rol `Owner` es desconectado y ve
una página de acceso denegado (ver `OwnerGateMiddleware`).

**Configuración de Identity.** La configuración de la contraseña (longitud
mínima 6, requiere mayúsculas, minúsculas, dígitos y caracteres especiales)
se define en `Program.cs`. No se requiere configuración externa — solo
Postgres y las migraciones.

**Cerrar sesión.** El encabezado muestra el nombre del usuario con un botón
**Cerrar sesión** que termina la sesión local y limpia la cookie.

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

Blazor Web App (render server interactivo) · EF Core 10 + Npgsql · PostgreSQL 16 ·
ASP.NET Core Identity (cookie) · xUnit.

Las fechas se guardan como `timestamp without time zone` (hora local del panadero); el
tiempo de inicio de preparación se calcula, no se persiste.
