using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wave.Data;
using Wave.Data.Transactional;

namespace Wave.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ArticleController(ILogger<ArticleController> logger, ApplicationRepository repository) : ControllerBase {
	[HttpGet("/api/categories"), AllowAnonymous]
	[Produces("application/json")]
	public async Task<Results<
			Ok<IEnumerable<Category>>, 
			NoContent, 
			UnauthorizedHttpResult, 
			ProblemHttpResult>> GetCategories(bool all = false, CancellationToken cancellation = default) {
		try {
			var categories = await repository.GetCategories(User, all, cancellation);
			
			if (categories.Count < 1) return TypedResults.NoContent();
			return TypedResults.Ok<IEnumerable<Category>>(categories);
		} catch (ApplicationException) {
			logger.LogTrace("Unauthenticated user tried to access all categories. Denied.");
			return TypedResults.Unauthorized();
		} catch (Exception ex) {
			logger.LogError(ex, "Unexpected error trying to get Categories for user {UserId}.", User.FindFirstValue("Id") ?? "unknown or anonymous");
			return TypedResults.Problem();
		}
	}

	[HttpGet("{id:guid}"), AllowAnonymous]
	[Produces("application/json")]
	public async Task<Results<
			Ok<ArticleView>, 
			NotFound, 
			UnauthorizedHttpResult, 
			ProblemHttpResult>> GetArticle(Guid id, CancellationToken cancellation = default) {
		try {
			return TypedResults.Ok(new ArticleView(await repository.GetArticleAsync(id, User, cancellation)));
		} catch (ArticleNotFoundException) {
			logger.LogWarning("Failed to look up Article with Id {ArticleId}. Not Found", id);
			return TypedResults.NotFound();
		} catch (ArticleMissingPermissionsException) {
			logger.LogWarning(
				"Failed to look up Article with Id {ArticleId}. User {UserId} Access Denied.", 
				id, User.FindFirstValue("Id") ?? "unknown or anonymous");
			return TypedResults.Unauthorized();
		} catch (Exception ex) {
			logger.LogError(ex, "Unexpected Error.");
			return TypedResults.Problem();
		}
	}

	[HttpPut(Name = nameof(CreateArticle)), Authorize]
	[Produces("application/json")]
	public async Task<Results<
			CreatedAtRoute<ArticleView>, 
			BadRequest<string>, 
			UnauthorizedHttpResult, 
			ProblemHttpResult>> CreateArticle(ArticleCreateDto input, CancellationToken cancellation = default) {
		try {
			var article = new ArticleView(await repository.CreateArticleAsync(input, User, cancellation));
			return TypedResults.CreatedAtRoute(article, nameof(CreateArticle), article.Id);
		} catch (ArticleMissingPermissionsException) {
			logger.LogWarning(
				"Unauthorized User with ID {UserId} tried to create an Article.",
				User.FindFirstValue("Id") ?? "unknown or anonymous");
			return TypedResults.Unauthorized();
		} catch (ArticleMalformedException ex) {
			logger.LogWarning("User with ID {UserId} tried to create an article but submitted bad data.", 
				User.FindFirstValue("Id") ?? "unknown or anonymous");
			return TypedResults.BadRequest($"Submitted data is not valid: {string.Join(",", ex.Errors.Select(e => e.ErrorMessage))}.");
		} catch (Exception ex) {
			logger.LogError(ex, "Unexpected Error.");
			return TypedResults.Problem();
		}
	}

	[HttpPost, Authorize]
	[Produces("application/json")]
	public async Task<Results<
			Ok<ArticleView>, 
			NotFound,
			BadRequest<string>, 
			UnauthorizedHttpResult,
			ProblemHttpResult>> UpdateArticle(ArticleUpdateDto input, CancellationToken cancellation = default) 
	{
		try {
			return TypedResults.Ok(new ArticleView(await repository.UpdateArticleAsync(input, User, cancellation)));
		} catch (ArticleNotFoundException) {
			return TypedResults.NotFound();
		} catch (ArticleMalformedException ex) {
			logger.LogWarning("User with ID {UserId} tried to update article with ID {ArticleId} but submitted bad data.", 
				User.FindFirstValue("Id") ?? "unknown or anonymous", input.Id);
			return TypedResults.BadRequest($"Submitted data is not valid: {string.Join(",", ex.Errors.Select(e => e.ErrorMessage))}.");
		} catch (ArticleMissingPermissionsException) {
			return TypedResults.Unauthorized();
		}  catch (ArticleException ex) {
			logger.LogError(ex, "Unexpected Article Error.");
			return TypedResults.Problem();
		}
	}

}