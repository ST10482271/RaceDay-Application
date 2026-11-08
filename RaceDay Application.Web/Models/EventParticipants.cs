namespace RaceDayApplication.Web.Models
{
    public class EventParticipants
    {
        public int EnrollmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
    }
}
