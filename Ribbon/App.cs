using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media;

namespace ListAddin.Ribbon;

public sealed class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        const string tab = "List";
        try { application.CreateRibbonTab(tab); } catch (Autodesk.Revit.Exceptions.ArgumentException) { }
        var panel = application.GetRibbonPanels(tab).FirstOrDefault(p => p.Name == "Documentation")
                    ?? application.CreateRibbonPanel(tab, "Documentation");
        var data = new PushButtonData("ListProjectSymbols", "List Project\nSymbols",
            Assembly.GetExecutingAssembly().Location, "ListAddin.Commands.ListProjectSymbolsCommand")
        {
            ToolTip = "Create paginated legend views listing every compatible family symbol.",
            LongDescription = "Lists loaded Generic Annotation family types from Revit's Symbol command, grouped alphabetically and laid out on 8.5 x 11 legend pages."
        };
        var button = (PushButton)panel.AddItem(data);
        button.Image = IconFactory.Create(16, "S");
        button.LargeImage = IconFactory.Create(32, "S");

        var filledRegionData = new PushButtonData("ListFilledRegions", "List Filled\nRegions",
            Assembly.GetExecutingAssembly().Location, "ListAddin.Commands.ListFilledRegionsCommand")
        {
            ToolTip = "Create paginated legend views listing every filled region type.",
            LongDescription = "Lists every Filled Region type in the project alphabetically, with a sample swatch, on 8.5 x 11 legend pages."
        };
        var filledRegionButton = (PushButton)panel.AddItem(filledRegionData);
        filledRegionButton.Image = IconFactory.Create(16, "F");
        filledRegionButton.LargeImage = IconFactory.Create(32, "F");
        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
}

internal static class IconFactory
{
    public static ImageSource Create(double size, string label)
    {
        var group = new DrawingGroup();
        using (var dc = group.Open())
        {
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(31, 78, 121)), null,
                new System.Windows.Rect(0, 0, size, size), size * .16, size * .16);
            var text = new FormattedText(label, System.Globalization.CultureInfo.InvariantCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Segoe UI Semibold"), size * .68,
                Brushes.White, 1.0);
            dc.DrawText(text, new System.Windows.Point((size - text.Width) / 2, (size - text.Height) / 2));
        }
        group.Freeze();
        var image = new DrawingImage(group); image.Freeze(); return image;
    }
}
