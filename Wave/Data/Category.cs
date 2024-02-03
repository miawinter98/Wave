using System.ComponentModel.DataAnnotations;

namespace Wave.Data;
// We want to order categories based on their Color/Role, 
// so more  important ones can be displayed first
public enum CategoryColors {
    Primary = 1, 
    Dangerous = 5, 
    Important = 10, 
    Informative = 15, 
    Secondary = 20,
    Default = 25, 
    Extra = 50, 
}

public class Category {
    public Guid Id { get; set; }
    
    [MaxLength(128)]
    public required string Name { get; set; }
    public CategoryColors Color { get; set; } = CategoryColors.Default;

	public IList<Article> Articles { get; set; } = [];
}