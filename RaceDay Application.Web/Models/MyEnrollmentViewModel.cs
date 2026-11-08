namespace RaceDayApplication.Web.Models
{
    public class MyEnrollmentViewModel
    {
        public int Id { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
        public EventResultViewModel? Result { get; set; }
        
    }

    public class EventResultViewModel
    {
        public int Position { get; set; }
        public string FinishTime { get; set; } = string.Empty;
    }

    public enum EnrollmentStatus
    {
        Pending,
        Accepted,
        Rejected
    }
}
