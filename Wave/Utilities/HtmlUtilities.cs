using System.Text.RegularExpressions;

namespace Wave.Utilities;

public static class HtmlUtilities {
	public static string GetPlainText(string html) {
		return Regex.Replace(html, "<[^>]+>", "", RegexOptions.Compiled | RegexOptions.NonBacktracking);
	}
}