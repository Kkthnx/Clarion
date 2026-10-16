---
name: Setting request
about: Ask for a setting, a repair job or an app to be added
labels: enhancement
---

A setting goes in only when it can be explained, reverted and sourced (see CONTRIBUTING.md). The more of this you can fill in, the faster it can be checked. "I do not know" is a fine answer.

**What it changes**
The exact thing, for example a registry value with its full path, a Group Policy name, a service, a scheduled task, a Windows feature, or a winget package ID. For an app, the ID from `winget search`.

**What it does**
What you see change when it is on. Say if you saw it yourself or read it somewhere.

**The source**
A link to Microsoft's documentation or the vendor's. If there is none, say so. A setting with no source is graded Unproven and only shown in Expert mode.

**What could go wrong**
Anything that stops working, any edition or build where it does nothing, and whether it needs a restart or signing out.

**How to put it back**
What the value was before, or the command that undoes it. .Clarion records the real previous value when the setting is applied, so this is only needed if undoing it is not a simple switch.

**Windows version**
Edition and build, from Settings, System, About. Say which ones you tried it on.
