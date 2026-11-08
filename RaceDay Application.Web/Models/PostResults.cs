namespace RaceDayApplication.Web.Models
{
    public class PostResults
    {
        public int EnrollmentId { get; set; }
        public int EventId { get; set; }
        public int Position { get; set; }
        public string FinishTime { get; set; } = "00:00:00"; // Format: "HH:mm:ss"
    }
}
