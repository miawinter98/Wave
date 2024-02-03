using Microsoft.AspNetCore.Mvc;

namespace Wave.Controllers;

[ApiController]
[Route("/api/proxy")]
public class ApiProxy(HttpClient client) : ControllerBase {
	private HttpClient Client { get; } = client;

	[Route("favicon/{host}")]
	[Produces("image/x-icon")]
	[ResponseCache(Duration = 60*60*24*30, Location = ResponseCacheLocation.Any)]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task GetFavicon(string host, [FromQuery] int size = 32) {
		var response = await DoProxy("https://favicone.com/" + host + "?s=" + size);

		if (!response.IsSuccessStatusCode) {
			Response.StatusCode = StatusCodes.Status404NotFound;
			return;
		}

		byte[] data = await response.Content.ReadAsByteArrayAsync();
		await Response.BodyWriter.WriteAsync(data);
	}


	private async Task<HttpResponseMessage> DoProxy(string url) {
		return await Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url));
	}
}