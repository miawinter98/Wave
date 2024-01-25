using Microsoft.AspNetCore.Mvc;

namespace Wave.Controllers;

[ApiController]
[Route("/theme")]
public class ThemeController : ControllerBase {
    [HttpGet("{theme}")]
    public IActionResult SetLanguage(string? theme, string returnUrl = "") {
        if (theme is null) {
            Response.Cookies.Delete(".Wave.Theme");
        } else {
            Response.Cookies.Append(".Wave.Theme", theme, new CookieOptions {
                Expires = DateTimeOffset.UtcNow.AddYears(1), 
                IsEssential = true, 
                SameSite = SameSiteMode.Strict
            });
        }
        
        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }

}