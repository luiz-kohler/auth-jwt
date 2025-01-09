using Microsoft.Extensions.Options;
using XSystem.Security.Cryptography;

namespace API.Handlers
{
    public class HashHandler : IHashHandler
    {
        private readonly string _salt;

        public HashHandler(IOptions<HashHandlerOptions> options)
        {
            _salt = options?.Value?.HashSalt ?? throw new ArgumentException("Hash salt must be informed");

        }

        public string Hash(string input)
        {
            using (var sha = new SHA256Managed())
            {
                byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(input + _salt);
                byte[] hashBytes = sha.ComputeHash(textBytes);

                string hash = BitConverter
                    .ToString(hashBytes)
                    .Replace("-", String.Empty);

                return hash;
            }
        }

        public bool Verify(string input, string hashedValue)
        {
            var inputHashed = Hash(input);

            return inputHashed == hashedValue;
        }
    }

    public interface IHashHandler
    {
        string Hash(string input);
        bool Verify(string input, string hashedValue);
    }

    public class HashHandlerOptions
    {
        public string HashSalt { get; set; }
    }
}
