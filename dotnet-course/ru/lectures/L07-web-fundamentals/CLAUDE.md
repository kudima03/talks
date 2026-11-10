# Л7. Веб-фундамент: HTTP и стили взаимодействия — house rules

Working notes for this lecture's directory. Course-wide rules — the palette,
the two-colour system, what may go on a slide, the build, the slide-cue
convention — live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that
first; this file carries only what is specific to Л7.

## What is in here

| File | What it is |
|---|---|
| `L07-web-fundamentals.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses — a summary and a crib sheet for the lectern, with the frame each thesis lives on. |
| `slides/L07-web-fundamentals-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB07-task.md` | The lab, as handed to the student. |
| `lab/LAB07-review.md` | The same lab for whoever accepts it. |

The five move together. Every thesis in `README.md` carries a frame number
and `[—]` marks a thesis with no frame. **Adding, cutting or reordering a
frame invalidates those numbers** — renumber the cues in the script, then
re-derive the tags in `README.md`, in the same commit. The lab starts a new
console project, `Shop.RawServer`, next to `Shop.Domain`/`HelloWorld`; it
references `Shop.Domain` and asks for nothing the lecture does not cover. Lab
8 rebuilds the same service on ASP.NET Core and moves `OrderStore` over as
is, so the routes, the two seeded orders and the response texts are a
contract between the two labs — change them in both.

## Shape specific to Л7

- **67 frames**: §1 2–19, §2 20–39, §3 40–48, §4 49–67. Seven `\vote` frames
  (3, 13, 27, 31, 41, 50, 56) — one per `[ВОПРОС В ЗАЛ]`, the question alone.
  Section timings 20 + 25 + 10 + 25 = 80.
- **No speaker notes in the deck** — no `\note{}`, on request. Delivery lives
  in the script only.
- **The four layers are one overview frame (4) and then one frame per layer**
  with a diagram and marks: DNS (5), TCP handshake (6), TCP on the server —
  port, accept queue, socket (7), TLS handshake (8), certificate checks (9),
  HTTP (10). Review decision (PR #19); do not fold them back into one ladder.
- **Frame 10 carries the mark «Сервер не может начать разговор первым»**, and
  frame 11 ends on a large «Переиспользуй соединения!» — a plain large line,
  not a `\shout`. Both were asked for in review.
- **Head-of-line blocking (14) and QUIC's features (15) are separate frames**;
  frame 12 is UDP against TCP only.
- **§2 opener reads «Структура HTTP»** (review), not «HTTP — это текст…».
- **Methods (25), status groups (29) and 4xx (30) carry a short explanation
  per row**; «ошибка — это код» (32) is its own frame with a 404/200 pair.
- **§3: the four places for state (45) carry no lecture numbers**, and frame
  46 states the rule «состояние — только данные в базе»: client and token are
  what the request brings, cache is an accelerator that may be lost. The cache
  paragraph in the script was changed to match (no basket or draft in cache).
- **§4: every style has its own frame(s)** — REST 51–53, SOAP 54–55, GraphQL
  57–58, gRPC 59–61, SSE 62, WebSocket 63, then 64–66 compare. The section
  opener has no ladder.
- **The socket-server listing at the end was cut** in review («Это не нужно»),
  together with its walk-through paragraph. The `[ЛАБА 7 — …]` block that
  stayed after the closing thought is gone too: its content now lives in
  `lab/`. The spoken mentions of the lab (§2, §4) stay.
- **Comparison order:** the side to follow is on the left in teal — `404`
  before `200` (32), stateless before sessions in memory (42, 43), «можно»
  before «нельзя» (26, 47, 61), SSE before WebSocket (64).
- Frames 19, 39, 48, 67 close the four sections with `\keyline`.

## What is on the slides — how it was checked

The deck has no C# listings, so nothing was compiled.

| Frames | How it was checked |
|---|---|
| 21, 23, 28, 32, 62 | Raw HTTP is an illustration, not captured output. `Content-Length: 25` on 23 matches `printf '{"id":42,"status":"paid"}' \| wc -c`. |
| 22 | URL parts follow RFC 3986; the fragment is never sent. |
| 25 | Safe / idempotent per RFC 9110; `PATCH` is neither (RFC 5789). |
| 7, 18 | Addresses are from the documentation ranges (`203.0.113.0/24`, `198.51.100.0/24`) and `10.0.0.0/8`. |
| 44 | The Twelve-Factor App, factor VI «Processes». |
| 54 | SOAP 1.1 envelope namespace. |
| 59 | The `.proto` is the one from the script; `protoc` is not installed here, so it was not run through a compiler. |

Labels beside raw messages (21, 23, 62) are positioned with `remember
picture` marks inside the listing, so the deck needs two pdflatex passes —
`latexmk` does that. After editing a raw message, check the labels still sit
on their lines in the built PDF.

## Lab — how it was checked

Everything quoted in `LAB07-review.md` and the session in `LAB07-task.md`
comes from a minimal reference implementation — a stub `Shop.Domain` and a
`Shop.RawServer` on `TcpListener`, port 8088 — built and run on dotnet SDK
**10.0.114** (Linux) with curl **8.5.0** and OpenBSD `nc`, in a scratch
directory, not committed to the repo. `curl -v` was run under a pseudo-terminal
so the output has no progress meter, as a student sees it.

| What | How it was checked |
|---|---|
| Session, sizes | `/` is 32 bytes for 17 characters; `/orders` 57, `/orders/1` 27, `/orders/7` 11. |
| `Trying [::1]` | Real: `localhost` resolves to both, the listener is `IPAddress.Loopback` (IPv4). |
| Step 2 | Accept-and-close → `curl: (52) Empty reply from server`. Second instance → `SocketException (98): Address already in use`, exit 134. Ctrl+C with `e.Cancel = true` → `stopped`, exit 0; without it → exit 130. |
| Step 7 | Each break toggled in the reference and run with `curl -v`: chars → body `Магазин р`, `Excess found`, curl exit 0; `+1` → error 18; `+1` without `using` → error 28 «32 out of 33»; correct length without `using` → no error; no empty line → error 8 «Header without colon»; LF in the response → no error; LF in the request → server waits, `timeout` ends `nc` with 124. |
| Step 7.6 | A held `nc` with unfinished headers blocks a parallel `curl` (`Connected`, then error 28 with 0 bytes); after `nc` closes, the server logs `closed before headers` and a `Broken pipe` for the late `curl`. |
| Extra task | Keep-alive: `Re-using existing connection`; `+1` hangs with `using` in place. `curl` collapses `/files/../x` to `/x` unless `--path-as-is`. |
| Compiler | `CS8509` (with the example pattern `("GET", [""])`) and `CS4014` copied verbatim; no `using` on `TcpClient` and no token on `AcceptTcpClientAsync` build clean. Without the token, SIGINT does not stop the server — the next connection is served normally. |

**Not checked:** the browser (the `favicon.ico` request and what a page
without `charset` looks like are stated as typical, not captured), and `nc`
flags on macOS. Re-run the reference if a route, a seeded order or a response
text changes — the byte counts above depend on them.

## Open items

The author's call. Do not quietly "fix" these:

1. **`QUERY` is «проходит стандартизацию»** in the script and on frame 28.
   Check its status and .NET 10 support before the lecture.
2. **The Latin Modern fallback has not been checked** for this deck; it builds
   with PT Sans.
