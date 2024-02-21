using System.ComponentModel.DataAnnotations;

namespace Wave.Data;

public class ArticleImage {
	[Key]
	public Guid Id { get; set; }

	[MaxLength(2048)]
	public string ImageDescription { get; set; } = string.Empty;
}