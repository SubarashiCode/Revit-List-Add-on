using Autodesk.Revit.DB;

namespace ListAddin.Models;

public sealed record FilledRegionItem(ElementId TypeId, string TypeName);

public sealed record FilledRegionPlacement(FilledRegionItem Item, int Page, double X, double Y, bool ShowHeading);
