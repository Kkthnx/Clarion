# .Clarion Brand Identity

## Name candidates

| Name | Slogan | Why it works |
|---|---|---|
| **.Clarion** (chosen) | See it. Set it. Done. | A clarion call is clear and carries far. Says clarity first, speed second. |
| Kestrel | Sharp eyes on every setting. | A hovering raptor. Precision and control. |
| Lucent | Windows, made clear. | Means shining through. Fits the explain everything goal. |
| Tessera | Every piece in its place. | One tile of a mosaic. Fits a catalog of small, exact changes. |
| Ballast | Drop the weight. | Direct and memorable. Leans on the debloat side only. |

## The name

The name is **.Clarion**, with the leading dot, in the way .NET is. It is written that way in the app, the readme, the installer and the release titles, and in sentences (".Clarion keeps a record of every change").

The dot stays out of anything a computer reads, because a name that starts with a dot is hidden or refused in many places. So these keep the plain spelling **Clarion**: the program file Clarion.exe, the install and data folders, the code namespaces, the scheduled task folder, the winget package id Kkthnx.Clarion, the setup file marker, the release file names, and the GitHub repository name. A person who types the name into a search box can use either spelling.

Before this name, the project was called Clarion, and old releases and their notes still say so. The earlier name was chosen from the candidates below.

## Brand files

The mark is a blue dot followed by a C, so the full name reads as `.Clarion`. The wordmark is set in Space Grotesk Bold and the tagline in Inter Medium, both under the SIL Open Font License, so they are free to use. Brand blue is `#5C8BCF` on the dark background `#0F141B`, the same values as the app's theme.

The files are in `docs/brand`. Use the light versions on dark backgrounds and the dark versions on light ones.

| File | What it is |
|---|---|
| `wordmark-light-2048.png`, `wordmark-dark-2048.png` | The `.Clarion` wordmark on a transparent background. Actually 782 by 263 pixels. |
| `c-mark-light-1024.png`, `c-mark-dark-1024.png` | The `.C` monogram on a transparent background. Actually 486 by 518 pixels. |
| `app-icon-1024.png` | The app icon tile, a rounded square on a transparent background. |
| `avatar-1024.png` | The square avatar for GitHub and social profiles. |
| `banner-1280x640.png` | The banner, which is also the right size for the GitHub social preview. |

What the app and the repository use from this set: `docs/images/banner.png` (the banner), and three files that `scripts/make-icon.ps1` builds from `app-icon-1024.png`: `src/Clarion.App/Assets/Clarion.ico` (sizes 16, 20, 24, 32, 40, 48, 64, 128 and 256), `src/Clarion.App/Assets/Clarion.png` and `docs/images/logo.png` (the app icon at 512 pixels). The icon is rebuilt, and not copied from the pack, because the pack's own icon stores every size as a PNG and has no 20 or 40 pixel size. The script writes sizes under 256 as classic bitmaps, which every Windows tool reads, as the earlier icon did. The GitHub social preview and the profile avatar are set by hand in the repository and account settings, since GitHub has no way to set them from code.

The old `scripts/make-brand-assets.ps1` drew the previous mark and was removed so it cannot overwrite these files. Run `scripts/make-icon.ps1` again if `app-icon-1024.png` changes.

## Voice

Plain, short, honest. Say what a change does, what it gives, and what it risks. No hype, no scare words, no jargon without a one line explanation.

## Dark palette (default)

| Role | Token | HEX |
|---|---|---|
| App background | `bg.base` | `#0F141B` |
| Sidebar | `bg.sidebar` | `#121820` |
| Surface (cards) | `surface.1` | `#161D27` |
| Raised surface (dialogs, flyouts) | `surface.2` | `#1C2430` |
| Hover surface | `surface.3` | `#243040` |
| Border | `border.default` | `#2B3646` |
| Divider | `border.subtle` | `#1F2833` |
| Primary text | `text.primary` | `#E6EBF2` |
| Secondary text (silver) | `text.secondary` | `#A7B3C4` |
| Muted text | `text.muted` | `#7A8799` |
| Disabled | `text.disabled` | `#566173` |
| Accent (steel blue) | `accent.base` | `#5C8BCF` |
| Accent hover | `accent.hover` | `#7AA3DD` |
| Accent pressed | `accent.pressed` | `#4673B3` |
| Accent subtle fill | `accent.tint` | `#1B2A42` |
| Text on accent | `accent.onfill` | `#0B1016` |
| Cyan (active, live, info) | `cyan.base` | `#3FD0E0` |
| Cyan dim | `cyan.dim` | `#1E8FA0` |
| Cyan tint | `cyan.tint` | `#102B31` |
| Success (applied, safe) | `ok` | `#4CC38A` |
| Caution | `warn` | `#E3B341` |
| Danger (high risk) | `danger` | `#E5534B` |
| Focus ring | `focus` | `#3FD0E0` |

Use dark text on accent fills. White on `#5C8BCF` falls below 4.5:1, dark text on it clears 5:1.

## Light palette

| Role | HEX |
|---|---|
| Background | `#F3F5F8` |
| Surface | `#FFFFFF` |
| Border | `#D3DAE4` |
| Primary text | `#131A24` |
| Secondary text | `#4A5668` |
| Accent | `#3F6DB5` |
| Cyan | `#0E8797` |
| Success / Caution / Danger | `#1F8F5F` / `#9A6B00` / `#C4372F` |

## Usage rules

1. Cyan means live state only. An applied tweak, a running task, a current value.
2. Steel blue means action. Primary buttons, selection, links.
3. Green, amber and red appear only in risk and result badges, never as decoration.
4. Flat surfaces, 1px borders, 8px corner radius, no gradients, no heavy shadows.
5. Motion under 200ms, and off when the system asks for reduced motion.
6. Every state has a text or icon cue so color is never the only signal.

## Type

Segoe UI Variable for text, Cascadia Mono for registry paths and commands.
