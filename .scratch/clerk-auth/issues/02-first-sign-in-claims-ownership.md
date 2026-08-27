# 02: First sign-in claims ownership

**What to build:** While no owner is recorded and `Auth:AllowClaim` is enabled, the first successful sign-in is persisted as the owner in a single-row table; thereafter only that Clerk user ID (the `sub` claim) is admitted. Any other signed-in user is signed out and shown a minimal access-denied message.

**Blocked by:** 01 (Gate the app behind Clerk sign-in).

**Status:** ready-for-agent

- [ ] A single-row owner record is persisted via an EF Core migration
- [ ] While `Auth:AllowClaim` is on and no owner exists, the first successful sign-in is recorded as owner (first-write-wins, race-safe)
- [ ] Once an owner exists, only their Clerk user ID is admitted; other signed-in users are signed out and see an access-denied message
- [ ] `Auth:AllowClaim` defaults on for development and can be disabled via config for production
- [ ] Tests cover the claim, lock-down, and rejection paths

## Comments

Implemented. `Models/AppOwner` is a single-row table enforced by `PK_AppOwners` on `Id`
(fixed `Id = 1`) plus a check constraint; `OwnerService.AdmitAsync` claims the first
sign-in while `Auth:AllowClaim` is on (first-write-wins via the PK — a concurrent duplicate
insert throws `DbUpdateException`, is caught, and re-queries the owner). `OwnerGateMiddleware`
runs after authentication: a signed-in non-owner is signed out and 302'd to
`/access-denied` (an `[AllowAnonymous]` page, so no sign-in loop). `Auth:AllowClaim` defaults
true in `OwnerAuthOptions`; the app fails fast at startup if it is disabled with no owner
recorded. Tests in `OwnerServiceTests`/`OwnerAuthOptionsTests` cover claim, lock-down, and
rejection paths.

Known gap (from code review): the race path — the `DbUpdateException` catch on a concurrent
`Id = 1` insert — is not exercised by tests, because the EF InMemory provider does not
enforce the PK/check constraints. Race-safety is structurally guaranteed by the DB primary
key; simulating the losing writer would require a mocked DbContext seam. `Auth:AllowClaim`
defaults on in all environments; disabling for production is a manual config step (no
environment-aware default).

Status: ready-for-agent (implemented) → implemented