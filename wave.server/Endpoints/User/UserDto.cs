using wave.server.Data;

namespace wave.server.Endpoints.User;

public record UserDto(string Name)
{
	public UserDto(ApplicationUser record)
		: this(record.FullName ?? "Wave Author") { }
}
