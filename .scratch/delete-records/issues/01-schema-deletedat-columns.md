# 01: Schema — DeletedAt columns on Customer and Product

**What to build:** Add a nullable `DeletedAt` (DateTime) column to `Customer` and `Product` entities, configure it in `AppDbContext`, and generate an EF Core migration that creates the columns. No app-side query changes — those land in tickets 02 / 03 / 06.

**Blocked by:** None

**Status:** ready-for-agent

- [ ] `Customer.DeletedAt` and `Product.DeletedAt` are nullable `DateTime` properties added to the model classes
- [ ] `AppDbContext.OnModelCreating` configures both columns as `timestamp without time zone` (consistent with `Order.DeliveryAt`); not required, no default
- [ ] A new EF Core migration is generated (`dotnet ef migrations add AddDeletedAt`) and applies cleanly against the local Postgres (`dotnet ef database update`)
- [ ] Existing `dotnet test` continues to pass (no query-side changes in this ticket)
- [ ] `dotnet build` and `dotnet test` pass
