using Autodesk.Revit.DB;
using ListAddin.Models;

namespace ListAddin.Services;

public static class LegendCollector
{
    public static View FindSourceLegend(Document doc)
    {
        return new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>()
                   .FirstOrDefault(v => v.ViewType == ViewType.Legend && !v.IsTemplate)
               ?? throw new InvalidOperationException(
                   "No Legend view exists in this project. Revit's public API cannot create the first Legend view. Create one empty Legend view, then run the command again; no symbol or seed component is required.");
    }

    public static IReadOnlyList<FamilyGroup> CollectProjectSymbols(Document doc)
    {
        var genericAnnotationId = new ElementId(BuiltInCategory.OST_GenericAnnotation);
        return new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>()
            .Where(s => s.Family != null && !s.Family.IsInPlace && s.Category?.Id == genericAnnotationId)
            .GroupBy(s => s.FamilyName, StringComparer.CurrentCultureIgnoreCase)
            .Select(g => new FamilyGroup(g.Key, g.Select(s => new FamilyTypeItem(s.Id, s.Name)).ToList()))
            .OrderBy(g => g.FamilyName, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    public static IReadOnlyList<FilledRegionItem> CollectFilledRegions(Document doc)
    {
        return new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).Cast<FilledRegionType>()
            .Select(type => new FilledRegionItem(type.Id, type.Name))
            .OrderBy(type => type.TypeName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
