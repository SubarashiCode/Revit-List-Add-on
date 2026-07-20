using Autodesk.Revit.DB;

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

        var fine = CreatePlan(doc, source, viewFamilyType.Id, level.Id,
            "Pipe Accessories - Fine (Double Line)", ViewDetailLevel.Fine);
        var single = CreatePlan(doc, source, viewFamilyType.Id, level.Id,
            "Pipe Accessories - Single Line", ViewDetailLevel.Coarse);
        return (fine, single);
    }

    private static ViewPlan CreatePlan(Document doc, ViewPlan source, ElementId viewFamilyTypeId,
        ElementId levelId, string baseName, ViewDetailLevel detailLevel)
    {
        var view = ViewPlan.Create(doc, viewFamilyTypeId, levelId);
        view.Name = NextName(doc, baseName);
        view.Scale = source.Scale;
        view.DetailLevel = detailLevel;
        view.SetViewRange(source.GetViewRange());

        if (source.CropBoxActive)
        {
            view.CropBox = source.CropBox;
            view.CropBoxActive = true;
            view.CropBoxVisible = source.CropBoxVisible;
        }

        ShowOnlyPipeAccessories(doc, view);
        return view;
    }

    private static void ShowOnlyPipeAccessories(Document doc, View view)
    {
        var pipeAccessories = Category.GetCategory(doc, BuiltInCategory.OST_PipeAccessory)
                              ?? throw new InvalidOperationException("The Pipe Accessories category is unavailable.");

        foreach (Category category in doc.Settings.Categories)
        {
            if (category.Id == pipeAccessories.Id || !category.get_AllowsVisibilityControl(view))
                continue;

            view.SetCategoryHidden(category.Id, true);
        }

        if (pipeAccessories.get_AllowsVisibilityControl(view))
            view.SetCategoryHidden(pipeAccessories.Id, false);
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
