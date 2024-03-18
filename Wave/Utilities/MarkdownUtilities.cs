using Markdig;
using Markdig.Extensions.MediaLinks;
using Microsoft.AspNetCore.Components;
using Markdown.ColorCode;

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
            .UseColorCode()
            .DisableHtml()
		.Build();
		return Markdig.Markdown.ToHtml(markdown, pipeline);
    }

    public static MarkupString ParseToMarkup(string markdown) {
        return new MarkupString(Parse(markdown));
    }
}