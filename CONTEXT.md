# Order Manager

Aplicación web para que un panadero registre y administre los encargos de sus clientes y sepa
cuándo iniciar la preparación de cada producto para entregar a tiempo.

## Language

**User**:
The baker operating the app, signed in via ASP.NET Core Identity. Owns the app's data as a single tenant.
_Avoid_: account, profile

**Customer**:
The bakery's client who places orders; has a name and phone but no sign-in.
_Avoid_: account, client (UI: "cliente")

**Order**:
A client's order (UI: "encargo") with a delivery date/time, status, optional note, and one or
more OrderLines. Its total is computed from its lines.
_Avoid_: request, sale

**OrderLine**:
A single line of an Order: a Product, a quantity, and a unit price (UI: "renglón").
_Avoid_: item

**Product**:
Something the bakery makes, with a name, a price, and its own prepHours (preparation time in
hours).
_Avoid_: good, sku

**OrderStatus**:
The lifecycle state of an Order: Pending, InProduction, Delivered.
_Avoid_: state, phase

**PrepSchedule**:
The computed start-by time for a line, `DeliveryAt − prepHours` of the product. Never persisted.
_Avoid_: production plan

**Single tenant**:
The app holds one baker's data. Auth gates access; it never scopes data per user.
_Avoid_: multi-tenant, workspace