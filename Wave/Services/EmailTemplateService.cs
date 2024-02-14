using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Distributed;
using Mjml.Net;

namespace Wave.Services;

public partial class EmailTemplateService(ILogger<EmailTemplateService> logger, IDistributedCache tokenCache, FileSystemService fileSystem) {
	public enum Constants {
		BrowserLink, HomeLink, ContentLogo, ContentTitle, ContentBody, EmailUnsubscribeLink, ArticleRecommendations 
	}

	private ILogger<EmailTemplateService> Logger { get; } = logger;
	private MjmlRenderer Renderer { get; } = new();
	private IDistributedCache TokenCache { get; } = tokenCache;
	private FileSystemService FileSystem { get; } = fileSystem;

	private Regex TokenMatcher { get; } = MyRegex();

	public async Task<(string user, string token)> CreateConfirmTokensAsync(Guid subscriberId, string role = "subscribe", TimeSpan? expiration = null) {
		string user = Convert.ToBase64String(subscriberId.ToByteArray());
		string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
		string cacheKey = role + "-" + user;
		
		await TokenCache.SetAsync(cacheKey, 
			Convert.FromBase64String(token), 
			new DistributedCacheEntryOptions {
				AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromDays(1)
			});
		
		return (user, token);
	}

	public async Task<Guid?> ValidateTokensAsync(string user, string token, string role = "subscribe", bool deleteToken = true) {
		string cacheKey = role + "-" + user;
		byte[]? tokenInCache = await TokenCache.GetAsync(cacheKey);
		
		if (tokenInCache is null || token != Convert.ToBase64String(tokenInCache))
			return null;

		if (deleteToken) await TokenCache.RemoveAsync(cacheKey);
		return new Guid(Convert.FromBase64String(user));
	}
	

	public string Default(string home, string logoLink, string title, string body) {
		return Process("default", new Dictionary<Constants, object?> {
			{Constants.HomeLink, home},
			{Constants.ContentLogo, logoLink},
			{Constants.ContentTitle, title},
			{Constants.ContentBody, body}
		});
	}

	public string Newsletter(string home, string browserUrl, string logoLink, string title, string body, string unsubscribe) {
		return Process("newsletter", new Dictionary<Constants, object?> {
			{ Constants.HomeLink, home },
			{ Constants.BrowserLink, browserUrl },
			{ Constants.ContentLogo, logoLink },
			{ Constants.ContentTitle, title },
			{ Constants.ContentBody, body },
			{ Constants.EmailUnsubscribeLink, unsubscribe }
		});
	}

	public string Welcome(string home, string logoLink, string title, string body, string unsubscribe, string articles) {
		return Process("welcome", new Dictionary<Constants, object?> {
			{ Constants.HomeLink, home },
			{ Constants.ContentLogo, logoLink },
			{ Constants.ContentTitle, title },
			{ Constants.ContentBody, body },
			{ Constants.EmailUnsubscribeLink, unsubscribe },
			{ Constants.ArticleRecommendations, articles }
		});
	}

	public void TryCreateDefaultTemplates() {
		FileSystem.GetEmailTemplate("default", DefaultTemplates["default"]);
		FileSystem.GetEmailTemplate("newsletter", DefaultTemplates["newsletter"]);
		FileSystem.GetEmailTemplate("welcome", DefaultTemplates["welcome"]);
	}

	public string ApplyTokens(string template, Func<string, string?> replacer) {
		return TokenMatcher.Replace(template, t => replacer(t.Value[2..^2]) ?? "");
	}

	public string GetTemplate(string templateName) {
		return FileSystem.GetEmailTemplate(templateName, 
			       DefaultTemplates.TryGetValue(templateName, out string? s) ? s : null) 
		       ?? throw new ApplicationException("Failed to retrieve mail template " + templateName + ".");
	}

	public string CompileTemplate(string template, string templateName = "unknown") {
		var options = new MjmlOptions { Beautify = false };

		(string html, var errors) = Renderer.Render(template, options);

		foreach (var error in errors) {
			Logger.LogWarning("Validation error in template {template}: [{position}] [{type}] {error}", 
				templateName, error.Position, error.Type, error.Error);
		}

		return html;
	}

	public string Process(string templateName, Dictionary<Constants, object?> data) {
		string template = ApplyTokens(GetTemplate(templateName), token => 
			data.TryGetValue(Enum.Parse<Constants>(token, true), out object? v) ? v?.ToString() : null);
		return CompileTemplate(template, templateName);
	}

	[GeneratedRegex(@"(\[\[.*?\]\])", 
		RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
	private static partial Regex MyRegex();


	private Dictionary<string, string> DefaultTemplates { get; } = new() {
		{
			"default",
			$"""
			 <mjml>
			   <mj-head>
			     <mj-preview />
			   </mj-head>
			   <mj-body>
			     <mj-section direction="rtl" padding-bottom="5px" padding-left="0px" padding-right="0px" padding-top="15px" padding="15px 0px 5px 0px">
			       <mj-column vertical-align="middle" width="33%">
			         <mj-image align="center" alt="" border-radius="0" border="none" container-background-color="transparent" height="auto" padding-bottom="5px" padding-left="5px" padding-right="5px" padding-top="5px" padding="5px 5px 5px 5px" href="[[{Constants.HomeLink}]]" src="[[{Constants.ContentLogo}]]"></mj-image>
			       </mj-column>
			       <mj-column vertical-align="middle" width="67%">
			         <mj-text font-size="13px" font-family="Ubuntu,Verdana">
			           <h1>[[{Constants.ContentTitle}]]</h1>
			         </mj-text>
			       </mj-column>
			     </mj-section>
			     <mj-section padding-top="5px" padding-bottom="5px" padding="5px 0 5px 0">
			       <mj-column>
			         <mj-divider border-color="#9f9f9f" border-width="1px"></mj-divider>
			       </mj-column>
			     </mj-section>
			     <mj-section>
			       <mj-column>
			         <mj-text color="#55575d" font-size="13px" font-family="Ubuntu,Verdana">[[{Constants.ContentBody}]]</mj-text>
			       </mj-column>
			     </mj-section>
			     <mj-section padding-top="5px" padding-bottom="5px" padding="5px 0 5px 0">
			       <mj-column>
			         <mj-divider border-color="#9f9f9f" border-width="1px"></mj-divider>
			       </mj-column>
			     </mj-section>
			   </mj-body>
			 </mjml>
			 """
		},
		{
			"newsletter",
			$"""
			 <mjml>
			   <mj-head>
			     <mj-preview />
			   </mj-head>
			   <mj-body>
			     <mj-section>
			       <mj-column>
			         <mj-text align="center" font-size="13px" font-family="Ubuntu,Verdana">
			           <a href="[[{Constants.BrowserLink}]]">Read in Browser</a>
			         </mj-text>
			       </mj-column>
			     </mj-section>
			     <mj-section direction="rtl" padding-bottom="5px" padding-left="0px" padding-right="0px" padding-top="15px" padding="15px 0px 5px 0px">
			       <mj-column vertical-align="middle" width="33%">
			         <mj-image align="center" alt="" border-radius="0" border="none" container-background-color="transparent" height="auto" padding-bottom="5px" padding-left="5px" padding-right="5px" padding-top="5px" padding="5px 5px 5px 5px" href="[[{Constants.HomeLink}]]" src="[[{Constants.ContentLogo}]]"></mj-image>
			       </mj-column>
			       <mj-column vertical-align="middle" width="67%">
			         <mj-text font-size="13px" font-family="Ubuntu,Verdana">
			           <h1>[[{Constants.ContentTitle}]]</h1>
			         </mj-text>
			       </mj-column>
			     </mj-section>
			     <mj-section padding-top="5px" padding-bottom="5px" padding="5px 0 5px 0">
			       <mj-column>
			         <mj-divider border-color="#9f9f9f" border-width="1px"></mj-divider>
			       </mj-column>
			     </mj-section>
			     <mj-section>
			       <mj-column>
			         <mj-text color="#55575d" font-size="13px" font-family="Ubuntu,Verdana">[[{Constants.ContentBody}]]</mj-text>
			       </mj-column>
			     </mj-section>
			     <mj-section padding-top="5px" padding-bottom="5px" padding="5px 0 5px 0">
			       <mj-column>
			         <mj-divider border-color="#9f9f9f" border-width="1px"></mj-divider>
			       </mj-column>
			     </mj-section>
			     <mj-section>
			       <mj-column>
			         <mj-text align="center" font-size="13px" font-family="Ubuntu,Verdana">
			           <a href="[[{Constants.EmailUnsubscribeLink}]]">Unsubscribe</a>
			         </mj-text>
			       </mj-column>
			     </mj-section>
			   </mj-body>
			 </mjml>
			 """
		},
		{
			"welcome",
			$"""
			 <mjml>
			   <mj-head>
			     <mj-preview />
			   </mj-head>
			   <mj-body>
			     <mj-section direction="rtl" padding-bottom="5px" padding-left="0px" padding-right="0px" padding-top="15px" padding="15px 0px 5px 0px">
			       <mj-column vertical-align="middle" width="33%">
			         <mj-image align="center" alt="" border-radius="0" border="none" container-background-color="transparent" height="auto" padding-bottom="5px" padding-left="5px" padding-right="5px" padding-top="5px" padding="5px 5px 5px 5px" href="[[{Constants.HomeLink}]]" src="[[{Constants.ContentLogo}]]"></mj-image>
			       </mj-column>
			       <mj-column vertical-align="middle" width="67%">
			         <mj-text font-size="13px" font-family="Ubuntu,Verdana">
			           <h1>[[{Constants.ContentTitle}]]</h1>
			         </mj-text>
			       </mj-column>
			     </mj-section>
			     <mj-section padding-top="5px" padding-bottom="5px" padding="5px 0 5px 0">
			       <mj-column>
			         <mj-divider border-color="#9f9f9f" border-width="1px"></mj-divider>
			       </mj-column>
			     </mj-section>
			     <mj-section>
			       <mj-column>
			         <mj-text color="#55575d" font-size="13px" font-family="Ubuntu,Verdana">[[{Constants.ContentBody}]]</mj-text>
			       </mj-column>
			     </mj-section>
			     <mj-section padding-top="5px" padding-bottom="5px" padding="5px 0 5px 0">
			       <mj-column>
			         <mj-divider border-color="#9f9f9f" border-width="1px"></mj-divider>
			       </mj-column>
			     </mj-section>
			     <mj-section>
			       <mj-column>
			         <mj-text color="#55575d" font-size="13px" font-family="Ubuntu,Verdana">[[{Constants.ArticleRecommendations}]]</mj-text>
			       </mj-column>
			     </mj-section>
			     <mj-section padding-top="5px" padding-bottom="5px" padding="5px 0 5px 0">
			       <mj-column>
			         <mj-divider border-color="#9f9f9f" border-width="1px"></mj-divider>
			       </mj-column>
			     </mj-section>
			     <mj-section>
			       <mj-column>
			         <mj-text align="center" font-size="13px" font-family="Ubuntu,Verdana">
			           <a href="[[{Constants.EmailUnsubscribeLink}]]">Unsubscribe</a>
			         </mj-text>
			       </mj-column>
			     </mj-section>
			   </mj-body>
			 </mjml>
			 """
		}
	};
}