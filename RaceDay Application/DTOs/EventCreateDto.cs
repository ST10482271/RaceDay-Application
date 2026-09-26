namespace RaceDayApplication.DTOs
{
    public class EventCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
    //A DTO is what the database uses to transfer data between the client and the server.
    //It is there so only the necessary data is transferred and makes sure properties are not exposed to the client that should not be.
    //It is a way to encapsulate data and send it over the network in a structured format.
    // It is a simple object that contains only the necessary properties for creating an event, without any additional logic or behavior.

    // This DTO is used for creating new events.
    // It contains the necessary properties that a client must provide when making a request to create an event.
    // The properties include the event's name, date, location, and category ID.
}
