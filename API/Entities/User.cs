namespace API.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public bool IsAdmin { get; set; }
        public int? RefreshTokenId { get; set; }
        public virtual RefreshToken? RefreshToken { get; set; }
    }
}
