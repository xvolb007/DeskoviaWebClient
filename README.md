# Deskovia Web Client

Functional Blazor Web App with Interactive WebAssembly. Authenticated pages run without server prerender and call the API directly with credentials and antiforgery protection. The interface is in English.

## Running

After applying the backend migration, start both applications normally. The backend automatically prepares active ABC; registration needs no separate setup command or payment. The first registered account automatically receives Owner; all later accounts receive Employee. On upgrade, an existing ABC without Owner assigns the earliest active local member at startup. See the [backend instructions](../DeskoviaEngine/docs/PrototypeRunbook.md). The API must run at https://localhost:7095.

~~~powershell
dotnet build
dotnet run --project DeskoviaWebClient --launch-profile https
~~~

Open https://localhost:7178. Public API configuration is ApiBaseUrl in DeskoviaWebClient.Client/wwwroot/appsettings.json; the exact frontend origin must match the backend's Prototype:FrontendOrigin.

Shared is referenced from the adjacent DeskoviaEngine repository. Keep both repositories in this layout or update the project reference after moving them.

## Screens

Sign-in/registration, office/room/date selection, map with selectable desks, individual dates and hours, own reservations and history. Administrator/Owner additionally have office/schedule management, a compact layout editor with side-by-side Room/Desk forms and live maps, archiving, closure previews, members and organisation history. Owner can access the demo directory and test mailbox.

Maps and history display the selected office's times rather than the browser time zone. The backend rechecks permissions, time windows and conflicts. Forms provide loading, error and empty states and prevent duplicate submissions; reservation/closure conflicts refresh data after 409.

## Layout editor

Each Room or Desk card sits on the left of its own map. Fields use one row per value; X/Y, room dimensions and desk rotation each have a slider directly below the numeric input. Both controls update the preview while editing, before saving.

New rooms are highlighted immediately on the office map. The selected room uses draft position/dimensions while other rooms retain their saved geometry. Coordinate guides show the offset from the office origin. The desk map shows all active saved desks and the current draft, with rotation around its centre. Click a room/desk or select it from the list to edit it; Enter/Space also selects focused map items. Save buttons still persist changes through the existing API.

On narrow screens, the preview appears below its form. Save a new room before adding desks, and save changed room dimensions before saving a desk.

Manual acceptance: create a new room without saving, edit each number and slider, and confirm the highlighted rectangle/coordinate summary update immediately. Save/reload and verify the geometry. Select a saved room and add/edit a desk, checking both coordinate sliders and rotation. Confirm other saved rooms/desks remain visible and selectable. Repeat at a narrow viewport and with keyboard navigation.

## Checks

~~~powershell
pwsh -NoProfile -File scripts/Check-TextFormat.ps1
~~~

HTTP acceptance, external integration limits and UI scenarios: [PrototypeRunbook.md](../DeskoviaEngine/docs/PrototypeRunbook.md). Completed checks and unverified conditions: [Verification.md](../DeskoviaEngine/docs/Verification.md).

## Package restore and Microsoft sign-in

Repository-local NuGet.Config clears inherited package/audit sources and uses HTTPS nuget.org. Corporate HTTP feeds are not required.

Sign in with Microsoft performs genuine single-tenant authentication through the backend's OpenID Connect callback. It creates an account only after Microsoft's response is validated. Local password forms remain available. Account linking and the Sign-in settings entry have been removed. Existing Microsoft identities continue to sign in normally. The email-only demo flow and countdown have been removed.

Configuration and acceptance: ../DeskoviaEngine/docs/EntraLogin.md. The backend's Web redirect URI is https://localhost:7095/signin-oidc, and the frontend stays at https://localhost:7178. Client secrets never belong in this repository. This change requires no additional migration.

## Workspace selection and aligned controls

Choose Office, then Room from matching dropdowns; only the selected room's map is displayed. Desks are selected directly on the map with mouse or keyboard. Duplicate desk buttons beneath the map are removed; occupied desks retain hover details and selecting one shows its reservation details. Changing the room clears the previous desk and selected booking dates. Offices/rooms without active resources have explicit empty states.

Shared toolbar rows place labels above fields and align dropdowns, dates, action buttons and text on the control row. The same layout applies to office administration, history filters/pagination, closures, member actions and billing. On narrower screens, rows wrap with their labels kept beside their own field.

Manual verification after rebuilding the frontend: select an office and each room, verify only its desks appear, select a desk and booking dates then switch rooms (the selection must clear), and inspect selector/button rows at desktop and mobile widths. The frontend build and BOM/CRLF/spaces checks pass; authenticated visual/browser acceptance is not claimed by compilation alone.

The application has no payment flow, subscription or booking paywall. The one-time project delivery price is managed outside Deskovia.
