using ColorCode;

namespace Wave.Utilities.SyntaxHighlighting;

#nullable disable

public class YamlLanguage : ILanguage {
	public string Id => "yml";
	public string FirstLinePattern => null;
	public string Name => "Yaml";
	public string CssClassName => "yml";

	public IList<LanguageRule> Rules { get; } = [
		new LanguageRule(@"(?m)^\s*(#.*)$", new Dictionary<int, string> {
			{1, ColorCode.Common.ScopeName.Comment}
		}),
		new LanguageRule(@"(?s)^(.+)(\-\-\-)", new Dictionary<int, string> {
			{1, ColorCode.Common.ScopeName.XmlCDataSection}
		}),
		new LanguageRule("""(?m)^\s*(.+:|\- .+)( ".*")?( .+)?""", new Dictionary<int, string> {
			{1, ColorCode.Common.ScopeName.XmlAttributeValue},
			{2, ColorCode.Common.ScopeName.String},
			{3, ColorCode.Common.ScopeName.XmlAttribute}
		})
	];
	
	public bool HasAlias(string lang) {
		return string.Equals(lang, "yml", StringComparison.CurrentCultureIgnoreCase) ||
		       string.Equals(lang, "yaml", StringComparison.CurrentCultureIgnoreCase);
	}

}