using System.ComponentModel.DataAnnotations;

namespace Wave.Data;

public class EmailNewsletter {
	[Key]
	public int Id { get; set; }
	
	public bool IsSend { get; set; }
	public required DateTimeOffset DistributionDateTime { get; set; }
	public required Article Article { get; set; }
}