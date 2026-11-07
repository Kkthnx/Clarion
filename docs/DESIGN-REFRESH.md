# Design refresh, November 2026

A short brief for the visual pass after 1.4.0. It covers Home and the settings rows. The rest of the app keeps its look.

## Design thesis

A careful workbench for a PC you care about. Calm by default, loud only when something needs you, and nothing changes until you have read what it will do.

## Who and what

Three groups use it: someone who wants a quieter PC without breaking it, a gamer who tunes a desktop or a gaming laptop, and a power user who wants the exact registry value behind every switch.

Core tasks, most common first:
1. Pick a preset or a handful of settings, read them, apply.
2. See whether earlier changes still hold after a Windows update.
3. Free disk space safely.
4. Find out what broke something and undo it.
5. Tune for games, on a laptop without hurting it.

## What was wrong (seen on screen, 1400 by 900)

- Home opened on a large logo and a stack of full width boxes, with the presets below the first screen. The task people come for was not visible.
- The six hardware tiles filled the first screen and are reference, not action.
- Every row said "Suggested: turn on" and "Safe", also on settings already on. On Privacy, every row carried two labels, and most told nothing new.

## Three directions considered

| | Idea | Fit |
|---|---|---|
| A. Convention first | Keep the layout, tighten spacing | Safe, but leaves the first screen as it was |
| B. Work first | Home says what needs you, then lists the jobs, then presets, then facts about the PC. Rows say only what is not already obvious | Fits tasks 1 to 4, small change, no new framework |
| C. Cockpit | A health ring and live gauges | Would need numbers .Clarion does not measure, so it would be decoration |

Chosen: B.

## What changed

- Home: a 48 pixel header, then one group "N things to look at" holding every notice, then a Jobs list (check settings, free up space, tune for games, something stopped working), then presets, then This PC. The jobs are one list inside one container, not four cards.
- Rows: the Suggested label is hidden once a setting is on and while it is queued. The Safe label is hidden on rows (Low, Medium and High stay). Both are still in the details under the i. Same for Clean up and Repair.
- Color keeps one meaning: green low risk, amber caution, red high risk, blue action.

## Checked

Rendered and looked at: Home at 1400 by 900 and 820 by 640, Privacy, Clean up, Repair. Not checked: light theme, high contrast, 150 percent scaling, Narrator, a second monitor.

## References

Not done as a separate step this round. The decisions come from the screens above and from the earlier design notes in DESIGN.md. Microsoft's Windows app design pages and at least one product outside the category should be read before any larger redesign.

## Non goals

New navigation, new icons, a new palette, animation.
