using Autodesk.Revit.DB;
using ListAddin.Models;

namespace ListAddin.Services;

public static class LegendCollector
{
    public static (View Legend, Element Seed) FindSeed(Document doc)
    {
        foreach (var view in new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>()
                     .Where(v => v.ViewType == ViewType.Legend && !v.IsTemplate))
        {
            var seed = new FilteredElementCollector(doc, view.Id)
                .FirstOrDefault(e => e.get_Parameter(BuiltInParameter.LEGEND_COMPONENT) != null);
            if (seed != null) return (view, seed);
        }
        throw new InvalidOperationException("No seed Legend Component was found. Create one Legend view, place any Legend Component in it, then run the command again.");
    }

    public static IReadOnlyList<FamilyGroup> CollectCompatible(Document doc, Element seed)
    {
        var parameter = seed.get_Parameter(BuiltInParameter.LEGEND_COMPONENT)
                        ?? throw new InvalidOperationException("The seed has no Legend Component parameter.");
        var original = parameter.AsElementId();
        var compatible = new List<FamilySymbol>();
        foreach (var symbol in new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>()
                     .Where(s => s.Family != null && !s.Family.IsInPlace)
                     .OrderBy(s => s.FamilyName).ThenBy(s => s.Name))
        {
            using var sub = new SubTransaction(doc); sub.Start();
            try { if (parameter.Set(symbol.Id)) compatible.Add(symbol); }
            catch (Autodesk.Revit.Exceptions.ApplicationException) { }
            finally { sub.RollBack(); }
        }
        parameter.Set(original);
        return compatible.GroupBy(s => s.FamilyName, StringComparer.CurrentCultureIgnoreCase)
            .Select(g => new FamilyGroup(g.Key, g.Select(s => new FamilyTypeItem(s.Id, s.Name)).ToList()))
            .OrderBy(g => g.FamilyName, StringComparer.CurrentCultureIgnoreCase).ToList();
    }
}

