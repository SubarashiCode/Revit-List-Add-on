using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ListAddin.Services;

namespace ListAddin.Commands;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class ListProjectSymbolsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc=commandData.Application.ActiveUIDocument?.Document;
        if(doc==null){message="Open a Revit project first.";return Result.Failed;}
        using var group=new TransactionGroup(doc,"List Project Symbols"); group.Start();
        try
        {
            IReadOnlyList<Autodesk.Revit.DB.View> pages;
            int familyCount, typeCount;
            using(var tx=new Transaction(doc,"Create project symbol legends"))
            {
                tx.Start();
                var styles=TextStyleManager.Ensure(doc);
                var (source,seed)=LegendCollector.FindSeed(doc);
                var families=LegendCollector.CollectCompatible(doc,seed);
                if(families.Count==0) throw new InvalidOperationException("No family types accepted by the seed Legend Component were found.");
                var placements=LegendLayoutEngine.Layout(families);
                pages=LegendViewBuilder.Build(doc,source,seed,placements,styles.Regular,styles.Heading);
                familyCount=families.Count; typeCount=families.Sum(f=>f.Types.Count);
                tx.Commit();
            }
            group.Assimilate();
            TaskDialog.Show("List Project Symbols",$"Created {pages.Count} legend page(s) listing {typeCount:N0} types in {familyCount:N0} families.\n\nFirst view: {pages[0].Name}");
            commandData.Application.ActiveUIDocument.ActiveView=pages[0];
            return Result.Succeeded;
        }
        catch(OperationCanceledException){group.RollBack();return Result.Cancelled;}
        catch(Exception ex){group.RollBack();message=ex.Message;TaskDialog.Show("List Project Symbols",ex.Message);return Result.Failed;}
    }
}

