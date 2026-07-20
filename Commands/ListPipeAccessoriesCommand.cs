using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ListAddin.Services;

namespace ListAddin.Commands;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class ListPipeAccessoriesCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc?.Document;
        if (doc == null)
        {
            message = "Open a Revit project first.";
            return Result.Failed;
        }

        if (doc.IsFamilyDocument)
        {
            TaskDialog.Show("List Pipe Accessories", "This command is available only in a Revit project.");
            return Result.Cancelled;
        }

        if (uiDoc!.ActiveView is not ViewPlan source || source.ViewType != ViewType.FloorPlan || source.IsTemplate)
        {
            TaskDialog.Show("List Pipe Accessories",
                "Open the floor plan whose level, view range, scale, and crop extents should be used, then run the command again.");
            return Result.Cancelled;
        }

        using var group = new TransactionGroup(doc, "List Pipe Accessories");
        group.Start();
        try
        {
            ViewPlan fineView;
            ViewPlan singleLineView;
            using (var tx = new Transaction(doc, "Create pipe accessory floor plans"))
            {
                tx.Start();
                (fineView, singleLineView) = PipeAccessoryPlanBuilder.Build(doc, source);
                tx.Commit();
            }

            group.Assimilate();
            uiDoc.ActiveView = fineView;
            TaskDialog.Show("List Pipe Accessories",
                $"Created two floor-plan catalogs of every loaded Pipe Accessory type:\n\n" +
                $"Fine (double line): {fineView.Name}\n" +
                $"Coarse (single line): {singleLineView.Name}");
            return Result.Succeeded;
        }
        catch (OperationCanceledException)
        {
            group.RollBack();
            return Result.Cancelled;
        }
        catch (Exception ex)
        {
            group.RollBack();
            message = ex.Message;
            TaskDialog.Show("List Pipe Accessories", ex.Message);
            return Result.Failed;
        }
    }
}
