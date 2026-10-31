namespace RaceDayApplication.DTOs
{
    public class UpdateEventDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}
