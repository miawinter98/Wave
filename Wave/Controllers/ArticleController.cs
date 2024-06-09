﻿using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Wave.Data;
using Wave.Data.Transactional;

namespace Wave.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ArticleController(ILogger<ArticleController> logger, ApplicationRepository repository) : ControllerBase {
	[HttpGet, AllowAnonymous]
	[Produces("application/json")]
	public async Task<Results<
			Ok<Article>, 
			NotFound, 
			UnauthorizedHttpResult, 
			ProblemHttpResult>> GetArticle(Guid id, CancellationToken cancellation = default) {
		try {
			return TypedResults.Ok(await repository.GetArticle(id, User, cancellation));
		} catch (ArticleNotFoundException) {
			logger.LogWarning("Failed to look up Article with Id {ArticleId}. Not Found", id);
			return TypedResults.NotFound();
		} catch (ArticleMissingPermissionsException) {
			logger.LogWarning(
				"Failed to look up Article with Id {ArticleId}. User {UserId} Access Denied.", 
				id, User.FindFirstValue("Id") ?? "unknown or anonymous");
			return TypedResults.Unauthorized();
		} catch (ArticleException ex) {
			logger.LogError(ex, "Unexpected Article Error.");
			return TypedResults.Problem();
		}
	}

	[HttpPut, Authorize]
	[Produces("application/json")]
	public async Task<Results<
			CreatedAtRoute<ArticleView>, 
			BadRequest<string>, 
			UnauthorizedHttpResult, 
			ProblemHttpResult>> CreateArticle(ArticleDto input, CancellationToken cancellation = default) {
		if (input.Id is not null) return TypedResults.BadRequest(
			"You cannot provide an ID when creating an article, did you intend to update an existing one instead?");

		try {
			var article = new ArticleView(await repository.CreateArticle(input, User, cancellation));
			return TypedResults.CreatedAtRoute(article, nameof(GetArticle), article.Id);
		} catch (ArticleMissingPermissionsException) {
			logger.LogWarning(
				"Unauthorized User with ID {UserId} tried to create an Article.",
				User.FindFirstValue("Id") ?? "unknown or anonymous");
			return TypedResults.Unauthorized();
		} catch (ArticleMalformedException ex) {
			logger.LogWarning("User with ID {UserId} tried to create an article but submitted bad data.", 
				User.FindFirstValue("Id") ?? "unknown or anonymous");
			return TypedResults.BadRequest($"Submitted data is not valid: {string.Join(",", ex.Errors.Select(e => e.ErrorMessage))}.");
		} catch (ArticleException ex) {
			logger.LogError(ex, "Unexpected Article Error.");
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
			ProblemHttpResult>> SaveArticle(ArticleDto input, CancellationToken cancellation = default) 
	{
		try {
			return TypedResults.Ok(new ArticleView(await repository.UpdateArticle(input, User, cancellation)));
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
	
	public sealed record ArticleView(Guid Id, string Title, string Slug) {
		public ArticleView(Article article) : this(article.Id, article.Title, article.Slug) {}
	}
}