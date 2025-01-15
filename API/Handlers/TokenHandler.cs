using API.Entities;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Handlers
{
    public class TokenHandler : ITokenHandler
    {
        private static string _secret;

        public TokenHandler(IOptions<TokenHandlerOptions> options)
        {
            _secret = options?.Value?.SecretKey ?? throw new ArgumentException("Secret key must be informed");
        }

        public string GenerateAccessToken(User user)
        {
            return new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(Encoding.ASCII.GetBytes(_secret))
                .AddClaim("exp", DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds())
                .AddClaim("name", user.Name)
                .AddClaim("role", user.IsAdmin
                                  ? Roles.Admin
                                  : Roles.NonAdmin)
                .Encode();
        }

        public IDictionary<string, object> VerifyToken(string token)
        {
            return new JwtBuilder()
                 .WithSecret(_secret)
                 .MustVerifySignature()
                 .Decode<IDictionary<string, object>>(token);
        }
    }

    public interface ITokenHandler
    {
        string GenerateAccessToken(User user);
        IDictionary<string, object> VerifyToken(string token);
    }

    public class TokenHandlerOptions
    {
        public string SecretKey { get; set; }
    }

    public static class Roles
    {
        public const string Admin = "admin";
        public const string NonAdmin = "non-admin";
    }

    public static class JWTConfigurator
    {
        public static void ConfigureJWT(this IServiceCollection services, string secretKey)
        {
            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, 
                    ValidateAudience = false, 
                    ValidateLifetime = true, 
                    ClockSkew = TimeSpan.Zero 
                };
            });
        }
    }
}
