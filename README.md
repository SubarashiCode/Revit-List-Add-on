# List Add-in for Revit 2025 and 2026

Creates a **List** ribbon tab and a **Documentation** panel with three commands:

- **List Project Symbols** builds alphabetized Legend pages containing loaded Generic Annotation symbols and their type names.
- **List Filled Regions** builds alphabetized Legend pages containing a sample of every Filled Region type and its type name.
- **List Pipe Accessories** places and labels one sample of every loaded Pipe Accessory family type in two floor-plan catalogs: a Fine (double-line) view and a Coarse (single-line) view. Both hide unrelated model categories.

The symbol and filled-region commands use the same paginated 8.5 x 11 system.

## Build and install

```powershell
.\build.ps1
.\install.ps1
```

Revit must be restarted after installation. Output is built separately against the installed Revit 2025 and 2026 APIs.

## Legend view requirement

The Revit API cannot create a project's first Legend view directly. Before the first run, the project must contain one Legend view. It may be completely empty: no Legend Component or seed symbol is required. The command duplicates that view and places loaded `OST_GenericAnnotation` family types, matching Revit's **Symbol** command rather than the Legend Component picker.

The source Legend is never modified. Symbol pages are named `Project Symbol List 01`, `02`, and so on; Filled Region pages are named `Filled Region List 01`, `02`, and so on.

## Layout

- Scale: 1\" = 1'-0\"
- Border: 8.5 x 11 inches on paper, using `<Wide Lines>` when available
- Text: `List 3/32\" Arial` and `List 3/32\" Arial Bold Underline`
- Families and types: alphabetical
- Family blocks remain together where they fit, flow top-to-bottom into columns, then continue on new Legend views
