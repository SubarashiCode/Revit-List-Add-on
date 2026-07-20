using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ListAddin.Services;

namespace ListAddin.Commands;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class ListFilledRegionsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument?.Document;
        if (doc == null) { message = "Open a Revit project first."; return Result.Failed; }
        using var group = new TransactionGroup(doc, "List Filled Regions");
        group.Start();
        try
        {
            IReadOnlyList<View> pages;
            int typeCount;
            using (var tx = new Transaction(doc, "Create filled region legends"))
            {
                tx.Start();
                var styles = TextStyleManager.Ensure(doc);
                var source = LegendCollector.FindSourceLegend(doc);
                var types = LegendCollector.CollectFilledRegions(doc);
                if (types.Count == 0)
                    throw new InvalidOperationException("No Filled Region types exist in this project.");
                var placements = FilledRegionLayoutEngine.Layout(types);
                pages = FilledRegionViewBuilder.Build(doc, source, placements, styles.Regular, styles.Heading);
                typeCount = types.Count;
                tx.Commit();
            }
            group.Assimilate();
            TaskDialog.Show("List Filled Regions",
                $"Created {pages.Count} legend page(s) listing {typeCount:N0} filled region types.\n\nFirst view: {pages[0].Name}");
            commandData.Application.ActiveUIDocument.ActiveView = pages[0];
            return Result.Succeeded;
        }
        catch (OperationCanceledException) { group.RollBack(); return Result.Cancelled; }
        catch (Exception ex)
        {
            group.RollBack();
            message = ex.Message;
            TaskDialog.Show("List Filled Regions", ex.Message);
            return Result.Failed;
        }
    }
}
