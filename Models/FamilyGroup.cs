using Autodesk.Revit.DB;

namespace ListAddin.Models;

public sealed record FamilyTypeItem(ElementId TypeId, string TypeName);
public sealed record FamilyGroup(string FamilyName, IReadOnlyList<FamilyTypeItem> Types);

