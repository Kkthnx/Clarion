# Code signing

Clarion asks for administrator rights, so an unsigned download that triggers a SmartScreen warning
is the biggest trust problem it has. Signing is part of the 1.0 gate, next to finishing the beta.

## State today

- Nothing is signed. The README says so.
- `scripts/build-installer.ps1` can package an already published folder (`-PublishDir`), which is
  what a signing step needs.
- `.github/workflows/release.yml` builds the setup program and zip on a version tag. Its signing
  steps run only when the repository variable `SIGNPATH_ORGANIZATION_ID` exists. The workflow passed
  `actionlint` but has not been run on GitHub yet.
- Two things are signed when it is switched on: the app files (before they go into the installer and
  zip), then the setup program. A zip itself cannot be signed.

## Options

| Option | Cost | Who it suits | Catch |
|---|---|---|---|
| [SignPath Foundation](https://signpath.org/terms) | Free for open source | Open source projects, anywhere | Needs an OSI approved license, an automated build, a released version, active maintenance. The publisher shown to users is SignPath Foundation, not you. Requests may need a manual approval. |
| [Azure Artifact Signing](https://learn.microsoft.com/azure/artifact-signing/quickstart) | Paid, about $10 a month in one report | Closed or open source | Individuals must be in the US or Canada for public trust. Microsoft's pages disagree on the organization country list, so check the live page. |
| A commercial certificate authority | Paid | Anyone | Not researched here. |

## What blocks the free route

Clarion's README says "All rights reserved for now" and the repository has no license file. SignPath
Foundation needs an OSI approved license with no commercial dual licensing and no proprietary
components. That is a decision for the owner, not something to settle in a build script. If Clarion
stays closed, Azure Artifact Signing or a commercial certificate is the way instead, and the
workflow only needs its two signing steps swapped.

## Steps for SignPath Foundation

1. Choose and add an OSI approved license.
2. Apply at [signpath.org](https://signpath.org). Describe what Clarion does on the download page.
3. In SignPath, create the project and two artifact configurations named `app` and `setup`.
   `app` signs `Clarion.exe` and Clarion's own `.dll` files inside the uploaded zip. `setup` signs
   `Clarion-*-setup.exe`. Add the predefined GitHub trusted build system to the project.
4. In the GitHub repository settings add:
   - secret `SIGNPATH_API_TOKEN` (a token for a user who can submit signing requests)
   - variables `SIGNPATH_ORGANIZATION_ID`, `SIGNPATH_PROJECT_SLUG`, `SIGNPATH_SIGNING_POLICY_SLUG`
5. Run the workflow from the Actions tab. Check that the setup program and `Clarion.exe` show the
   signature in file properties.
6. Update the README line that says the files are not signed, and say who the publisher is.

## Not known yet

- How quickly Windows SmartScreen stops warning for a newly signed program was not researched.
  Do not promise users "no warning" until it has been tested with a real signed build.
- The exact artifact configuration XML for SignPath depends on the project. It is created in their
  dashboard, so it is not stored in this repository.
