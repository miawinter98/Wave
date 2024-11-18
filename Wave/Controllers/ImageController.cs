using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
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

	[HttpPut("create")]
	[Authorize(Policy = "ArticleEditPermissions")]
	[Consumes("multipart/form-data")]
	public async Task<IActionResult> CreateImageAsync(
			[FromForm] IFormFile file, 
			ImageService.ImageQuality quality = ImageService.ImageQuality.Normal) {
		try {
			string tempFile = Path.GetTempFileName();
			{
				await using var stream = System.IO.File.OpenWrite(tempFile);
				await file.CopyToAsync(stream);
				stream.Close();
			}
			var id = await ImageService.StoreImageAsync(tempFile);
			if (id is null) throw new ApplicationException("Saving image failed unexpectedly.");
			return Created($"/images/{id}", new CreateResponse(id.Value));
		} catch (Exception ex) {
			return BadRequest($"Failed to process image: {ex.Message}.");
		}
	}

	record CreateResponse(Guid Id);
}