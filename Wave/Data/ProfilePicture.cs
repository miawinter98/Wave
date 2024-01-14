using System.ComponentModel.DataAnnotations;

namespace Wave.Data;

public class ProfilePicture {
    [Key]
    public int Id { get; set; }
    public Guid ImageId { get; set; }
}