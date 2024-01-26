namespace Wave.Data;

public class ArticleCategory {
    public int Id { get; set; }
    public required Article Article { get; set; }
    public required Category Category { get; set; }
}