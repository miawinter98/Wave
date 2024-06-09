using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wave.Data.Transactional;
using Wave.Utilities;

namespace Wave.Data;

public class ArticleException : ApplicationException;
public class ArticleNotFoundException : ArticleException;
public class ArticleMissingPermissionsException : ArticleException;
public class ArticleMalformedException : ArticleException {
	public IReadOnlyCollection<ValidationResult> Errors { get; init; } = [];
}

/// <summary>
/// Adapter for ApplicationDbContext, that enforced valid data and the permission system
/// </summary>
/// <param name="contextFactory"></param>
public class ApplicationRepository(IDbContextFactory<ApplicationDbContext> contextFactory) {
	private IDbContextFactory<ApplicationDbContext> ContextFactory { get; } = contextFactory;

	public async ValueTask<Article?> GetArticle(Guid id, ClaimsPrincipal user, CancellationToken cancellation = default) {
		await using var context = await ContextFactory.CreateDbContextAsync(cancellation);
		var article = await context.Set<Article>()
			.Include(a => a.Author)
			.Include(a => a.Reviewer)
			.Include(a => a.Categories)
			.FirstOrDefaultAsync(a => a.Id == id, cancellation);
		
		if (article.AllowedToRead(user))
			return article;
		
		if (article is null)
			throw new ArticleNotFoundException();
		throw new ArticleMissingPermissionsException();
	}

	public async ValueTask<Article> CreateArticle(ArticleCreateDto dto, ClaimsPrincipal user, CancellationToken cancellation = default) {
		if (!Permissions.AllowedToCreate(user))
			throw new ArticleMissingPermissionsException();

		List<ValidationResult> results = [];
		if (!Validator.TryValidateObject(dto, new ValidationContext(dto), results, true)) {
			throw new ArticleMalformedException() {
				Errors = results
			};
		}
		
		await using var context = await ContextFactory.CreateDbContextAsync(cancellation);
		var appUser = await context.Users.FindAsync([user.FindFirstValue("Id")], cancellation);

		var article = new Article {
			Author = appUser ?? throw new ArticleException(), 
			Title = "", 
			Body = ""
		};
		await context.UpdateArticle(dto, article, cancellation);
		await context.Set<Article>().AddAsync(article, cancellation);
		await context.SaveChangesAsync(cancellation);

		return article;
	}

	public async ValueTask<Article> UpdateArticle(ArticleUpdateDto dto, ClaimsPrincipal user, 
			CancellationToken cancellation = default) {
		List<ValidationResult> results = [];
		if (!Validator.TryValidateObject(dto, new ValidationContext(dto), results, true)) {
			throw new ArticleMalformedException() {
				Errors = results
			};
		}
		
		await using var context = await ContextFactory.CreateDbContextAsync(cancellation);
		var article = await context.Set<Article>()
			.IgnoreQueryFilters()
			.Include(a => a.Author)
			.FirstOrDefaultAsync(a => a.Id == dto.Id, cancellation);
		if (article is null)
			throw new ArticleNotFoundException();
		if (!article.AllowedToEdit(user))
			throw new ArticleMissingPermissionsException();
		var appUser = await context.Users.FindAsync([user.FindFirstValue("Id")], cancellation);
		if (appUser is null)
			throw new ArticleException();

		if (appUser.Id != article.Author.Id) {
			article.Reviewer = appUser;
		}
		
		await context.UpdateArticle(dto, article, cancellation);
		await context.SaveChangesAsync(cancellation);

		return article;
	}

}