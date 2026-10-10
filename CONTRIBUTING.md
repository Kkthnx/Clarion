# Contributing to Clarion

Thank you for looking. This page says how the project is kept honest, and how to add a setting.

The licence is not chosen yet. Until it is, please open an issue before sending a large change, so nothing is built that cannot be used.

## What stays true

- Every setting is explained, can be reverted, and has a source. A setting without all three does not go in.
- Clarion never turns off Defender, SmartScreen, security updates or exploit protection.
- Clarion sends nothing anywhere. The only network request it can make is the update check, which is off until you turn it on.
- A change is recorded before it is made, and the value it replaced is kept, so Revert puts back what was really there.

The voice is in [docs/BRAND.md](docs/BRAND.md): plain, short, honest. No hype, no scare words.

## Build and test

You need the .NET 9 SDK on Windows 11.

```
dotnet build Clarion.sln -c Release
dotnet test Clarion.sln -c Release
```

The build treats warnings as errors. Some tests touch the real system (registry, services, DNS, Task Scheduler) and are skipped unless you opt in from an elevated shell with `CLARION_REAL_TESTS=1`. Read a test before you run it that way.

`CLARION_DATA_DIR` points Clarion at another data folder, which is how to try it on a clean slate without touching your own history.

## Adding a setting

The catalog is JSON under `src/Clarion.Core/Catalog/Data/`. The fields are described in [docs/DESIGN.md](docs/DESIGN.md), and every existing entry is a worked example. The list of all of them is [docs/CATALOG.md](docs/CATALOG.md).

1. Find the Microsoft documentation for the setting. Prefer a policy that is documented (`Evidence: Proven`) to a value people found by looking. Put the address under `sources`.
2. Write the entry. Use the typed operations that already exist (`registry.set`, service, task, feature, app package). The undo is captured when the setting is applied, so you do not write one.
3. Say plainly what it does, what it gives, and what it risks. If the effect only shows on some builds or editions, say that in `requires`.
4. Try it on a real PC. Apply it, look for the effect, revert it, and check the value came back. Put the Windows build you tried it on in `lastVerifiedBuild`. If you could not see the effect, say so in the pull request instead of claiming it.
5. Write the reference page again, then run the tests:

   ```
   Clarion.exe --catalog-doc > docs/CATALOG.md
   dotnet test Clarion.sln -c Release
   ```

The catalog tests will tell you what is missing: a source for a setting marked Proven, the build it was checked on, an id that is already used, or a service that must never be touched.

## Changing the app

- Keep a change small and about one thing. Do not reformat files you are not changing.
- Add a test that fails without your change. For a bug, write the test first and watch it fail.
- Do not weaken or delete a test to get a build through. If a test is wrong, say why in the pull request.
- If you could not run a check, say which one and what is left unchecked.
- The app has three layers. `Clarion.Core` holds the rules and has no Windows code. `Clarion.Engine` talks to Windows. `Clarion.App` is the window. Put logic where it can be tested, which is usually Core.
- Colors come from the theme file, not from the page. A test keeps every text color readable in both themes.
- Give anything a screen reader would meet a name (`AutomationProperties.Name`), and check the page at the smallest window size.

## Commits and pull requests

Short, imperative commit messages ("Fix the stuck repair dialog"), one logical change each. Put the reason in the body when it is not obvious. Update `CHANGELOG.md` under Unreleased in the same plain voice, grouped as New, Safer, Fixed or Polish.

## Reporting a problem

The Report a problem page in the app builds the report for you and shows it before anything is sent. Nothing is sent from the app. You read it and open it on GitHub yourself.
