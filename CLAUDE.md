# LabHub — project rules

Educational materials site (ASP.NET Core 8, Razor Pages). Public side is for
students. Hosting: **GitHub Pages** — the app is the renderer and local
preview, `tools/export_static.py` turns it into a static tree, and
`.github/workflows/pages.yml` publishes it on every push to `main`. Any repo
name works: the workflow derives the URL prefix from the repo name (empty for
`<org>.github.io`, `/<repo>` otherwise) and the exporter rewrites all internal
links with it. Live site: repo `LabHub` in org `Yarik-Developer-Group` →
https://yarik-developer-group.github.io/LabHub/. Admin panel, Google OAuth and the content
webhook still exist in code but are NOT used in production anymore (Azure
hosting was dropped deliberately); do not build on them. Owner: Yaroslav
(ybalaba@teamworkcommerce.com), chat language — Russian.

## Commands

```bash
dotnet run --project src/LabHub.Web        # http://localhost:5204
dotnet test                                # xunit, all tests must stay green
python3 tools/check_style.py               # gate: functions ≤3 params, ≤30 lines
python3 tools/export_static.py static-out  # static export (site must be running)
```

CI (`.github/workflows/pages.yml`): tests + style gate → run the site →
static export → deploy to GitHub Pages. Every push to `main` redeploys —
content and code changes go through the same pipeline.

## Iron code rules

- **No comments in code** — none, anywhere (C#, csproj, yml).
- Functions: **max 3 parameters, max 30 lines** (enforced by script).
- Error handling is mandatory: the app never crashes — broken content is
  skipped, unreachable GitHub does not block startup, exception = 500 page.
- Security: strict CSP with no inline scripts and no CDNs (everything
  self-hosted, including KaTeX), raw HTML in markdown is escaped, path
  traversal on `/assets` is closed, webhook check runs in constant time.
- Tests are mandatory for new functionality (unit + integration via a live
  app on a random port, fixture `LabHubAppFixture`).

## Architecture (brief)

Content = a markdown tree in `/content`, parsed into an immutable
`ContentSnapshot` (all HTML pre-rendered, Markdig + Mathematics), held in
`ContentStore` (volatile reference, atomic swap). Routing: `/c/{**path}`.
Images/files — `/assets/{**path}` (extension allowlist). Formulas: `$x^2$`
and `$$\frac{a}{b}$$` → KaTeX (self-hosted, `wwwroot/lib/katex`). Red callout
boxes in lectures: `:::lab-callout ... :::` custom container.

## Content rules

- Content language — **Ukrainian**; UI strings too. Programming language and
  DBMS in assignments are **not imposed** (recommendations: MS SQL /
  PostgreSQL, differences as "in MS SQL — like this, in PostgreSQL — like
  this" asides).
- Front-matter: `title`, `order` (sorting, ties break alphabetically),
  `summary`, `track` (groups children into tracks on the parent page).
- Folders named `_name` or `.name` are hidden (drafts), not published.
- Folders containing only images (no .md) do not become pages.
- Structure: `content/general` (order 0, knows nothing about specific
  courses), `content/networks` (theory / labs ЛР1–ЛР8 / labs/extra /
  vm-practicals / theory/extra), `content/databases` (theory / practice
  ПР1–ПР8 / extra / vm-practical), `content/python` (topics 1–8, homework,
  quizzes).
- Quizzes = летучки only (the big multiple-choice test was dropped
  deliberately: "pick the true statement" is outdated); answer keys are
  published next to them (`answers-*.md`) — philosophy: "not to flunk, but
  to find the gaps".
- Extra-lab numbering in databases goes by branches: С*, Р*, ДП*, ДА*
  (not sequential!). Networks extra labs have NO numbers at all; each starts
  with a "коли виконувати" note naming the lab it builds on.
- Tone: lively but no profanity; a "доказ" section at the end of every exam
  task.

## Sync (important for cloud sessions)

The source of truth is the user's working copy
`C:\Common\GIT\PetProjects\LabHub` (git repo there, commits authored as
Yaroslav). The cloud copy at `/home/claude/labhub` is the session workspace;
the two commit histories diverge — that is normal, the device wins.
On the device mount git cannot delete its own lock files: before/after git
commands move `.git/index.lock`/`HEAD.lock` into `_to_delete/` (folder is in
`.gitignore`, the user empties it himself). Transport zips (`*-sync.zip`)
are gitignored too — never commit them. The path `.github/workflows/` is
protected from remote writes: deliver workflow files to a temp name and move
them into place with a device shell command.

## NuGet and the sandbox

In the cloud sandbox NuGet is blocked: packages (Markdig, xunit, …) restore
from the local feed `/home/claude/localfeed` (Markdig packed from sources,
xunit — an API-compatible shim run via /tmp/xrunner). On the user's machine
and in CI — regular nuget.org.

## Roadmap

Long-term plan lives in `ROADMAP.md` (Ukrainian): stage 0 — strip dead
server code (admin/OAuth/webhook) + feedback channel before the pilot;
stage 1 — autumn 2026 pilot of the networks course (course frozen, keep a
pilot journal, decide ЛР5 squeeze and other changes only after);
stage 2 — template repo so other teachers can start their own course;
stage 3 — .NET course (winter–spring 2027, opens with the HTTP-evolution
lecture, brings back Docker/k8s/OTel); stage 4 — small static teaching
tools. Update the roadmap when a stage completes.

## Open threads

- Course «Комп'ютерні мережі» — COMPLETE and FROZEN for the pilot student
  cohort (decision Aug 2026): measure how they cope, then adjust. Layout:
  intro lecture (sockets, ARP + ND footnote), theory/extra popular-science
  lectures (CDN/torrents, datacenters/AI, wireless incl. roaming vs
  handover, eavesdropping, Tor), the single monolithic crypto lecture
  `crypto-intro.md` (bricks: hash+salt, AES/modes/keys/CSPRNG, MAC,
  asymmetry with RSA-on-small-numbers and DH, signatures; assemblies:
  certificates/CA/chain of trust, AKE/PFS, finale TLS/SSH/E2EE, PAKE as
  read-only concept, epilogue "how crypto really breaks" merged inside).
  Labs (2–3 tasks each, language-agnostic body, Python hints in примітки,
  pass/fail — no extra credit ever): ЛР1 TCP, ЛР2 UDP video stream,
  ЛР3 HTTP by hand, ЛР4 data formats (XML/JSON/protobuf via file),
  ЛР5 web-API/REST (+debugging: curl/Postman/Bruno/Swagger/pytest; 5 tasks —
  known deviation, candidate to squeeze after the pilot), ЛР6 «Від Цезаря
  до AES», ЛР7 «Підписи, ключі та паролі», ЛР8 «HTTPS на власні очі» —
  the FINAL lab (Wireshark on loopback catches a plaintext password →
  openssl self-signed cert → TLS-wrap the ЛР3 socket server →
  ClientHello/Application Data in the sniffer; theory is links back into
  the crypto lecture). Extra labs live in `labs/extra` without numbers;
  currently one: telemetry-logs.md (one service, interleaved parallel logs →
  correlation ID by hand, then via the logging library; do after ЛР5).
  Lectures carry red `:::lab-callout` boxes linking to labs.
- CUT from the 2nd-year course (Aug 2026): Docker, Docker Compose,
  Kubernetes (old drafts never entered the repo), the VPN lab, and
  gRPC + HTTP/1.1 vs 2 vs 3 / QUIC — the latter is deferred to NEXT year's
  course as its opening lecture before microservices (QUIC is a callback to
  UDP from ЛР2). Infrastructure tail of year 2 is only the classroom VM
  practical (SSH keys/isolation/tunneling, `vm-practicals/`).
- Databases «Теорія» section keeps growing; git guides in «Загальне» wait
  for screenshots from a Google Docs export (user drops the export into the
  repo root).
- Student feedback via giscus and a search page — nice-to-have, not started.
