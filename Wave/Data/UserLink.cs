using System.ComponentModel.DataAnnotations;

namespace Wave.Data;

public class UserLink {
	[Key]
	public int Id { get; set; }

	[MaxLength(1024)] 
	public string UrlString { get; set; } = string.Empty;
	public Uri Url => new(UrlString, UriKind.Absolute);

	public bool Validate() {
		try {
			_ = Url;
			return true;
		} catch {
			return false;
		}
	}
}