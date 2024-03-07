using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AspNetCore.Authentication.ApiKey;

namespace Wave.Data;

public record ApiClaim(
	[property:Key] int Id, 
	[property:MaxLength(128)] string Type, 
	[property:MaxLength(128)] string Value);

public class ApiKey : IApiKey {
	[Key, MaxLength(128)]
	public required string Key { get; init; }
	[MaxLength(128)]
	public required string OwnerName { get; set; }

	public List<ApiClaim> ApiClaims { get; } = [];
	
	public IReadOnlyCollection<Claim> Claims => ApiClaims.Select(api => new Claim(api.Type, api.Value)).ToList();
}