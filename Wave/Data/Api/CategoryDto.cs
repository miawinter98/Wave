using Wave.Utilities;

namespace Wave.Data.Api;

public record CategoryDto(string Name, string Role) {

	public CategoryDto(Category category) 
		: this(category.Name, CategoryUtilities.GetCssClassPostfixForColor(category.Color)) {}
}