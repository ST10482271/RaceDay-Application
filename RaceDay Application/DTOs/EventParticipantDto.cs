namespace RaceDayApplication.DTOs
{
    public class EventParticipantDto
    {
        public int EnrollmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;//using a string here instead of enum
    }
}
