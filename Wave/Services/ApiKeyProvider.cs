using System.Security.Claims;
using System.Security.Cryptography;
using AspNetCore.Authentication.ApiKey;
using Microsoft.EntityFrameworkCore;
using Wave.Data;

namespace Wave.Services;

public class ApiKeyProvider(ILogger<ApiKeyProvider> logger, ApplicationDbContext context) : IApiKeyProvider {
	private ILogger<ApiKeyProvider> Logger { get; } = logger;

	private record ActualApiKey(string Key, string OwnerName, IReadOnlyCollection<Claim> Claims) : IApiKey;

	public async Task<IApiKey?> ProvideAsync(string key) {
		try {
			string unescapedKey = key;
			if (unescapedKey.Contains('%')) unescapedKey = Uri.UnescapeDataString(key);

			byte[] data = Convert.FromBase64String(unescapedKey);
			string hashedKey = Convert.ToBase64String(SHA256.HashData(data));

			var apiKey = await context.Set<ApiKey>().Include(a => a.ApiClaims).SingleOrDefaultAsync(k => k.Key == hashedKey);
			if (apiKey is not null)
				return new ActualApiKey(key, apiKey.OwnerName, apiKey.Claims);
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to get api key");
		}
		return null;
	}
}