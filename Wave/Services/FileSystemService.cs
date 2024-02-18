using System.Text;

namespace Wave.Services;

public class FileSystemService(ILogger<FileSystemService> logger) {
	public const string ConfigurationDirectory = "/configuration";

	private ILogger<FileSystemService> Logger { get; } = logger;

	public ValueTask<string?> GetEmailTemplateAsync(string name, string? defaultTemplate = null) {
		string path = Path.Combine(ConfigurationDirectory, "templates", "email", name + ".mjml");
		return GetFileContentAsync(path, defaultTemplate);
	}
	
	public string? GetEmailTemplate(string name, string? defaultTemplate = null) {
		string path = Path.Combine(ConfigurationDirectory, "templates", "email", name + ".mjml");
		return GetFileContent(path, defaultTemplate);
	}
	
	public ValueTask<string?> GetPartialTemplateAsync(string name, string? defaultTemplate = null) {
		string path = Path.Combine(ConfigurationDirectory, "templates", "partials", name + ".html");
		return GetFileContentAsync(path, defaultTemplate);
	}

	public string? GetPartialTemplate(string name, string? defaultTemplate = null) {
		string path = Path.Combine(ConfigurationDirectory, "templates", "partials", name + ".html");
		return GetFileContent(path, defaultTemplate);
	}

	private string? GetFileContent(string path, string? defaultContent = null) {
		if (!File.Exists(path)) {
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				if (!string.IsNullOrWhiteSpace(defaultContent)) {
					File.WriteAllText(path, defaultContent, Encoding.UTF8);
				}
			} catch (Exception ex) {
				Logger.LogError(ex, "File system access failed trying write default content of '{template}'", path);
				return defaultContent;
			}
		}

		try {
			return File.ReadAllText(path, Encoding.UTF8);
		} catch (Exception ex) {
			Logger.LogError(ex, "File system access failed trying to retrieve content of '{template}'", path);
			return defaultContent;
		}
	}
	private async ValueTask<string?> GetFileContentAsync(string path, string? defaultContent = null) {
		if (!File.Exists(path)) {
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				if (!string.IsNullOrWhiteSpace(defaultContent)) {
					await File.WriteAllTextAsync(path, defaultContent, Encoding.UTF8);
				}
			} catch (Exception ex) {
				Logger.LogError(ex, "File system access failed trying write default content of '{template}'", path);
				return defaultContent;
			}
		}

		try {
			return await File.ReadAllTextAsync(path, Encoding.UTF8);
		} catch (Exception ex) {
			Logger.LogError(ex, "File system access failed trying to retrieve content of '{template}'", path);
			return defaultContent;
		}
	}
}