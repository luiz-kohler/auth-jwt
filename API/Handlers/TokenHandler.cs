using API.Entities;
using API.Infra;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace API.Handlers
{
    public class TokenHandler : ITokenHandler
    {
        const int MAX_TIMES_REFRESHED = 10;
        private static string _secret;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IHashHandler _hashHandler;

        public TokenHandler(IOptions<TokenHandlerOptions> options,
            IRefreshTokenRepository refreshTokenRepository,
            IHashHandler hashHandler)
        {
            _secret = options?.Value?.SecretKey ?? throw new ArgumentException("Secret key must be informed");
            _refreshTokenRepository = refreshTokenRepository;
            _hashHandler = hashHandler;
        }

        public string GenerateAccessToken(User user)
        {
            return new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(Encoding.ASCII.GetBytes(_secret))
                .AddClaim(Claims.EXPIRATION_TIME, DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds())
                .AddClaim(Claims.ID, user.Id)
                .AddClaim(Claims.NAME, user.Name)
                .AddClaim(Claims.ROLE, user.IsAdmin
                                  ? Roles.ADMIN
                                  : Roles.NON_ADMIN)
                .Encode();
        }

        public IDictionary<string, object> VerifyToken(string token)
        {
            return new JwtBuilder()
                 .WithSecret(_secret)
                 .MustVerifySignature()
                 .Decode<IDictionary<string, object>>(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }

        public bool ValidateIgnoringLifeTime(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = false,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = false,
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> Refresh(string token, string refreshToken)
        {
            if (!ValidateIgnoringLifeTime(token))
                throw new UnauthorizedAccessException();

            var refreshTokenHashed = _hashHandler.Hash(refreshToken);
            var refreshTokenEntity = await _refreshTokenRepository.FirstOrDefaultAsync(x => x.HashedRefreshToken == refreshTokenHashed);

            if (refreshTokenEntity == null)
                throw new UnauthorizedAccessException();

            if (refreshTokenEntity.TimesUsed > MAX_TIMES_REFRESHED ||
                refreshTokenEntity.LastTimeUsed < DateTime.UtcNow.AddMonths(-6))
            {
                refreshTokenEntity.Valid = false;

                _refreshTokenRepository.Update(refreshTokenEntity);
                await _refreshTokenRepository.SaveChangesAsync();

                throw new UnauthorizedAccessException();
            }

            refreshTokenEntity.LastTimeUsed = DateTime.UtcNow;
            refreshTokenEntity.TimesUsed++;

            _refreshTokenRepository.Update(refreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            return GenerateAccessToken(refreshTokenEntity.User);
        }
    }

    public interface ITokenHandler
    {
        string GenerateAccessToken(User user);
        Task<string> Refresh(string token, string refreshToken);
        IDictionary<string, object> VerifyToken(string token);
        string GenerateRefreshToken();
    }

    public class TokenHandlerOptions
    {
        public string SecretKey { get; set; }
    }

    public static class Roles
    {
        public const string ADMIN = "admin";
        public const string NON_ADMIN = "non-admin";
    }

    public static class Claims
    {
        public const string EXPIRATION_TIME = "exp";
        public const string ID = "id";
        public const string NAME = "name";
        public const string ROLE = "role";
    }

    public static class JWTConfigurator
    {
        public static void ConfigureJWT(this IServiceCollection services, string secretKey)
        
        {
            var key = Encoding
                .ASCII
                .GetBytes(secretKey);

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
