using Microsoft.AspNetCore.Mvc;

namespace Wave.Controllers;

[ApiController]
[Route("/theme")]
public class ThemeController : ControllerBase {
    [HttpGet("")]
    public IActionResult SetTheme(string returnUrl = "") {
        Response.Cookies.Delete(".Wave.Theme");
        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }
    
    [HttpGet("{theme}")]
    public IActionResult SetTheme(string? theme, string returnUrl = "") {
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