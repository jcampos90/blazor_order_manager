# 05: Simplify sign-out

**What to build:** Simplify `SignOutHandler` to clear only the Identity cookie. No OIDC end-session redirect. Update tests to verify the simplified behavior.

**Blocked by:** 02

**Status:** done

- [x] Update `SignOutHandler` to sign out of `IdentityConstants.ApplicationScheme` only (single sign-out)
- [x] Update `SignOutHandlerTests` to verify single cookie sign-out instead of dual sign-out
- [x] Remove test assertions for OIDC scheme sign-out
- [x] `dotnet build` passes
- [x] `dotnet test` passes
