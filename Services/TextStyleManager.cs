using Autodesk.Revit.DB;

namespace ListAddin.Services;

public static class TextStyleManager
{
    public static (TextNoteType Regular, TextNoteType Heading) Ensure(Document doc)
    {
        var types = new FilteredElementCollector(doc).OfClass(typeof(TextNoteType)).Cast<TextNoteType>().ToList();
        if (types.Count == 0) throw new InvalidOperationException("The project contains no Text Note Type to duplicate.");
        return (GetOrCreate(doc, types, "List 3/32\" Arial", false, false),
                GetOrCreate(doc, types, "List 3/32\" Arial Bold Underline", true, true));
    }

    private static TextNoteType GetOrCreate(Document doc, List<TextNoteType> types, string name, bool bold, bool underline)
    {
        var type = types.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                   ?? (TextNoteType)types[0].Duplicate(name);
        Set(type, BuiltInParameter.TEXT_STYLE_FONT, "Arial");
        Set(type, BuiltInParameter.TEXT_STYLE_SIZE, UnitUtils.ConvertToInternalUnits(3.0 / 32.0, UnitTypeId.Inches));
        Set(type, BuiltInParameter.TEXT_STYLE_BOLD, bold ? 1 : 0);
        Set(type, BuiltInParameter.TEXT_STYLE_ITALIC, 0);
        Set(type, BuiltInParameter.TEXT_STYLE_UNDERLINE, underline ? 1 : 0);
        return type;
    }

    private static void Set(Element e, BuiltInParameter id, string value) { var p=e.get_Parameter(id); if (p is { IsReadOnly:false }) p.Set(value); }
    private static void Set(Element e, BuiltInParameter id, double value) { var p=e.get_Parameter(id); if (p is { IsReadOnly:false }) p.Set(value); }
    private static void Set(Element e, BuiltInParameter id, int value) { var p=e.get_Parameter(id); if (p is { IsReadOnly:false }) p.Set(value); }
}
