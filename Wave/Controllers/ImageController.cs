using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Wave.Services;

namespace Wave.Controllers;

[ApiController]
[Route("/images")]
public class ImageController(ImageService imageService) : ControllerBase {
	private ImageService ImageService { get; } = imageService;

	[HttpGet]
	[Route("{imageId:guid}")]
	public async Task<IActionResult> Get(Guid imageId, [FromQuery, Range(16, 800)] int size = 800) {
		string? path = ImageService.GetPath(imageId);

		if (path is null) return NotFound();
		if (size < 800) return File(await ImageService.GetResized(path, size), ImageService.ImageMimeType);
		return File(System.IO.File.OpenRead(path), ImageService.ImageMimeType);
	}
}