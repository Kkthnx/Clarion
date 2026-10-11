# Versioning

.Clarion uses Semantic Versioning, `MAJOR.MINOR.PATCH`. A MAJOR bump is a change that breaks how people use it or the files it saves. MINOR adds features and also marks a deprecation. PATCH fixes bugs. Bumping one number resets the ones after it.

While the version starts with 0, .Clarion is in initial development and nothing is promised to stay the same. During that time a breaking change to saved data or to the command line bumps MINOR (0.1.0 to 0.2.0), and features and fixes stay inside the current MINOR as pre-releases. 1.0.0 ships when the downloads are code signed and the beta reports have settled.

Pre-releases are `alpha`, `beta` or `rc`, in that order, numbered from 1 (`0.1.0-beta.10`). Alpha means features may still move, beta means wider testing is under way and the way people use it should stay put, so what is saved and the command line only change with a note, while settings, pages and repair jobs can still be added, and rc means it ships unless a blocker turns up. Numbering restarts when the label changes, and the final release drops the suffix. The numbers are compared as numbers, so beta.10 comes after beta.9.

A released version is never changed. If a release is wrong, the fix is the next number, and a tag is not moved after it has been pushed. The version is written in one place, `Directory.Build.props`, and the installer, the zip and the packaging scripts read it from there. Each release is tagged `vX.Y.Z` in git, and its notes are that version's section of the changelog.
