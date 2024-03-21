using ColorCode;

namespace Wave.Utilities.SyntaxHighlighting;

public class ShellLanguage : ILanguage {
	public string Id => "shell";
	public string FirstLinePattern => @"^\s*\$.*";
	public string Name => "Shell";
	public string CssClassName => "shell";

	public IList<LanguageRule> Rules { get; } = [
		new LanguageRule(@"\$\s*\w[\w\d]+\b", new Dictionary<int, string> {
			{0, ColorCode.Common.ScopeName.PowerShellCommand}
		}),
		new LanguageRule(@"(?:\s(\-\w|\-\-\w\w+)\b)", new Dictionary<int, string> {
			{0, ColorCode.Common.ScopeName.PowerShellParameter}
		}),
		new LanguageRule("[^\\\\]\"(?:.*?)[^\\\\]\"", new Dictionary<int, string> {
			{0, ColorCode.Common.ScopeName.String}
		})
	];

	public bool HasAlias(string lang) {
		return false;
	}

}