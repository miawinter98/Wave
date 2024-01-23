using Markdig;
using Microsoft.AspNetCore.Components;
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

    public static MarkupString ParseToMarkup(string markdown) {
        return new MarkupString(Parse(markdown));
    }
}