using System.Text.Json.Serialization;

namespace RaceDayApplication.Web.Models
{
    public class EventViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Organizer { get; set; } = string.Empty;
    }
}
