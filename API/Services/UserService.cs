using API.DTOs;
using API.Entities;
using API.Infra;

namespace API.Services
{
    public interface IUserService
    {
        Task Create(UserRequest request);
    }

    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;

        public UserService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task Create(UserRequest request)
        {
            var userWithSameEmail = await _userRepository.FirstOrDefaultWithWithNoTrackingAsync(user => user.Email == request.Email);

            if (userWithSameEmail != null)
                throw new BadHttpRequestException("there is already user with same email");

            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                PasswordHashed = request.Password
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }
    }
}
