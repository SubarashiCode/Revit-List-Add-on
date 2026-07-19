using Autodesk.Revit.DB;

namespace ListAddin.Services;

public static class BorderBuilder
{
    public static void Draw(Document doc, View view, double width, double height)
    {
        var lineCategory = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
        var wideCategory = lineCategory.SubCategories.Cast<Category>()
            .FirstOrDefault(x => x.Name == "<Wide Lines>");
        var wide = wideCategory?.GetGraphicsStyle(GraphicsStyleType.Projection);
        var points = new[] { new XYZ(0,0,0), new XYZ(width,0,0), new XYZ(width,height,0), new XYZ(0,height,0) };
        for (var i=0;i<4;i++)
        {
            var curve = doc.Create.NewDetailCurve(view, Line.CreateBound(points[i], points[(i+1)%4]));
            if (wide != null) curve.LineStyle = wide;
        }
    }
}
