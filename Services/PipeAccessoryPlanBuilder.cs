using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace ListAddin.Services;

public static class PipeAccessoryPlanBuilder
{
    public static (ViewPlan FineView, ViewPlan SingleLineView) Build(Document doc, ViewPlan source)
    {
        var level = source.GenLevel
                    ?? throw new InvalidOperationException("The active floor plan is not associated with a level.");
        var viewFamilyType = doc.GetElement(source.GetTypeId()) as ViewFamilyType;
        if (viewFamilyType == null || viewFamilyType.ViewFamily != ViewFamily.FloorPlan)
            throw new InvalidOperationException("The active view does not use a valid floor plan view type.");

        var symbols = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .OfCategory(BuiltInCategory.OST_PipeAccessory)
            .Cast<FamilySymbol>()
            .OrderBy(symbol => symbol.FamilyName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(symbol => symbol.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        if (symbols.Count == 0)
            throw new InvalidOperationException("No loaded Pipe Accessory family types were found in this project.");

        var fine = CreatePlan(doc, source, viewFamilyType.Id, level.Id,
            "Pipe Accessories - Fine (Double Line)", ViewDetailLevel.Fine, symbols, level, true);
        var single = CreatePlan(doc, source, viewFamilyType.Id, level.Id,
            "Pipe Accessories - Single Line", ViewDetailLevel.Coarse, symbols, level, false);
        return (fine, single);
    }

    private static ViewPlan CreatePlan(Document doc, ViewPlan source, ElementId viewFamilyTypeId,
        ElementId levelId, string baseName, ViewDetailLevel detailLevel,
        IReadOnlyList<FamilySymbol> symbols, Level level, bool placeSamples)
    {
        var view = ViewPlan.Create(doc, viewFamilyTypeId, levelId);
        view.Name = NextName(doc, baseName);
        view.Scale = source.Scale;
        view.DetailLevel = detailLevel;
        view.SetViewRange(source.GetViewRange());

        ShowOnlyPipeAccessories(doc, view);
        PlaceCatalog(doc, view, level, symbols, placeSamples);
        return view;
    }

    private static void PlaceCatalog(Document doc, ViewPlan view, Level level,
        IReadOnlyList<FamilySymbol> symbols, bool placeSamples)
    {
        const int columnCount = 4;
        const double columnSpacing = 8.0;
        const double rowSpacing = 5.0;
        const double labelOffset = 1.25;

        for (var index = 0; index < symbols.Count; index++)
        {
            var symbol = symbols[index];
            if (!symbol.IsActive)
            {
                symbol.Activate();
                doc.Regenerate();
            }

            var column = index % columnCount;
            var row = index / columnCount;
            var point = new XYZ(column * columnSpacing, -row * rowSpacing, level.Elevation);
            if (placeSamples)
                PlaceSample(doc, view, level, symbol, point);
            var label = string.Format("{0}\n{1}", symbol.FamilyName, symbol.Name);
            TextNote.Create(doc, view.Id, point + new XYZ(0, -labelOffset, 0), label,
                doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType));
        }

        var rowCount = (int)Math.Ceiling(symbols.Count / (double)columnCount);
        var crop = new BoundingBoxXYZ
        {
            Min = new XYZ(-3.0, -(rowCount - 1) * rowSpacing - 3.5, level.Elevation - 10.0),
            Max = new XYZ((columnCount - 1) * columnSpacing + 5.0, 3.0, level.Elevation + 10.0)
        };
        view.CropBox = crop;
        view.CropBoxActive = true;
        view.CropBoxVisible = false;
    }

    private static void PlaceSample(Document doc, ViewPlan view, Level level,
        FamilySymbol symbol, XYZ point)
    {
        switch (symbol.Family.FamilyPlacementType)
        {
            case FamilyPlacementType.OneLevelBased:
            case FamilyPlacementType.TwoLevelsBased:
                doc.Create.NewFamilyInstance(point, symbol, level, StructuralType.NonStructural);
                break;
            case FamilyPlacementType.ViewBased:
                doc.Create.NewFamilyInstance(point, symbol, view);
                break;
            default:
                throw new InvalidOperationException(string.Format(
                    "Pipe Accessory type '{0}: {1}' uses unsupported placement type '{2}'.",
                    symbol.FamilyName, symbol.Name, symbol.Family.FamilyPlacementType));
        }
    }

    private static void ShowOnlyPipeAccessories(Document doc, View view)
    {
        var pipeAccessories = Category.GetCategory(doc, BuiltInCategory.OST_PipeAccessory)
                              ?? throw new InvalidOperationException("The Pipe Accessories category is unavailable.");
        var textNotes = Category.GetCategory(doc, BuiltInCategory.OST_TextNotes);

        foreach (Category category in doc.Settings.Categories)
        {
            if (category.Id == pipeAccessories.Id || category.Id == textNotes?.Id ||
                !category.get_AllowsVisibilityControl(view))
                continue;

            view.SetCategoryHidden(category.Id, true);
        }

        if (pipeAccessories.get_AllowsVisibilityControl(view))
            view.SetCategoryHidden(pipeAccessories.Id, false);
        if (textNotes != null && textNotes.get_AllowsVisibilityControl(view))
            view.SetCategoryHidden(textNotes.Id, false);
    }

    private static string NextName(Document doc, string baseName)
    {
        var names = new HashSet<string>(new FilteredElementCollector(doc)
            .OfClass(typeof(View)).Cast<View>().Select(view => view.Name));
        if (!names.Contains(baseName)) return baseName;
        for (var i = 2;; i++)
        {
            var candidate = $"{baseName} {i:00}";
            if (!names.Contains(candidate)) return candidate;
        }
    }
}
