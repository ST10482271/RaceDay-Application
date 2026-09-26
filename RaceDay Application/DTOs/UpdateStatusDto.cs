using RaceDayApplication.Models;

namespace RaceDayApplication.DTOs
{
    public class UpdateStatusDto
    {
        public EnrollmentStatus Status { get; set; }//it is an enum that represents the new status of the enrollment. default value is "Pending". 
    }
    // This DTO is used for updating the status of an enrollment.
    // It contains the necessary property that a client(organizer) must provide when making a request to update the status of an enrollment.
    // The property includes the new status of the enrollment.

    // It connects to the UpdateStatus endpoint in the OrganizerController and is used to change the status of a specific enrollment. e.g . from "Pending" to "Accepted" or "Rejected".
    //It uses the EnrollmentStatus enum from the Enrollment model to ensure that only valid status values are provided.
    // It is a simple object that contains only the necessary property for updating the status of an enrollment, without any additional logic or behavior.
}
