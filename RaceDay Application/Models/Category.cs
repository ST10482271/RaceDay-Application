namespace RaceDayApplication.Models
{
    public class Category// This class represents a category in the application. It contains properties for the category's ID, name, description, and a collection of events associated with this category.
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation Property (One Category has Many Events)
        public ICollection<Event> Events { get; set; } = new List<Event>();
        // This property represents a collection of events associated with this category.
        // It is initialized to a new List<Event> to ensure it is not null when accessed.
        //so when users search for events by category, they can easily retrieve all events that belong to a specific category.
    }
}
