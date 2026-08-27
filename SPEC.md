# Spec: Order Manager — Gestión de encargos de pan

## Objective

Aplicación web para que un panadero registre y administre los **encargos** (órdenes) de
clientes. Cada encargo tiene productos (renglones), fecha/hora de entrega, monto total y
datos del cliente (nombre y teléfono). El valor central del sistema es responder:
**"¿cuándo debo iniciar la preparación de cada pan para entregarlo a tiempo?"**

Regla de negocio: cada producto tiene su propio **tiempo de preparación** (ej. 24h). Para un
encargo con entrega el `2026-08-15 08:00`, un producto con 24h de preparación debe iniciarse
a más tardar el `2026-08-14 08:00`.

Éxito: el panadero abre la app y ve qué producciones deben arrancar hoy/ahora, agrupadas por
hora límite de inicio, sin hacer cuentas mentales.

## Tech Stack

- .NET 10 (SDK 10.0.110 instalado)
- Blazor Web App — interactive **Server** render mode
- EF Core 10 + Npgsql (PostgreSQL)
- PostgreSQL v16 vía Docker Compose (desarrollo local)
- Sin autenticación (uso personal/single-user por ahora)

## Commands

```
Build:   dotnet build
Run:     dotnet run --project src/OrderManager.Web  (usa la connection string local)
DB up:   docker compose up -d db
DB down: docker compose down
Migrate: dotnet ef database update --project src/OrderManager.Web
```

## Project Structure

```
order-manager/
├── docker-compose.yml            → Postgres local
├── SPEC.md                       → este documento
├── tasks/plan.md, tasks/todo.md  → plan y tareas
└── src/
    └── OrderManager.Web/
        ├── Program.cs
        ├── appsettings.json       → connection string
        ├── Components/            → Blazor components
        │   ├── App.razor
        │   ├── Layout/
        │   ├── Pages/
        │   │   ├── Home.razor          → Dashboard "Qué preparar hoy"
        │   │   ├── Orders.razor        → Lista de encargos
        │   │   ├── OrderEdit.razor     → Crear/editar encargo
        │   │   ├── Products.razor      → CRUD de productos
        │   │   └── Customers.razor     → CRUD de clientes
        │   └── Shared/
        ├── Data/                   → EF Core DbContext, migraciones
        ├── Models/                 → entidades
        └── Services/               → lógica de negocio (cálculo de inicio de preparación)
```

## Code Style

- Nombres en inglés en código (entidades, servicios); UI en español.
- `async` Task-based handlers; EF Core vía `IDbContextFactory<T>` (scoped DB no compatible con
  render server interactivo).
- Decimal para dinero; `DateTime` con **local time** (zona del panadero) para `DeliveryAt`.
- Migraciones por `dotnet ef` (entidad `OrderManager.Web`).

Ejemplo de estilo:

```csharp
public sealed record PrepScheduleLine(int Quantity, string Product, DateTime StartBy);

public static class PrepSchedule
{
    public static DateTime StartBy(DateTime deliveryAt, int prepHours) =>
        deliveryAt.AddHours(-prepHours);
}
```

## Testing Strategy

- xUnit + EF Core **InMemory** (o Testcontainers si resulta accesible) para la lógica de
  `PrepSchedule` y agregación del dashboard.
- Pruebas en `tests/OrderManager.Web.Tests/`.
- Ejecutar: `dotnet test`.

## Boundaries

- **Always:** validar `prepHours > 0` al guardar producto; validar `DeliveryAt` en el futuro;
  correr `dotnet build` + `dotnet test` antes de dar algo por terminado.
- **Ask first:** cambios de esquema en producción, añadir dependencias, auth/multiusuario,
  cambio del modelo de pago (señas/crédito).
- **Never:** commits de secrets; borrar migraciones sin revisar; editar el directorio de
  salida de build.

## Success Criteria

- [ ] `docker compose up -d db` levanta Postgres y `dotnet run` arranca la app.
- [ ] CRUD de productos: nombre, precio, horas de preparación, activo.
- [ ] Alta de encargo con cliente (nombre+teléfono), fecha/hora de entrega, N renglones
      (producto, cantidad, precio unitario) y monto total calculado.
- [ ] Dashboard "Hoy" muestra cada renglón a preparar con su hora límite de inicio
      (`DeliveryAt - PrepHours`), resaltando los que ya pasaron la hora límite.
- [ ] `dotnet test` pasa para la lógica de preparación.
- [ ] Persistencia en Postgres (no en memoria).

## Open Questions / Asunciones

1. **Monto cobrado = total calculado** de los renglones (precio unitario del producto ×
   cantidad). No hay descuentos/manuales por ahora. → corregir si no.
2. **Estado de encargo**: enum simple `Pendiente / En producción / Entregado` para marcarlo
   entregado al repartir. → quitar si sobra.
3. **Zona horaria local** del panadero; sin manejo de timezones.
4. **Clientes**: tabla con nombre+teléfono; el form de encargo permite buscar o crear rápido.
5. La app es single-user local (sin login). El `Monto cobrado` y la fecha/hora de entrega se
   capturan en el encargo; la hora de entrega es día **y** hora (ej. 08:00).
