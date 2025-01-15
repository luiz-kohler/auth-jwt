namespace API.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHashed { get; set; }
        public string TokenRefresh { get; set; }
        public bool IsAdmin { get; set; }
    }
}
