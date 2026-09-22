namespace RaceDayApplication.Models
{
    public class EventResult
    {
        public int Id { get; set; }

        // Enrollment Relationship (1:1 with Enrollment)
        public int EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;
        // This property represents the enrollment associated with this event result, which is an instance of the Enrollment class.
        //so when users view their race results, they can see the details of their enrollment and the associated event.
        public int Position { get; set; }
        public TimeSpan FinishTime { get; set; }
        // This property represents the finish time of the participant in the event, which is of type TimeSpan.
    }
}
