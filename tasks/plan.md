# Implementation Plan: Order Manager

## Overview

App Blazor Web App (Server) + EF Core 10 + PostgreSQL 16 (Docker Compose) para gestionar
encargos de pan: productos con tiempo de preparación propio, encargos con N renglones,
fecha/hora de entrega, monto total y cliente (nombre+teléfono). Un dashboard responde
"qué debo empezar a preparar y cuándo" calculando `StartBy = DeliveryAt - PrepHours` por renglón.

## Architecture Decisions

- **Blazor Web App interactive server**: simple, sin API separada. Usar
  `IDbContextFactory<AppDbContext>` para evitar el problema de DbContext scope en render
  server interactivo (mejor aún: DbContext por operación).
- **EF Core 10 + Npgsql**: migraciones versionadas; connection string en appsettings.json.
- **Monto total calculado** desde renglones (`qty × unitPrice`), sin descuentos por ahora.
- **`DateTime` local** para `DeliveryAt`; `prepHours` entero en producto; `StartBy` se calcula
  (no se persiste) para evitar desincronización.
- **Cliente normalizado** en tabla `Customers` (Name, Phone); autocomplete en el form.
- **Estado de encargo**: enum `OrderStatus` (Pending, InProduction, Delivered).
- Orden de trabajo: infraestructura (compose + template) → dominio/modelos → CRUD productos →
  CRUD clientes → alta de encargo → dashboard → polish.

## Task List

### Phase 1: Foundation
- [ ] Task 1: Infra local — docker-compose con Postgres 16 + appsettings connection string.
- [ ] Task 2: Scaffold solución — `dotnet new blazor` (OrderManager.Web), proyecto de tests xUnit.
- [ ] Task 3: Entidades + DbContext + migración inicial + seed de productos de ejemplo.

### Checkpoint: Foundation
- [ ] `dotnet build` limpio; `dotnet ef database update` crea la BD; tests pasan.

### Phase 2: Core Features
- [ ] Task 4: CRUD de productos (página UI + validación prepHours>0).
- [ ] Task 5: CRUD de clientes + búsqueda/autocomplete en form de encargo.
- [ ] Task 6: Alta/edición de encargo con renglones y monto calculado (guardar en Postgres).
- [ ] Task 7: Dashboard "Hoy" — renglones a preparar con `StartBy`, alerta si ya pasó el límite.

### Checkpoint: Core Features
- [ ] Flujo completo end-to-end: crear producto → crear encargo → verlo en dashboard.
- [ ] `dotnet test` verde con tests de `PrepSchedule` y agregación del dashboard.

### Phase 3: Polish
- [ ] Task 8: Estado de encargo (marcar entregado), lista de encargos con filtro por fecha.
- [ ] Task 9: Estilos consistentes (Layout + CSS), validación de fechas en el form, README breve.

### Checkpoint: Complete
- [ ] Todos los criterios de éxito del SPEC.md cumplidos.
- [ ] Revisión humana del resultado.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| DbContext scoped vs render server interactivo | Med | Usar `IDbContextFactory` / DbContext por operación |
| Tiempos de preparación cambiantes afectan órdenes históricas | Low | `StartBy` calculado con el prepHours **actual** del producto; no se persiste |
| Postgres no disponible | Med | `docker compose up -d db`; validar healthcheck en Task 1 |

## Open Questions

- Confirmar asunciones 1-5 del SPEC.md (monto calculado, estado, timezone local, clientes, sin auth).
