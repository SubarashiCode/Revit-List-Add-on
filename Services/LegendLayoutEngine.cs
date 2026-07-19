using ListAddin.Models;

namespace ListAddin.Services;

public sealed record Placement(FamilyGroup Group, int Page, double X, double Y);

public static class LegendLayoutEngine
{
    public static IReadOnlyList<Placement> Layout(IReadOnlyList<FamilyGroup> groups)
    {
        const double left=.40, top=.45, bottom=.40, columnWidth=2.55, usableWidth=7.70;
        const double heading=.28, row=.31, gap=.22;
        var result=new List<Placement>(); var page=0; var x=left; var y=11-top;
        foreach (var group in groups)
        {
            var height=heading + group.Types.Count*row + gap;
            if (y-height < bottom) { x += columnWidth; y=11-top; }
            if (x+columnWidth > left+usableWidth) { page++; x=left; y=11-top; }
            result.Add(new Placement(group,page,x,y)); y -= height;
        }
        return result;
    }
}

