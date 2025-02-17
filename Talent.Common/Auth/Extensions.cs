using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Talent.Common.Auth
{
    public static class Extensions
    {
        public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var options = new JwtOptions();
            var section = configuration.GetSection("jwt");
            section.Bind(options);
            services.Configure<JwtOptions>(configuration.GetSection("jwt"));
            services.AddSingleton<IJwtHandler, JwtHandler>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(cfg =>
                {
                    Console.WriteLine($"Config JWT key: {options.SecretKey}");
                    Console.WriteLine($"Config Issuer key: {options.Issuer}");

                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = false,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = options.Issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey))
                    };

                    // Disable automatic claim mapping
                    cfg.MapInboundClaims = false;

  
                    // Add event handling for debugging
                    cfg.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var token = context.SecurityToken as Microsoft.IdentityModel.JsonWebTokens.JsonWebToken; // Use JsonWebToken for .NET 8
                            Console.WriteLine($"Token validated: {token?.Id}");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        }
                    };
                });
        }

    }
}
