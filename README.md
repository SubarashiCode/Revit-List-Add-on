# List Add-in for Revit 2025 and 2026

Creates a **List** ribbon tab, a **Documentation** panel, and a **List Project Symbols** command. The command builds alphabetized, paginated 8.5 x 11 Legend views containing loaded Generic Annotation symbols and their type names.

## Build and install

```powershell
.\build.ps1
.\install.ps1
```

Revit must be restarted after installation. Output is built separately against the installed Revit 2025 and 2026 APIs.

## Legend view requirement

The Revit API cannot create a project's first Legend view directly. Before the first run, the project must contain one Legend view. It may be completely empty: no Legend Component or seed symbol is required. The command duplicates that view and places loaded `OST_GenericAnnotation` family types, matching Revit's **Symbol** command rather than the Legend Component picker.

The source Legend is never modified. Generated pages are named `Project Symbol List 01`, `02`, and so on.

## Layout

- Scale: 1\" = 1'-0\"
- Border: 8.5 x 11 inches on paper, using `<Wide Lines>` when available
- Text: `List 3/32\" Arial` and `List 3/32\" Arial Bold Underline`
- Families and types: alphabetical
- Family blocks remain together where they fit, flow top-to-bottom into columns, then continue on new Legend views
