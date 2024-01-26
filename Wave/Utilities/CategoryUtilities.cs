using Wave.Data;

namespace Wave.Utilities;

public static class CategoryUtilities {
    public static string GetCssClassPostfixForColor(CategoryColors color) {
        return color switch {
            CategoryColors.Primary => "primary",
            CategoryColors.Dangerous => "error",
            CategoryColors.Important => "warning",
            CategoryColors.Informative => "info",
            CategoryColors.Secondary => "secondary",
            CategoryColors.Default => "",
            CategoryColors.Extra => "accent",
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };
    }
}