using API.DTOs;
using API.Entities;
using API.Handlers;
using API.Infra;
using System.ComponentModel.DataAnnotations;

namespace API.Services
{
    public interface IUserService
    {
        Task<RefreshTokenModel> Create(UserRequest request);
        Task<RefreshTokenModel> SignIn(SignInRequest request);
        Task<string> RefreshToken(RefreshTokenModel request);
    }

    public class UserService : IUserService
    {
        private readonly IHashHandler _hashHandler;
        private readonly ITokenHandler _tokenHandler;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public UserService(
            ITokenHandler tokenHandler,
            IHashHandler hashHandler,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _tokenHandler = tokenHandler;
            _hashHandler = hashHandler;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<RefreshTokenModel> Create(UserRequest request)
        {
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults);

            if(!isValid)
            {
                var errorMessages = validationResults.Select(result => $"{string.Join(", ", result.MemberNames)}: {result.ErrorMessage}");
                var errorMessage = string.Join("; ", errorMessages);
                throw new BadHttpRequestException($"Validation failed: {errorMessage}");
            }

            var userWithSameEmail = await _userRepository.FirstOrDefaultWithNoTrackingAsync(user => user.Email == request.Email);

            if (userWithSameEmail != null)
                throw new BadHttpRequestException("there is already user with same email");

            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                HashedPassword = _hashHandler.Hash(request.Password),
                IsAdmin = request.IsAdmin
            };

            await _userRepository.AddAsync(user);

            var refreshTokenValue = _tokenHandler.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                LastTimeUsed = DateTime.UtcNow,
                Valid = true,
                TimesUsed = 1,
                UserId = user.Id,
                User = user,
                HashedRefreshToken = _hashHandler.Hash(refreshTokenValue)
            };

            await _refreshTokenRepository.AddAsync(refreshToken);

            await _userRepository.SaveChangesAsync();

            return new()
            {
                AccessToken = _tokenHandler.GenerateAccessToken(user),
                RefreshToken = refreshTokenValue
            };
        }

        public async Task<string> RefreshToken(RefreshTokenModel request)
        {
            return await _tokenHandler.Refresh(request.AccessToken, request.RefreshToken);
        }

        public async Task<RefreshTokenModel> SignIn(SignInRequest request)
        {
            var user = await _userRepository.FirstOrDefaultAsync(user => user.Email == request.Email);

            if(user == null)
                throw new BadHttpRequestException("email is not linked to an account");

            if(!_hashHandler.Verify(request.Password, user.HashedPassword))
                throw new BadHttpRequestException("Invalid email or password");


            var currentRefershToken = await _refreshTokenRepository.FirstOrDefaultAsync(rt => rt.Id == user.RefreshTokenId.Value);

            if (currentRefershToken != null)
            {
                _refreshTokenRepository.Remove(currentRefershToken);
                await _refreshTokenRepository.SaveChangesAsync();
            }

            var refreshTokenValue = _tokenHandler.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                LastTimeUsed = DateTime.UtcNow,
                Valid = true,
                TimesUsed = 1,
                UserId = user.Id,
                User = user,
                HashedRefreshToken = _hashHandler.Hash(refreshTokenValue)
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.SaveChangesAsync();

            return new()
            {
                AccessToken = _tokenHandler.GenerateAccessToken(user),
                RefreshToken = refreshTokenValue
            };
        }
    }
}
