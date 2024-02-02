using Markdig;
using Markdig.Extensions.MediaLinks;
using Microsoft.AspNetCore.Components;
using Wave.Data;

namespace Wave.Utilities;

public static class MarkdownUtilities {
    public static string Parse(string markdown) {
        var pipeline = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseEmphasisExtras()
            .UseListExtras()
            .UseSoftlineBreakAsHardlineBreak()
            .UseSmartyPants()
            .UseAutoLinks()
            .UseMediaLinks(new MediaOptions {
                AddControlsProperty = true,
                Class = "max-w-full"
            })
            .DisableHtml()
		.Build();
		return Markdown.ToHtml(markdown, pipeline);
    }

    public static MarkupString ParseToMarkup(string markdown) {
        return new MarkupString(Parse(markdown));
    }
}