using Markdig;
using Wave.Data;

namespace Wave.Utilities;

public static class MarkdownUtilities {
    public static string Parse(string markdown) {
        var pipeline = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseEmphasisExtras()
            .DisableHtml()
		.Build();
		return Markdown.ToHtml(markdown, pipeline);
    }
}