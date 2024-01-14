using ImageMagick;
using static System.Net.Mime.MediaTypeNames;

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

    public async ValueTask<Guid?> StoreImageAsync(string temporaryPath, int size = 800, CancellationToken cancellation = default) {
        if (File.Exists(temporaryPath) is not true) return null;

        try {
            if (File.Exists(BasePath) is not true) Directory.CreateDirectory(BasePath);

            var image = new MagickImage();
            await image.ReadAsync(temporaryPath, cancellation);

            // Jpeg with 90% compression should look decent
            image.Resize(new MagickGeometry(size)); // this preserves aspect ratio
            image.Format = MagickFormat.Jpeg;
            image.Quality = 90;

            if (image.GetExifProfile() is not null) {
                image.RemoveProfile(image.GetExifProfile());
            }

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