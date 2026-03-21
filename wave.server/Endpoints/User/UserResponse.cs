using wave.server.Data;

namespace wave.server.Endpoints.User;

public record UserResponse(string Id, string Name)
{
    public UserResponse(ApplicationUser user)
        : this(user.Id, user.FullName ?? "Wave User") { }
}
