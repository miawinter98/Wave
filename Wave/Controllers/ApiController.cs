using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Wave.Data;
using Wave.Data.Api;

namespace Wave.Controllers;

[ApiController]
[Route("/[controller]")]
public class ApiController(ApplicationDbContext context, IOptions<Customization> customizationOptions) : ControllerBase {

	[HttpGet("article/featured")]
	[Produces("application/json")]
	public async Task<Results<Ok<ArticleDto>, NoContent>> GetArticleFeatured([FromQuery, Range(16, 800)] int profilePictureSize = 800) {
		Response.Headers.AccessControlAllowOrigin = "*";

		var article = await context.Set<Article>()
			.IgnoreAutoIncludes()
			.Include(a => a.Author).ThenInclude(a => a.Articles)
			.Include(a => a.Reviewer)
			.Include(a => a.Categories)
			.OrderByDescending(a => a.PublishDate).ThenBy(a => a.Id)
			.FirstOrDefaultAsync();
		if (article is null) return TypedResults.NoContent();

		return TypedResults.Ok(ArticleDto.GetFromArticle(article, GetHost(), profilePictureSize));
	}

	[HttpGet("email/subscriber/{email}")]
	[Produces("application/json")]
	[Authorize("EmailApi")]
	[OutputCache(Duration = 60*10)]
	public async Task<Results<Ok<EmailSubscriberDto>, NotFound>> GetEmailSubscriber([EmailAddress] string email) {
		var subscriber = await context.Set<EmailSubscriber>()
			.IgnoreQueryFilters()
			.FirstOrDefaultAsync(s => s.Email == email);
		if (subscriber is null) return TypedResults.NotFound();

		return TypedResults.Ok(new EmailSubscriberDto(subscriber));
	}

	private Uri GetHost() {
		string customUrl = customizationOptions.Value.AppUrl;

		if (!string.IsNullOrEmpty(customUrl)) return new Uri(customUrl, UriKind.Absolute);
		return new Uri($"{Request.Scheme}://{Request.Host}");
	}
}