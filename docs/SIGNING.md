# Code signing

.Clarion asks for administrator rights, so an unsigned download that triggers a SmartScreen warning
is the biggest trust problem it has. Signing is part of the 1.0 gate, next to finishing the beta.

## State today

- Nothing is signed. The README says so.
- `scripts/build-installer.ps1` can package an already published folder (`-PublishDir`), which is
  what a signing step needs.
- `.github/workflows/release.yml` builds the setup program and zip on a version tag. Its signing
  steps run only when the repository variable `SIGNPATH_ORGANIZATION_ID` exists. It has run on GitHub for
  the beta tags, unsigned, and passes `actionlint`. The signing steps themselves have never run. On a version
  tag it also drafts the GitHub release, as a draft and marked a pre-release when the version has a dash,
  with the matching changelog section as the notes and the four release files attached.
- Two things are signed when it is switched on: the app files (before they go into the installer and
  zip), then the setup program. A zip itself cannot be signed.

## Options, with what each one costs and needs

Checked on 2026-10-11 against Microsoft's page on [code signing options](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options) (updated 2026-08-29) and the [SignPath Foundation terms](https://signpath.org/terms).

| Option | Cost | Fits .Clarion today? |
|---|---|---|
| Microsoft Store as an MSIX package. The Store signs it, and users see no SmartScreen warning. | Free, worldwide. Individual registration is free. | Not without a rewrite. The app needs administrator rights, and a packaged app can only ask for them through the restricted `allowElevation` capability, which a Store vendor describes as very unlikely to be approved. How the package would treat the registry and files .Clarion changes was not confirmed from Microsoft's documentation. |
| SignPath Foundation | Free | Not under the current licence. It needs an OSI approved licence on every part, with no dual licensing. The publisher shown is SignPath Foundation, the product name must match the project name, it reviews the project's reputation first, every release is approved by hand, and the project page needs a code signing policy section. |
| Azure Artifact Signing | About $9.99 a month. Free and trial Azure subscriptions are not accepted. | Yes for an individual in the USA or Canada, but not free. Reputation still builds over time, so early releases can still show a warning. |
| An OV certificate from a certificate authority | About $150 to $300 a year, and the key must live on a hardware token or cloud service. | Not free. The same reputation rule applies. |
| A self-signed certificate | Free | No. Windows blocks it for anyone who has not installed the certificate by hand. |
| No signature | Free | What it is today. SmartScreen blocks a download it has not seen before. |

No option is both free and fits a source-available, all rights reserved app. The two that cost nothing each need something that .Clarion does not have: an OSI licence, or a Store package.

## What is done without a certificate

- Every release file has a SHA-256 file next to it.
- Since the build for 0.1.0-beta.16 every release file also carries a build attestation from GitHub, which is free. It is a signed statement that GitHub's runner built these exact files from this commit. Check one with `gh attestation verify <file> --repo Kkthnx/Clarion`. Windows does not read it, so SmartScreen is unchanged.
- Windows SmartScreen tracks an unsigned file by the file itself, so each new build starts with no reputation. A signature moves the reputation to the publisher, which is what carries over between releases.

## Ways to get a certificate

1. Change the licence to an OSI approved one and apply to SignPath Foundation. That is a decision about the whole project, and it also means every part of .Clarion is open. The project is only days old, so the reputation review may not pass yet.
2. Pay about $10 a month for Azure Artifact Signing. Donations on the About page could cover that, and nothing else changes.
3. Keep releasing unsigned, which is honest in the readme, and sign once either of the above is possible.
## Steps for SignPath Foundation (only if the licence changes)

1. Apply at [signpath.org](https://signpath.org). Describe what .Clarion does on the download page.
2. In SignPath, create the project and two artifact configurations named `app` and `setup`.
   `app` signs `Clarion.exe` and .Clarion's own `.dll` files inside the uploaded zip. `setup` signs
   `Clarion-*-setup.exe`. Add the predefined GitHub trusted build system to the project.
3. In the GitHub repository settings add:
   - secret `SIGNPATH_API_TOKEN` (a token for a user who can submit signing requests)
   - variables `SIGNPATH_ORGANIZATION_ID`, `SIGNPATH_PROJECT_SLUG`, `SIGNPATH_SIGNING_POLICY_SLUG`
4. Run the workflow from the Actions tab. Check that the setup program and `Clarion.exe` show the
   signature in file properties.
5. Update the README line that says the files are not signed, and say who the publisher is.

## Not known yet

- How quickly Windows SmartScreen stops warning for a newly signed program was not researched.
  Do not promise users "no warning" until it has been tested with a real signed build.
- The exact artifact configuration XML for SignPath depends on the project. It is created in their
  dashboard, so it is not stored in this repository.
