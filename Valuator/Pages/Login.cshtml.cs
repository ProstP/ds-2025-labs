using System.Security.Claims;
using DatabaseService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class LoginModel : PageModel
{
    private readonly IDatabaseService _databaseService;

    public LoginModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<IActionResult> OnPostAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return Redirect("login");
        }
        if (!IsUserExist(login))
        {
            return Redirect("register");
        }
        if (!IsPasswordTrue(login, password))
        {
            return Redirect("login");
        }

        ClaimsIdentity claimsIdentity = new(
            [
                new Claim(ClaimTypes.Name, login)
            ], CookieAuthenticationDefaults.AuthenticationScheme
        );

        ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Redirect("index");
    }
    private bool IsUserExist(string login)
    {
        string value = _databaseService.Get("USERS", login);

        return !string.IsNullOrWhiteSpace(value);
    }
    private bool IsPasswordTrue(string login, string password)
    {
        string passwordHash = _databaseService.Get("USERS", login);

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}