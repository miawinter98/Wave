namespace Wave.Utilities;

public static class FileUtilities {
    public static async Task<string> StoreTemporary(Stream fileStream) {
        string tempName = Path.GetRandomFileName();
        string tempDirectory = Path.Combine(".", "files", "temp");
        Directory.CreateDirectory(tempDirectory);
        string tempPath = Path.Combine(tempDirectory, tempName);
        await using var fs = new FileStream(tempPath, FileMode.Create);
        
        await fileStream.CopyToAsync(fs);

        return tempPath;
    }
}