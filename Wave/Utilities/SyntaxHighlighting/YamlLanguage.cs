using ColorCode;

namespace Wave.Utilities.SyntaxHighlighting;

#nullable disable

public class YamlLanguage : ILanguage {
	public string Id => "yml";
	public string FirstLinePattern => null;
	public string Name => "Yaml";
	public string CssClassName => "yml";

	public IList<LanguageRule> Rules { get; } = [
		new LanguageRule(@"(?:\w+):", new Dictionary<int, string> {
			{0, ColorCode.Common.ScopeName.XmlAttribute}
		}),
		new LanguageRule(@"#(?:.*)\b", new Dictionary<int, string> {
			{0, ColorCode.Common.ScopeName.Comment}
		}),
		new LanguageRule("[^\\\\]\"(?:.*?)[^\\\\]\"", new Dictionary<int, string> {
			{0, ColorCode.Common.ScopeName.String}
		}),
		new LanguageRule(@"\-(?:\s\w.+)\b", new Dictionary<int, string> {
			{0, ColorCode.Common.ScopeName.XmlAttributeValue}
		})
	];
	
	public bool HasAlias(string lang) {
		return string.Equals(lang, "yml", StringComparison.CurrentCultureIgnoreCase) ||
		       string.Equals(lang, "yaml", StringComparison.CurrentCultureIgnoreCase);
	}

}