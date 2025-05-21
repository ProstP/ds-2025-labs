using System.Security.Claims;
using DatabaseService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Valuator.Pages;

public class RegisterModel : PageModel
{
    private readonly IDatabaseService _databaseService;

    public RegisterModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<IActionResult> OnPost(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return Redirect("register");
        }
        if (IsLoginUsed(login))
        {
            return Redirect("login");
        }
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        _databaseService.Set("USERS", login, passwordHash);

        ClaimsIdentity claimsIdentity = new(
            [
                new Claim(ClaimTypes.Name, login)
            ], CookieAuthenticationDefaults.AuthenticationScheme
        );

        ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Redirect("index");
    }
    private bool IsLoginUsed(string login)
    {
        string value = _databaseService.Get("USERS", login);

        return !string.IsNullOrWhiteSpace(value);
    }
}