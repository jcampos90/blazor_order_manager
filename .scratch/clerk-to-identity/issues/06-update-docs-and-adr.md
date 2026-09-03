# 06: Update docs and ADR

**What to build:** Update `CONTEXT.md`, `README.md`, and create a new ADR that supersedes ADR-0001. Remove all Clerk references from documentation.

**Blocked by:** 02

**Status:** ready-for-agent

- [ ] Update `CONTEXT.md`: change "signed in via Clerk" to "signed in via ASP.NET Core Identity"
- [ ] Update `README.md`: remove Clerk user-secrets setup, document seed credentials (`admin@ordermanager.local` / `Admin123!`), document Identity configuration
- [ ] Create `docs/adr/0002-aspnet-core-identity-local-auth.md` superseding ADR-0001, recording the decision to use ASP.NET Core Identity with role-based owner gate
- [ ] Verify no remaining Clerk references in documentation
- [ ] `dotnet build` passes
- [ ] `dotnet test` passes
