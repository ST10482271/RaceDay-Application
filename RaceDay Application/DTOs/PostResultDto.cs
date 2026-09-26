namespace RaceDayApplication.DTOs
{
    public class PostResultDto
    {
        public int EnrollmentId { get; set; }
        public int Position { get; set; }
        public string FinishTime { get; set; } = "00:00:00"; // "HH:mm:ss"
    }
    // This DTO is used for posting race results.
    // It contains the necessary properties that a client(organizer) must provide when making a request to post a race result.
    // The properties include the enrollment ID, position, and finish time.
    //It connects to the PostResult endpoint in the OrganizerController and is used to submit race results for a specific enrollment.
    // It is a simple object that contains only the necessary properties for posting race results, without any additional logic or behavior.
}
