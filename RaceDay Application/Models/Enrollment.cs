namespace RaceDayApplication.Models
{
    // This enum represents the possible statuses of an enrollment in an event.
    public enum EnrollmentStatus
    {
        Pending,
        Accepted,
        Rejected
        // The possible statuses are Pending, Accepted, and Rejected.
    }

    public class Enrollment
    {
       
            public int Id { get; set; }

           // Event Relationship (Points to Event.Id)
            public int EventId { get; set; }
            public Event Event { get; set; } = null!;
        //this property represents the event associated with this enrollment, which is an instance of the Event class.
        //so when users view their enrollments, they can see the details of the event they are enrolled in.

        // Participant Relationship (Points to ApplicationUser.Id)
        public string ParticipantId { get; set; } = string.Empty;
            public ApplicationUser Participant { get; set; } = null!;
            //this property represents the participant of the enrollment, which is an instance of the ApplicationUser class.

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
        //this property represents the status of the enrollment, which is an instance of the EnrollmentStatus enum.
        //It is initialized to EnrollmentStatus.Pending by default.

        // Optional 1-to-1 Race Result
        public EventResult? Result { get; set; }
        //this property represents the race result associated with this enrollment, which is an instance of the EventResult class.
        //It is nullable, indicating that an enrollment may not have a race result yet.
        //so when users view their enrollments, they can see the status of their enrollment and any associated race results if available.

    }
}
