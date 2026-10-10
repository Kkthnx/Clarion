# Packaging

Clarion is a download from the GitHub release page (a setup program and a portable zip, each with a checksum). `scripts/make-package-manifests.ps1` writes the manifests that put it in two package managers, from the checksums the release published.

```
./scripts/make-package-manifests.ps1 -Version 0.1.0-beta.6 -License "<licence>"
```

It needs the licence to be named on the command line. Both package managers show it to people, and the project has not chosen one, so the script has no default and nothing is submitted until it is chosen.

## What it writes

- `packaging/winget/manifests/k/Kkthnx/Clarion/<version>/` holds the three winget files (version, installer, default locale), schema 1.12.0. The installer is the Inno Setup program, so winget supplies the silent switches itself. Check them with `winget validate --manifest <that folder>`. To publish, send them to the winget-pkgs repository (`wingetcreate submit`, or a pull request).
- `packaging/scoop/clarion.json` installs the portable zip. Put it in a `bucket` folder of a repository to make a Scoop bucket. Its version check reads the release list, because the "latest" address skips pre-releases, and every Clarion release so far is one.

## Before the first submission

1. Choose the licence.
2. Decide whether to sign the setup program (see SIGNING.md). Unsigned installers work, but SmartScreen warns about them, and the winget reviewers will see it.
3. Run the script for the release, validate, read the files.

The generated folder is not committed. It is rebuilt for each release from what the release says.
