using System.ComponentModel.DataAnnotations;
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
	public async Task<IActionResult> Get(string userId, [FromQuery, Range(16, 800)] int size = 800) {
		if (size > 800) size = 800;
		await using var context = await ContextFactory.CreateDbContextAsync();
		var user = await context.Users.Include(u => u.ProfilePicture).FirstOrDefaultAsync(u => u.Id == userId);
		if (user is null) return NotFound();
		if (user.ProfilePicture is null) {
			return Redirect("/dist/img/default_avatar.jpg");
		}

		string? path = ImageService.GetPath(user.ProfilePicture.ImageId);
		if (path is null) {
			return Redirect("/dist/img/default_avatar.jpg");
		}

		if (size < 800) return File(await ImageService.GetResized(path, size), ImageService.ImageMimeType);
		return File(System.IO.File.OpenRead(path), ImageService.ImageMimeType);
	}
}