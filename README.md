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

La app inicia en `https://localhost:7002` (ver `Properties/launchSettings.json`) y, en la
primera ejecución, siembra productos de ejemplo.

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

Blazor Web App (render server interactivo) · EF Core 10 + Npgsql · PostgreSQL 16 · xUnit.

Las fechas se guardan como `timestamp without time zone` (hora local del panadero); el
tiempo de inicio de preparación se calcula, no se persiste.
