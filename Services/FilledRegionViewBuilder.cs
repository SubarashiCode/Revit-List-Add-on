using Autodesk.Revit.DB;
using ListAddin.Models;

namespace ListAddin.Services;

public static class FilledRegionViewBuilder
{
    public static IReadOnlyList<View> Build(Document doc, View source,
        IReadOnlyList<FilledRegionPlacement> placements, TextNoteType regular, TextNoteType heading)
    {
        var pageCount = placements.Count == 0 ? 0 : placements.Max(x => x.Page) + 1;
        var pages = new List<View>();
        for (var page = 0; page < pageCount; page++)
        {
            var view = (View)doc.GetElement(source.Duplicate(ViewDuplicateOption.Duplicate));
            view.Name = NextName(doc);
            view.Scale = 12;
            BorderBuilder.Draw(doc, view, 8.5, 11);
            pages.Add(view);
            foreach (var placement in placements.Where(x => x.Page == page))
                PlaceItem(doc, view, placement, regular, heading);
        }
        return pages;
    }

    private static void PlaceItem(Document doc, View view, FilledRegionPlacement placement,
        TextNoteType regular, TextNoteType heading)
    {
        if (placement.ShowHeading)
            TextNote.Create(doc, view.Id, new XYZ(placement.X, placement.Y + .28, 0), "Filled Regions", heading.Id);

        const double width = .48, height = .18;
        var x = placement.X + .06;
        var y = placement.Y;
        var loop = new CurveLoop();
        var corners = new[]
        {
            new XYZ(x, y, 0), new XYZ(x + width, y, 0),
            new XYZ(x + width, y + height, 0), new XYZ(x, y + height, 0)
        };
        for (var i = 0; i < corners.Length; i++)
            loop.Append(Line.CreateBound(corners[i], corners[(i + 1) % corners.Length]));

        FilledRegion.Create(doc, placement.Item.TypeId, view.Id, new List<CurveLoop> { loop });
        TextNote.Create(doc, view.Id, new XYZ(placement.X + .70, y, 0), placement.Item.TypeName, regular.Id);
    }

    private static string NextName(Document doc)
    {
        var names = new HashSet<string>(new FilteredElementCollector(doc).OfClass(typeof(View))
            .Cast<View>().Select(view => view.Name));
        for (var i = 1;; i++)
        {
            var name = $"Filled Region List {i:00}";
            if (!names.Contains(name)) return name;
        }
    }
}
