using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wave.Data;
using Wave.Services;

namespace Wave.Controllers;

[ApiController]
[Route("/api/user")]
public class UserController(ImageService imageService, IDbContextFactory<ApplicationDbContext> contextFactory) : ControllerBase {
    private ImageService ImageService { get; } = imageService;
    private IDbContextFactory<ApplicationDbContext> ContextFactory { get; } = contextFactory;

    [HttpGet]
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
}