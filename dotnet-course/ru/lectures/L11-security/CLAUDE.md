# Л11. Безопасность: аутентификация и авторизация — house rules

Working notes for this lecture's directory. Course-wide rules live in
[`../../../CLAUDE.md`](../../../CLAUDE.md); read that first. This file carries
only what is specific to Л11.

| File | What it is |
|---|---|
| `L11-security.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses with the frame each one lives on; `[—]` = voice only. |
| `slides/L11-security-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB11-task.md`, `lab/LAB11-review.md` | The lab for the student and for whoever accepts it. |

Adding, cutting or reordering a frame invalidates the cue numbers and the
README tags — redo both in the same commit.

## Shape

- **58 frames**: title 1; §1 2–8, §2 9–31, §3 32–41, §4 42–52, §5 53–58.
  Timings 10 + 25 + 15 + 20 + 10 = 80.
- **Nine `\vote` frames** (3, 10, 16, 24, 27, 35, 43, 50, 56), one per
  `[ВОПРОС В ЗАЛ]`. **Keylines** close §2–§5 (31, 41, 52, 58); §1 ends on
  content. **One `\statement`**: 17, «Токен подписывается, а не шифруется».
  No `\shout`, no speaker notes, no lab mentions on slides.
- Frame order follows the script: «Пока издателя нет» (29) precedes «Что за
  двумя строками» (30); «403 или 404» (39) precedes the own/foreign/admin
  output (40). Do not swap them back.
- The script's one resource-authorization listing is split over 37 (requirement
  and handler) and 38 (endpoint); long lines are broken so the text labels
  clear the code. Listings may be reformatted, output may not.
- The script was changed only in its cues, plus removal of the `[ЛАБА 11 — …]`
  block (now the two lab files). §4 still says Keycloak is set up in the lab;
  by the lab-chain contract it is the optional extra — see README, open item 1.

## How the slides were checked

Scratch `dotnet new web -n HelloWorld` (not in the repo), dotnet SDK 10.0.114,
`Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.15, Development, port
5090, curl 8.5.0, jq 1.7. All C# on frames 8, 28, 34, 37, 38 compiles in that
one project. Tokens come from `dotnet user-jwts` (`$TOKEN` = user 42, role
customer; `$ADMIN`; `$MANAGER` with `department=sales`; `$EXPIRED`; `$REPORTS`
with `--audience reports-api`). Output on 5, 8, 11, 12, 14, 18, 22, 29, 34, 40
and 54 is pasted from those runs; frames 11–14, 18, 29 show the same token.

- Over-long output lines are wrapped (22: before `error_description=`; 54: the
  project path), the token on 29 is cut with `...`; nothing else is touched.
- 11 uses `fold -w 88` because wider lines leave the code ground.
- 18: `${TOKEN/NDIi/NDMi}` turns `sub` 42 into 43; the real reason text is
  «The signature key was not found». 22: the wrong-audience text really says
  `'(null)'`. Both are JwtBearer's wording for HS256 tokens without `kid`.
- `dotnet user-jwts create --audience X` rewrites `ValidAudiences` in
  `appsettings.Development.json`: create the odd-audience token first.
- The JSON header on 20 (`RS256`, `kid`) is the script's example, not output.
- Frames with `|` in a command (11, 12) or `||` in code (37) use
  `` escapechar=` ``.

## Lab

About twenty minutes, three steps and a commit: JWT bearer with `user-jwts`
tokens; policy `"Admin"` on `GET /orders`; `OrderOwnerHandler` through
`IAuthorizationService`, foreign order → `404`, `POST` takes the email from the
token. It starts from the LAB10 state and must leave the LAB11 state of the
chain contract — keep the names (`IOrderStore`, `AddOrders()`, `"Admin"`,
`OrderOwnerRequirement`, `OrderOwnerHandler`, claim `email`). `CreateOrderDto`
keeps `CustomerEmail` (later labs rely on the DTO and its validator); the
handler overrides it. The package needs `--version "10.0.*"`: .NET 11 is out
and the unpinned package fails with NU1202. Do not grow the lab: fallback
policy, expired tokens, Production and `--audience` are examiner prompts.

`LAB11-review.md` quotes a stub `Shop.Domain` + `Shop.Api` reference run on
port 5090. Not run: the `/total` route with `RatesClient`, and the Keycloak
extra task (its notes are expectations, marked as such).
