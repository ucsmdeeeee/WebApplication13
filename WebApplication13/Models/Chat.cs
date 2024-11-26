namespace WebApplication13.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public int User1Id { get; set; } // Первый пользователь
        public int User2Id { get; set; } // Второй пользователь
        public User User1 { get; set; }
        public User User2 { get; set; }
    }

}
