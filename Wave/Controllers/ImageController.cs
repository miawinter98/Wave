using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wave.Services;

namespace Wave.Controllers;

[ApiController]
[Route("/images")]
public class ImageController(ImageService imageService) : ControllerBase {
    private ImageService ImageService { get; } = imageService;

    [HttpGet]
    [Route("{imageId:guid}")]
    public IActionResult Get(Guid imageId) {
        string? path = ImageService.GetPath(imageId);

        if (path is null) return NotFound();
        return File(System.IO.File.OpenRead(path), ImageService.ImageMimeType);
    }
}