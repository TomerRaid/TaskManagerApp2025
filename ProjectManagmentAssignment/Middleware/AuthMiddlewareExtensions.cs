using Microsoft.AspNetCore.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.IdentityModel.Tokens.Jwt;

namespace ProjectManagmentApp.Middleware
{
    public static class AuthMiddlewareExtensions
    {
        public static IServiceCollection AddCognitoAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var region = configuration["AWS:Region"];
            var userPoolId = configuration["AWS:Cognito:UserPoolId"];
            var appClientId = configuration["AWS:Cognito:ClientId"];

            // Configure JwtBearer to validate tokens from Cognito
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    // AWS Cognito uses JWK for signing keys
                    IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
                    {
                        // Get JsonWebKeySet from AWS
                        var json = new WebClient().DownloadString($"https://cognito-idp.{region}.amazonaws.com/{userPoolId}/.well-known/jwks.json");
                        var keys = new JsonWebKeySet(json).Keys;
                        return keys;
                    },
                    ValidateIssuer = true,
                    ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}",
                    ValidateLifetime = true,
                    LifetimeValidator = (before, expires, token, parameters) => expires > DateTime.UtcNow,
                    ValidateAudience = false,
                    // Add these to ensure all JWT claims can be accessed
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        // Extract the Cognito groups from the token and add them as roles
                        var identity = context.Principal.Identity as ClaimsIdentity;
                        var cognitoGroups = context.Principal.FindFirst("cognito:groups");

                        if (cognitoGroups != null)
                        {
                            var groups = cognitoGroups.Value.TrimStart('[').TrimEnd(']').Split(',');
                            foreach (var group in groups)
                            {
                                if (group == "Admin")
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.Role, "ADMIN"));
                                }
                                identity.AddClaim(new Claim(ClaimTypes.Role, group.Trim('"')));
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Add custom logging here
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });

            // Add authorization policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
            });

            return services;
        }

        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        // Log the error
                        Console.WriteLine($"An error occurred: {contextFeature.Error}");

                        await context.Response.WriteAsync(new ErrorDetails
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "Internal Server Error. Please try again later."
                        }.ToString());
                    }
                });
            });

            return app;
        }
    }

    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }

    public interface IUserAccessor
    {
        string? GetUsername();
        bool IsInRole(string role);
        bool IsInRoleAdmin();
    }

    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetUsername()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst("username")?.Value;
        }

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }
        public bool IsInRoleAdmin()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value == "ADMIN";
        }
    }
}

