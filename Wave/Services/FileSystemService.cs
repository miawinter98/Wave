using System.Text;

namespace Wave.Services;

public class FileSystemService(ILogger<FileSystemService> logger) {
	public const string ConfigurationDirectory = "/configuration";

	private ILogger<FileSystemService> Logger { get; } = logger;

	public async Task<string?> GetEmailTemplateAsync(string name, string? defaultTemplate = null) {
		string path = Path.Combine(ConfigurationDirectory, "templates", "email", name + ".mjml");

		if (!File.Exists(path)) {
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				if (!string.IsNullOrWhiteSpace(defaultTemplate)) {
					await File.WriteAllTextAsync(path, defaultTemplate, Encoding.UTF8);
				}
			} catch (Exception ex) {
				Logger.LogError(ex, "File system access failed trying write default E-Mail template '{template}'", name);
				return defaultTemplate;
			}
		}

		try {
			return await File.ReadAllTextAsync(path, Encoding.UTF8);
		} catch (Exception ex) {
			Logger.LogError(ex, "File system access failed trying to retrieve E-Mail template '{template}'", name);
			return defaultTemplate;
		}
	}
	
	public string? GetEmailTemplate(string name, string? defaultTemplate = null) {
		string path = Path.Combine(ConfigurationDirectory, "templates", "email", name + ".mjml");

		if (!File.Exists(path)) {
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				if (!string.IsNullOrWhiteSpace(defaultTemplate)) {
					File.WriteAllText(path, defaultTemplate, Encoding.UTF8);
				}
			} catch (Exception ex) {
				Logger.LogError(ex, "File system access failed trying write default E-Mail template '{template}'", name);
				return defaultTemplate;
			}
		}

		try {
			return File.ReadAllText(path, Encoding.UTF8);
		} catch (Exception ex) {
			Logger.LogError(ex, "File system access failed trying to retrieve E-Mail template '{template}'", name);
			return defaultTemplate;
		}
	}
}