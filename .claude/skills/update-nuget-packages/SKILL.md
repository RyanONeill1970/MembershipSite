---
name: update-nuget-packages
description: Update/upgrade/bump NuGet packages in the MembershipSite solution — find outdated (and vulnerable) packages, update them in related groups, build + run unit tests after each group, and commit each group separately with a specific message. Use whenever the user asks to update dependencies, bump/upgrade NuGet packages, patch vulnerable packages, deal with outdated packages, or clear a `dotnet list package --outdated` list — even if they don't say the word "NuGet".
---

# Update NuGet packages

This solution has **no central package management** (no `Directory.Packages.props`, no
`Directory.Build.props`, no `nuget.config`, no `packages.config`). The solution
[`MembershipSite.Website.sln`](../../../MembershipSite.Website.sln) is all SDK-style projects on
**net9.0**, so versions live in each project's `<PackageReference ... Version="...">` inside the
individual `.csproj` files. There are six projects:

| Project | Has package references? |
|---------|-------------------------|
| `MembershipSite.Contracts` | no |
| `MembershipSite.ViewModels` | no (FrameworkReference only) |
| `MembershipSite.Datalayer` | EF Core, DataProtection, Sentry |
| `MembershipSite.Logic` | BCrypt.Net-Next, CsvHelper, MailKit, DataProtection, Microsoft.Extensions.Configuration |
| `MembershipSite.Website` | ASP.NET Core, EF Core Design, TypeScript.MSBuild, LibraryManager.Build, Sentry.AspNetCore |
| `MembershipSite.UnitTests` | xunit v2, Test.Sdk, coverlet, EF Core SqlServer |

Several packages (`Microsoft.AspNetCore.DataProtection.EntityFrameworkCore`,
`Microsoft.EntityFrameworkCore.*`) are referenced by **more than one project** at the same version —
they must all move together, or you get assembly version mismatches at runtime.

The goal is a **clean, per-group commit history** where every commit is one coherent set of package
bumps that builds and passes unit tests on its own. Invoking this skill is the user's authorization
to commit as you go — this run overrides the standing "commit only when asked" rule for these
package-update commits (and only these). Commit directly to `master` (the standing workflow here); do
not branch first, and never `git push`.

## Workflow at a glance

1. **Discover** — list outdated and vulnerable packages, and check for open Dependabot PRs.
2. **Group** — cluster packages that move together (same family / interdependent / same area).
3. **Per group**, in a loop: research (if major) → update → build → test → fix-or-revert → commit.
4. **Report** — summarise what was committed and what was skipped and why.

Work one group fully before starting the next, so a failure never leaves a half-updated tree.

## The working tree is usually dirty — never blanket-stage

This repo normally has a large pile of **untracked content** under `MembershipSite.Website/secure/`
and `wwwroot/` (members-only HTML, images, PDFs) plus unrelated modified files. **Never** use
`git add .`, `git add -A`, or `git commit -a`. Always stage the specific `.csproj` files you changed,
by path. Run `git status` before your first commit so you know what was already dirty and don't
attribute it to yourself.

## Step 1 — Discover what's outdated

Default to the whole solution unless the user named a project:

```bash
dotnet list MembershipSite.Website.sln package --outdated
```

```bash
dotnet list MembershipSite.Website.sln package --vulnerable
```

Add `--include-transitive` only if the user cares about transitive advisories. Capture the full list
(package, current version, latest version, and which project(s) reference each) — you'll need the
per-project mapping because there's no central version list to edit.

**Also check Dependabot.** `.github/dependabot.yml` runs a weekly NuGet check, so there may already
be open PRs for some of these bumps. Look before you start (`gh pr list`) and tell the user if your
work would duplicate or conflict with them.

## Step 2 — Group related packages

Each group becomes **one commit**, so group things that genuinely belong together. For this solution
the natural groups are:

- **Microsoft platform packages** — `Microsoft.EntityFrameworkCore.*`,
  `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore`,
  `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation`, `Microsoft.Extensions.*`. These all track the
  ASP.NET Core / EF Core release train and are currently pinned together on the same `9.0.x`. Move
  them in lockstep, across **every** project that references them.
- **Sentry** — `Sentry` (Datalayer) and `Sentry.AspNetCore` (Website) must share a version.
- **Test tooling** — `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`,
  `coverlet.collector`. Test-only, so a failure here is cheap and isolated.
- **Build-time tooling** — `Microsoft.TypeScript.MSBuild`, `Microsoft.Web.LibraryManager.Build`. See
  the LibMan warning below; keep these as their own commit(s), not folded into anything else.
- **Standalone libraries** — `BCrypt.Net-Next`, `CsvHelper`, `MailKit`. One commit each, or one
  commit if they're all trivial patch bumps.

Keep groups small and coherent. When unsure, a smaller group (even a single package) is safer than a
grab-bag, because a build/test failure then isolates cleanly to one revertable commit.

## Step 3 — The per-group loop

For each group, in order:

### 3a. Research majors before touching anything
Patch and minor bumps are applied directly. If **any** package in the group crosses a **major**
version boundary, first research breaking changes (release notes, the project's GitHub
changelog/releases, or its migration guide) and briefly summarise the risk before editing. If a major
looks disruptive, say so and let the user decide whether to include it — don't silently pull in a
breaking major.

Known hazards in this repo:

- **`Microsoft.*` 10.x on a net9.0 solution.** `--outdated` will offer `10.0.x` for EF Core, ASP.NET
  Core and `Microsoft.Extensions.*`. Those generally require net10.0. **Do not take them** while the
  projects target net9.0 — retargeting the whole solution is a separate job the user has to ask for.
  Stay on the newest `9.0.x`.
- **`Microsoft.Web.LibraryManager.Build`.** History shows a LibMan bump being taken and then reverted
  (`736eac7` "Update libman to latest" → `3d39e9c` "Revert buggy LibMan"). Bump it alone, and after
  building, confirm the client-side libraries in `MembershipSite.Website/libman.json` still restore
  into `wwwroot/lib`. If anything looks off, revert rather than debug it.
- **`MailKit`** drives the whole email/auth flow (registration, password reset), and **`BCrypt.Net-Next`**
  hashes stored passwords. A major on either is high-risk relative to how thin the test suite is —
  flag both for the user instead of taking them silently.

### 3b. Apply the bump
Update **every** project that references the package so versions stay consistent across the solution.
Either edit the `Version="..."` attribute on each `<PackageReference>`, or:

```bash
dotnet add <project.csproj> package <PackageId> --version <newVersion>
```

Note the two `.csproj` indentation styles in this repo (Datalayer/Contracts/UnitTests use spaces,
Logic/ViewModels/Website use tabs). Match whatever the file already does.

### 3c. Build the solution
```bash
dotnet build MembershipSite.Website.sln
```
Scoping the build to the affected project(s) is fine for speed while iterating, but confirm the
solution builds clean before committing the group. The Website build also runs TypeScript compilation
and LibMan restore, so a build failure there may be tooling rather than C#.

### 3d. Run the unit tests
The test project is **`MembershipSite.UnitTests`** (xUnit v2, net9.0). `dotnet test` works normally
here:

```bash
dotnet test MembershipSite.Website.sln
```

**Treat a green run as weak evidence.** The suite is currently a placeholder — essentially one test
class (`MailgunTests/WebhookDeserialisationTests.cs`). It will not catch a broken auth, email, or EF
mapping change. A clean build plus your own reading of what the package changed is doing most of the
work here; say so honestly in the report rather than implying the tests proved anything.

### 3e. Fix, or revert and skip
If the build or tests fail, **attempt a fix first** — a bump that broke a call site is often a small,
legitimate code change; match the surrounding code's style. Re-run 3c–3d after fixing. If it can't be
fixed in a reasonable effort, **revert that group's changes** and continue with the remaining groups:

```bash
git checkout -- <the .csproj files you changed>
```

Be precise with the paths — a bare `git checkout -- .` would also blow away the unrelated modified
files that are usually sitting in this working tree.

Record the group as skipped with the reason (e.g. "EF Core 10 needs net10.0 — needs owner decision")
for the final report. One stubborn group must never block the rest.

### 3f. Commit the group
Once it builds and tests pass, commit just this group with a specific message:

- **Message style** matches recent history: sentence case, imperative, ending in a full stop, with an
  optional area prefix (`Website: `) when the change is confined to one project. Past dependency
  commits here read `Update packages.`, `Update Sentry packages.`, `Update all packages.` — keep that
  voice but be more specific about what moved:
  - `Update Entity Framework and ASP.NET packages to 9.0.11.`
  - `Update Sentry packages to 5.14.0.`
  - `Website: Update TypeScript build package to 5.9.2.`
  - `Update MailKit to patch CVE-2024-… .`
- **No `Co-Authored-By:` / "Generated with" trailer** (owner preference; `includeCoAuthoredBy: false`
  is set in settings).
- If the message contains an **apostrophe**, write it to a temp file and use `git commit -F <file>`
  — inline quoting mangles apostrophes on this Windows/PowerShell setup.
- Stage only the files this group changed (`git add <those .csproj files>`), then commit. **Do not
  push.**

Then move to the next group.

## Step 4 — Final report

Summarise as a table so the state is obvious at a glance:

| Group | Packages | Version change | Major? | Build | Tests | Result |
|-------|----------|----------------|--------|-------|-------|--------|

End with an explicit list of any **skipped** groups and why, plus a one-line reminder that nothing was
pushed, that the test suite is thin so build-clean is not the same as verified, and that majors
flagged for the user still need a decision.

## Guardrails

- **Patch/minor:** apply freely. **Major:** research + flag before including.
- **Stable only** — don't move a package onto a prerelease/RC version unless the user asks.
- **Never bump a package past net9.0 support** — in practice, no `Microsoft.*` 10.x until the user
  asks for a framework retarget.
- Keep each commit self-consistent: it must build and pass unit tests on its own, so the history stays
  bisectable.
- Never blanket-stage; this working tree is routinely full of unrelated untracked members-only content.
- **Client-side packages are out of scope.** `libman.json` (jQuery, Bootstrap, etc.) is not NuGet —
  only touch it if the user explicitly asks.
