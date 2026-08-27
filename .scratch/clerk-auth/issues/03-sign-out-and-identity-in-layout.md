# 03: Sign-out + identity in the layout

**What to build:** The app's header shows the signed-in baker's name or email, plus a **Sign out** button that ends the Clerk session (full end-session) and clears the local auth cookie, returning the visitor to a signed-out state where pages redirect to sign-in.

**Blocked by:** 01 (Gate the app behind Clerk sign-in).

**Status:** ready-for-agent

- [ ] The signed-in user's name/email appears in the layout header
- [ ] A **Sign out** button ends the Clerk session and clears the local cookie
- [ ] After sign-out, app pages redirect to sign-in