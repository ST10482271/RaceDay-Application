using System.ComponentModel.DataAnnotations;

namespace RaceDayApplication.Web.Models
{
    public class CreateEvent
    {
        [Required(ErrorMessage = "Event name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
    }
}
