using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Wave.Controllers;

[ApiController]
[Route("/language")]
public class LanguageController : ControllerBase {
    [HttpGet("{culture}")]
    public IActionResult SetLanguage(string culture, string returnUrl = "") {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
}