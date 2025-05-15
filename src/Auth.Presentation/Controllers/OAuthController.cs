using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Auth.Presentation.Controllers;

public class OAuthController : Controller
{
    public IActionResult Login(string redirectUri)
    {
        ViewData["redirectUri"] = redirectUri;
        return View();
    }

    public IActionResult Hello()
    {
        return View();
    }
}