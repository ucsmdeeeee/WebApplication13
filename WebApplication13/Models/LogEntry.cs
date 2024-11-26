namespace WebApplication13.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string IP { get; set; }
        public string Url { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
        public string UserAgent { get; set; }
    }

}
