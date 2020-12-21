using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Conesoft.Users
{
    public static class StartupExtensions
    {
        public static void AddUsers(this IServiceCollection services, string applicationName, string rootPath = "")
        {
            services
                .AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(rootPath))
                .SetApplicationName(applicationName);

            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(365);
                    options.SlidingExpiration = true;
                });

            services.AddSingleton(s => new UsersRootPath(rootPath));

            services.AddControllers().AddApplicationPart(typeof(StartupExtensions).Assembly);
        }

        public static void UseUsers(this IApplicationBuilder app)
        {
            app.UseAuthentication();
        }
    }
}
