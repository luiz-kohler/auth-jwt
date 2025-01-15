using API.DTOs;
using API.Entities;
using API.Handlers;
using API.Infra;
using System.ComponentModel.DataAnnotations;

namespace API.Services
{
    public interface IUserService
    {
        Task<string> Create(UserRequest request);
    }

    public class UserService : IUserService
    {
        private readonly IHashHandler _hashHandler;
        private readonly ITokenHandler _tokenHandler;
        private readonly IUserRepository _userRepository;

        public UserService(
            ITokenHandler tokenHandler,
            IUserRepository userRepository,
            IHashHandler hashHandler)
        {
            _userRepository = userRepository;
            _tokenHandler = tokenHandler;
            _hashHandler = hashHandler;
        }

        public async Task<string> Create(UserRequest request)
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

            var userWithSameEmail = await _userRepository.FirstOrDefaultWithWithNoTrackingAsync(user => user.Email == request.Email);

            if (userWithSameEmail != null)
                throw new BadHttpRequestException("there is already user with same email");

            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                PasswordHashed = _hashHandler.Hash(request.Password),
                TokenRefresh = string.Empty,
                IsAdmin = request.IsAdmin
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return _tokenHandler.GenerateAccessToken(user);
        }
    }
}
