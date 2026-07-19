using Autodesk.Revit.DB;
using ListAddin.Models;

namespace ListAddin.Services;

public static class LegendViewBuilder
{
    public static IReadOnlyList<View> Build(Document doc, View source, Element sourceSeed,
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
            foreach (var p in placements.Where(x => x.Page == page)) PlaceGroup(doc, view, sourceSeed, p, regular, heading);
        }
        return pages;
    }

    private static void PlaceGroup(Document doc, View view, Element sourceSeed, Placement p, TextNoteType regular, TextNoteType heading)
    {
        TextNote.Create(doc, view.Id, new XYZ(p.X,p.Y,0), p.Group.FamilyName, heading.Id);
        var y=p.Y-.28;
        foreach (var type in p.Group.Types)
        {
            var sourceView=(View)doc.GetElement(sourceSeed.OwnerViewId);
            var ids=ElementTransformUtils.CopyElements(sourceView, new[] { sourceSeed.Id }, view,
                Transform.CreateTranslation(new XYZ(p.X+.12,y,0) - Location(sourceSeed)), new CopyPasteOptions());
            var component=doc.GetElement(ids.First());
            component.get_Parameter(BuiltInParameter.LEGEND_COMPONENT)?.Set(type.TypeId);
            TextNote.Create(doc, view.Id, new XYZ(p.X+.70,y,0), type.TypeName, regular.Id);
            y-=.31;
        }
    }

    private static XYZ Location(Element element) => element.Location is LocationPoint lp ? lp.Point :
        (element.get_BoundingBox(element.Document.GetElement(element.OwnerViewId) as View)?.Min ?? XYZ.Zero);

    private static string NextName(Document doc)
    {
        var names=new HashSet<string>(new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().Select(v=>v.Name));
        for(var i=1;;i++){var name=$"Project Symbol List {i:00}";if(!names.Contains(name))return name;}
    }
}
