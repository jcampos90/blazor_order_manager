# 02: First sign-in claims ownership

**What to build:** While no owner is recorded and `Auth:AllowClaim` is enabled, the first successful sign-in is persisted as the owner in a single-row table; thereafter only that Clerk user ID (the `sub` claim) is admitted. Any other signed-in user is signed out and shown a minimal access-denied message.

**Blocked by:** 01 (Gate the app behind Clerk sign-in).

**Status:** ready-for-agent

- [ ] A single-row owner record is persisted via an EF Core migration
- [ ] While `Auth:AllowClaim` is on and no owner exists, the first successful sign-in is recorded as owner (first-write-wins, race-safe)
- [ ] Once an owner exists, only their Clerk user ID is admitted; other signed-in users are signed out and see an access-denied message
- [ ] `Auth:AllowClaim` defaults on for development and can be disabled via config for production
- [ ] Tests cover the claim, lock-down, and rejection paths