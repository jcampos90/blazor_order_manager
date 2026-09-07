# Order Manager

Aplicación web para que un panadero registre y administre los encargos de sus clientes y sepa
cuándo iniciar la preparación de cada producto para entregar a tiempo.

## Language

**User**:
Anyone who can sign in to the app (via ASP.NET Core Identity). Identified by email;
carries a `DisplayName` for the UI. Holds exactly one role: `Owner` or `Staff`.
_Avoid_: account, profile, usuario (in code; "Usuarios" is fine as a page title)

**Owner**:
A User with the Owner role. Runs the bakery and manages other Users (create, edit,
reset password, change role, delete). In practice the seed admin is the only Owner.
_Avoid_: admin, superusuario

**Staff**:
A User with the Staff role. Can use every page of the app except user management.
Can change their own password and `DisplayName` from `Mi cuenta`. Cannot edit their
own email or role, and cannot manage other Users.
_Avoid_: empleado, helper

**DisplayName**:
A User's human-readable name (e.g. "María"), separate from the email used to sign
in. Optional at create time; the email is the fallback everywhere it would be
shown. Edited by the User on `Mi cuenta`, by an Owner on `Usuarios`.
_Avoid_: nombre, fullName

**TempPassword**:
A randomly generated initial or reset password for a User, shown to the Owner
exactly once after create / reset and never persisted in readable form. The User
is forced to change it on first sign-in (enforced by `MustChangePassword`).
_Avoid_: contraseña inicial, default password

**LastOwnerGuard**:
Invariant: the system always has at least one User with the Owner role. Enforced
on role change, disable, and hard-delete of any Owner.
_Avoid_: admin lockout (this is the prevention of it)

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
The app holds one bakery's data. Auth gates access; it never scopes data per
user. Multiple Users (one Owner plus zero or more Staff) can sign in, but they
all see and edit the same data — there is no per-user row-level isolation.
_Avoid_: multi-tenant, workspace