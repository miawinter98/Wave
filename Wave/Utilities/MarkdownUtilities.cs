using ColorCode.Styling;
using Markdig;
using Markdig.Extensions.MediaLinks;
using Microsoft.AspNetCore.Components;
using Markdown.ColorCode;
using Wave.Utilities.SyntaxHighlighting;

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
            .UseColorCode(HtmlFormatterType.Style, StyleDictionary.DefaultDark, [
                new ShellLanguage()
            ])
            .DisableHtml()
		.Build();
		return Markdig.Markdown.ToHtml(markdown, pipeline);
    }

    public static MarkupString ParseToMarkup(string markdown) {
        return new MarkupString(Parse(markdown));
    }
}