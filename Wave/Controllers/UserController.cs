using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Wave.Data;
using Wave.Services;

namespace Wave.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UserController(ImageService imageService, IDbContextFactory<ApplicationDbContext> contextFactory) : ControllerBase {
	private ImageService ImageService { get; } = imageService;
	private IDbContextFactory<ApplicationDbContext> ContextFactory { get; } = contextFactory;

	[HttpGet]
	[OutputCache(Duration = 60*5)]
	[ResponseCache(Duration = 60*5, Location = ResponseCacheLocation.Any)]
	[Route("pfp/{userId}")]
	public async Task<IActionResult> Get(string userId) {
		await using var context = await ContextFactory.CreateDbContextAsync();
		var user = await context.Users.Include(u => u.ProfilePicture).FirstOrDefaultAsync(u => u.Id == userId);
		if (user is null) return NotFound();
		if (user.ProfilePicture is null) {
			return StatusCode(StatusCodes.Status204NoContent);
		}

		string? path = ImageService.GetPath(user.ProfilePicture.ImageId);
		if (path is null) return NotFound();
		
		return File(System.IO.File.OpenRead(path), ImageService.ImageMimeType);
	}
	
	[HttpPost("link/{linkId:int}")]
	[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
	public async Task<IActionResult> DeleteUserLink(int linkId, [FromServices] UserManager<ApplicationUser> userManager) {
		if (!string.Equals(Request.Form["_method"], "delete", StringComparison.InvariantCultureIgnoreCase))
			return BadRequest();

		string returnUrl = Request.Form["ReturnUrl"].FirstOrDefault() ?? string.Empty;

		var user = await userManager.GetUserAsync(User);
		if (user is null) return Unauthorized();

		var link = user.Links.FirstOrDefault(l => l.Id == linkId);
		if (link is null) return NotFound();

		user.Links.Remove(link);
		await userManager.UpdateAsync(user);
		return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
	}
}