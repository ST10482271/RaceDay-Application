namespace RaceDayApplication.Web.Models
{
    public class UpdateEvent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}
