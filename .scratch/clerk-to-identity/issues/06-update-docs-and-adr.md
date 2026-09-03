# 06: Update docs and ADR

**What to build:** Update `CONTEXT.md`, `README.md`, and create a new ADR that supersedes ADR-0001. Remove all Clerk references from documentation.

**Blocked by:** 02

**Status:** done

- [x] Update `CONTEXT.md`: change "signed in via Clerk" to "signed in via ASP.NET Core Identity"
- [x] Update `README.md`: remove Clerk user-secrets setup, document seed credentials (`admin@ordermanager.local` / `Admin123!`), document Identity configuration
- [x] Create `docs/adr/0002-aspnet-core-identity-local-auth.md` superseding ADR-0001, recording the decision to use ASP.NET Core Identity with role-based owner gate
- [x] Verify no remaining Clerk references in documentation
- [x] `dotnet build` passes
- [x] `dotnet test` passes
