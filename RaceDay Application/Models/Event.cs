namespace RaceDayApplication.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;

        // Category Relationship (Points to Category.Id)
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        // This property represents the category associated with this event, which is an instance of the Category class.
        // It is initialized to null! to indicate that it will be set later, typically when the event is retrieved from the database.
        //so when users enter a catergory, they can see all the events that belong to that category.

        // Organizer Relationship (Points to ApplicationUser.Id)
        public string OrganiserId { get; set; } = string.Empty;
        public ApplicationUser Organiser { get; set; } = null!;
        // This property represents the organizer of the event, which is an instance of the ApplicationUser class.
        // It is initialized to null! to indicate that it will be set later, typically when the event is retrieved from the database.

        // Navigation Property (One Event has Many Enrollments)
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        // This property represents a collection of enrollments associated with this event.
        // It is initialized to a new List<Enrollment> to ensure it is not null when accessed.
        
        
    }
}
