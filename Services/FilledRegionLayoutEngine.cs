using ListAddin.Models;

namespace ListAddin.Services;

public static class FilledRegionLayoutEngine
{
    public static IReadOnlyList<FilledRegionPlacement> Layout(IReadOnlyList<FilledRegionItem> items)
    {
        const double left = .40, top = .45, bottom = .40, columnWidth = 2.55, usableWidth = 7.70;
        const double headingHeight = .42, rowHeight = .31;
        var result = new List<FilledRegionPlacement>();
        var page = 0;
        var x = left;
        var y = 11 - top;
        var showHeading = true;

        foreach (var item in items)
        {
            if (showHeading) y -= headingHeight;
            if (y - rowHeight < bottom)
            {
                x += columnWidth;
                y = 11 - top - headingHeight;
                showHeading = true;
            }
            if (x + columnWidth > left + usableWidth)
            {
                page++;
                x = left;
                y = 11 - top - headingHeight;
                showHeading = true;
            }

            result.Add(new FilledRegionPlacement(item, page, x, y, showHeading));
            showHeading = false;
            y -= rowHeight;
        }

        return result;
    }
}
