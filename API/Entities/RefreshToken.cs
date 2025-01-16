namespace API.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public DateTime LastTimeUsed { get; set; }
        public int TimesUsed { get; set; }
        public string HashedRefreshToken { get; set; }
        public bool Valid { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
