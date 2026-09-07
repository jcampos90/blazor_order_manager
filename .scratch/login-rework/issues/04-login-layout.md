# 04: LoginLayout — full-page login & access-denied

**What to build:** New `Components/Layout/LoginLayout.razor`. Switches `Login.razor` and `AccessDenied.razor` to use it. No sidebar, no topbar, just brand mark + body.

**Blocked by:** None

**Status:** pending

- [ ] `Components/Layout/LoginLayout.razor` exists, renders a single centered card with the existing brand mark SVG and `@Body`. Background uses a soft gradient (or just `var(--color-bg)`). `<ReconnectModal />` is included so the WebSocket reconnect still works.
- [ ] `Components/Pages/Login.razor` `@layout` directive is changed to `LoginLayout`. The form keeps its antiforgery token, error handling, and return-URL flow. Visual polish: clearer error band ("Email o contraseña incorrectos." / "Ingresá email y contraseña." / "Tu sesión expiró, volvé a iniciar sesión."), branded submit button.
- [ ] `Components/Pages/AccessDenied.razor` switches to `LoginLayout`. Copy: "No tenés permisos para entrar a esta página." with a `Volver al inicio` link to `/`.
- [ ] `dotnet build` passes; smoke test: open `/login` and confirm the sidebar is not in the DOM.
