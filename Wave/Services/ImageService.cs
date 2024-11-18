using ImageMagick;

namespace Wave.Services;

public class ImageService(ILogger<ImageService> logger) {
	public enum ImageQuality {
		Normal, High, Source
	}

	private ILogger<ImageService> Logger { get; } = logger;
	private const string BasePath = "./files/images";
	private const string ImageExtension = ".webp";
	public string ImageMimeType => "image/webp";

	public string? GetPath(Guid imageId) {
		string path = Path.Combine(BasePath, imageId + ImageExtension);
		// Fallback for older version
		if (!File.Exists(path)) {
			path = Path.Combine(BasePath, imageId + ".jpg");
		}
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
		CancellationToken cancellation = default, ImageQuality quality = ImageQuality.Normal) {
		if (File.Exists(temporaryPath) is not true) return null;

		try {
			if (File.Exists(BasePath) is not true) Directory.CreateDirectory(BasePath);

			var image = new MagickImage();
			await image.ReadAsync(temporaryPath, cancellation);
			
			image.Format = MagickFormat.WebP;
			if (quality is ImageQuality.Source) {
				image.Quality = 100;
				// Do not resize
			} else {
				int storedSize = size;
				if (quality is ImageQuality.Normal && storedSize < 800) storedSize = 800;
				if (quality is ImageQuality.High && storedSize < 1600) storedSize = 1600;

				image.Resize(new MagickGeometry(storedSize)); // this preserves aspect ratio
				if (enforceSize) image.Extent(new MagickGeometry(storedSize), Gravity.Center, MagickColors.Black);
				image.Quality = quality switch {
					ImageQuality.Normal => 85,
					ImageQuality.High => 95,
					var _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, null)
				};
			}

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
			Logger.LogWarning(ex, "Failed to process uploaded image.");
			return null;
		}
	}

	public void Delete(Guid imageId) {
		string path = Path.Combine(BasePath, imageId + ImageExtension);
		File.Delete(path);
	}
}