namespace RaceDayApplication.Web.Models
{
    public class OrganiserEvents
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int EnrollmentCount { get; set; }
    }
}
