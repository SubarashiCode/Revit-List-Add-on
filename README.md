# List Add-in for Revit 2025 and 2026

Creates a **List** ribbon tab, a **Documentation** panel, and a **List Project Symbols** command. The command builds alphabetized, paginated 8.5 x 11 Legend views containing compatible Legend Components and their type names.

## Build and install

```powershell
.\build.ps1
.\install.ps1
```

Revit must be restarted after installation. Output is built separately against the installed Revit 2025 and 2026 APIs.

## Required project seed

The Revit API cannot create a project's first Legend view or first Legend Component directly. Before the first run, create one Legend view and place any Legend Component in it. The command finds that seed automatically, duplicates its Legend, and validates every `FamilySymbol` by attempting to assign it to the seed's `LEGEND_COMPONENT` parameter. This mirrors Revit compatibility rather than relying on a hard-coded category list.

The seed view is never modified. Generated pages are named `Project Symbol List 01`, `02`, and so on.

## Layout

- Scale: 1\" = 1'-0\"
- Border: 8.5 x 11 inches on paper, using `<Wide Lines>` when available
- Text: `List 3/32\" Arial` and `List 3/32\" Arial Bold Underline`
- Families and types: alphabetical
- Family blocks remain together where they fit, flow top-to-bottom into columns, then continue on new Legend views

