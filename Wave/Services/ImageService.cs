using ImageMagick;

namespace Wave.Services;

public class ImageService(ILogger<ImageService> logger) {
	private ILogger<ImageService> Logger { get; } = logger;
	private const string BasePath = "./files/images";
	private const string ImageExtension = ".jpg";
	public string ImageMimeType => "image/jpg";

	public string? GetPath(Guid imageId) {
		string path = Path.Combine(BasePath, imageId + ImageExtension);
		return File.Exists(path) ? path : null;
	}

	public async Task<byte[]> GetResized(string path, int size, bool enforceSize = false, CancellationToken cancellation = default) {
		var image = new MagickImage(path);
		image.Resize(new MagickGeometry(size));
		if (enforceSize) image.Extent(new MagickGeometry(size), Gravity.Center, MagickColors.Black);
		using var memory = new MemoryStream();
		await image.WriteAsync(memory, cancellation);
		return memory.ToArray();
	}

	public async ValueTask<Guid?> StoreImageAsync(string temporaryPath, int size = 800, bool enforceSize = false,
		CancellationToken cancellation = default) {
		if (File.Exists(temporaryPath) is not true) return null;

		try {
			if (File.Exists(BasePath) is not true) Directory.CreateDirectory(BasePath);

			var image = new MagickImage();
			await image.ReadAsync(temporaryPath, cancellation);

			// Jpeg with 90% compression should look decent
			image.Resize(new MagickGeometry(size)); // this preserves aspect ratio
			if (enforceSize) image.Extent(new MagickGeometry(size), Gravity.Center, MagickColors.Black);
			image.Format = MagickFormat.Jpeg;
			image.Quality = 90;

			if (image.GetExifProfile() is { } exifProfile) image.RemoveProfile(exifProfile);

			// Overwrite exif for privacy reasons
			var exif = new ExifProfile {
				Parts = ExifParts.None
			};
			exif.CreateThumbnail();
			image.SetProfile(exif);

			var guid = Guid.NewGuid();
			string path = Path.Combine(BasePath, guid + ImageExtension);
			await image.WriteAsync(path, cancellation);
			return guid;
		} catch (Exception ex) {
			Logger.LogInformation(ex, "Failed to process uploaded image.");
			return null;
		}
	}

	public void Delete(Guid imageId) {
		string path = Path.Combine(BasePath, imageId + ImageExtension);
		File.Delete(path);
	}
}