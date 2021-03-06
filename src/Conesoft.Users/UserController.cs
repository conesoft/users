﻿using Conesoft.Files;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Conesoft.Users
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        readonly Directory rootPath;

        public UserController([FromServices] UsersRootDirectory usersRootPath)
        {
            rootPath = usersRootPath;
        }

        File UserFile(string username) => rootPath / Filename.From(username, "txt");

        [HttpPost("login")]
        public async Task<IActionResult> PostLoginAsync(string username, string password, string redirectto)
        {
            username = username.ToLowerInvariant();
            var passwordHasher = new PasswordHasher<string>();

            if (UserFile(username).Exists)
            {
                var lines = await UserFile(username).ReadLines();

                var salt = lines.First();
                var hashed = lines.Last();

                var validLogin = passwordHasher.VerifyHashedPassword(username, hashed, password + salt);

                if (validLogin == PasswordVerificationResult.Success)
                {
                    // Create the identity from the user info
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, username));
                    identity.AddClaim(new Claim(ClaimTypes.Name, username));

                    // Authenticate using the identity
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });

                }
            }

            return Redirect(redirectto);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> PostLogoutAsync(string redirectto)
        {
            await HttpContext.SignOutAsync();
            return Redirect(redirectto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> PostRegisterAsync(string username, string password, string redirectto)
        {
            username = username.ToLowerInvariant();
            var passwordHasher = new PasswordHasher<string>();

            if (UserFile(username).Exists == false)
            {
                var newsalt = Guid.NewGuid().ToString().ToLower().Replace("-", "");
                await UserFile(username).WriteLines(new[] {
                    newsalt,
                    passwordHasher.HashPassword(username, password + newsalt)
                });

                // Create the identity from the user info
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, username));
                identity.AddClaim(new Claim(ClaimTypes.Name, username));

                // Authenticate using the identity
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });
            }
            return Redirect(redirectto);
        }
    }
}
