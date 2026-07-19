using Autodesk.Revit.DB;
using ListAddin.Models;

namespace ListAddin.Services;

public static class LegendViewBuilder
{
    public static IReadOnlyList<View> Build(Document doc, View source,
        IReadOnlyList<Placement> placements, TextNoteType regular, TextNoteType heading)
    {
        var pageCount = placements.Count == 0 ? 0 : placements.Max(x => x.Page)+1;
        var pages = new List<View>();
        for (var page=0; page<pageCount; page++)
        {
            var view=(View)doc.GetElement(source.Duplicate(ViewDuplicateOption.Duplicate));
            view.Name=NextName(doc); view.Scale=12;
            BorderBuilder.Draw(doc, view, 8.5, 11);
            pages.Add(view);
            foreach (var p in placements.Where(x => x.Page == page)) PlaceGroup(doc, view, p, regular, heading);
        }
        return pages;
    }

    private static void PlaceGroup(Document doc, View view, Placement p, TextNoteType regular, TextNoteType heading)
    {
        TextNote.Create(doc, view.Id, new XYZ(p.X,p.Y,0), p.Group.FamilyName, heading.Id);
        var y=p.Y-.28;
        foreach (var type in p.Group.Types)
        {
            var symbol = (FamilySymbol)doc.GetElement(type.TypeId);
            if (!symbol.IsActive) symbol.Activate();
            doc.Create.NewFamilyInstance(new XYZ(p.X+.12,y,0), symbol, view);
            TextNote.Create(doc, view.Id, new XYZ(p.X+.70,y,0), type.TypeName, regular.Id);
            y-=.31;
        }
    }

    private static string NextName(Document doc)
    {
        var names=new HashSet<string>(new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().Select(v=>v.Name));
        for(var i=1;;i++){var name=$"Project Symbol List {i:00}";if(!names.Contains(name))return name;}
    }
}
