using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaceDayApplication.Models;

namespace RaceDayApplication.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    //this class inherits from IdentityDbContext to include identity management features
    //this class is used to define the database context for the application which is used in the controllers,
    //this class also includes the DbSet properties for the entities and the configuration of relationships between them.
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        // Constructor to initialize the database context with options, base(options) is used to pass the options to the base class constructor which is IdentityDbContext

        // DbSet is used to represent collections of entities in the database, it comes from the
        // Microsoft.EntityFrameworkCore namespace and is used to query and save instances of the entity type.
        public DbSet<Category> Categories { get; set; } = null!;//this property represents the Categories table in the database
        public DbSet<Event> Events { get; set; } = null!;//this property represents the Events table in the database
        public DbSet<Enrollment> Enrollments { get; set; } = null!;//this property represents the Enrollments table in the database
        public DbSet<EventResult> EventResults { get; set; } = null!;//this property represents the EventResults table in the database

        //collections meaning that each DbSet represents a table in the database and allows for querying and saving instances of the entity type

        protected override void OnModelCreating(ModelBuilder builder)
        // The OnModelCreating method is used to configure the model and relationships between entities
        // It is called when the model for a derived context has been initialized, but before the model has been locked down and used to initialize the context.
        //meaning that it is called before the model is finalized and used to create the database schema.
        //This allows for customization of the model and relationships between entities before they are locked down.
        //it is a protected method that can be overridden in a derived class e.g.in the AppDbContext class to customize the model and relationships between entities.


        // The ModelBuilder API is used to configure the model and relationships between entities
        //it comes from the Microsoft.EntityFrameworkCore namespace and is used to configure the model and relationships between entities in the database.
        {
            base.OnModelCreating(builder);
            // Call the base method to ensure identity tables are created and preserve the default behavior of the IdentityDbContext
            //identity tables are created because the AppDbContext class inherits from IdentityDbContext which includes identity management features such as user authentication and authorization.

            // Configure the relationships between entities using the Fluent API which comes from the Microsoft.EntityFrameworkCore namespace
            // Fluent API is a way to configure the model and relationships between entities using method chaining and lambda expressions
            //method chaining allows for a more readable and expressive way to configure the model and relationships between entities

            builder.Entity<Enrollment>()// Configure the Enrollment entity 
                .HasOne(e => e.Participant)// Configure the relationship between Enrollment and Participant
                .WithMany()// Configure the relationship between Participant and Enrollment
                .HasForeignKey(e => e.ParticipantId)// Configure the foreign key for the relationship between Enrollment and Participant
                .OnDelete(DeleteBehavior.Cascade);// Configure the delete behavior for the relationship between Enrollment and Participant

            builder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany()
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EventResult>()
                .HasIndex(r => r.EnrollmentId)
                .IsUnique();

            builder.Entity<EventResult>()
                .HasOne(r => r.Enrollment)
                .WithOne(e => e.Result)
                .HasForeignKey<EventResult>(r => r.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure a participant can only enroll in an event ONCE at the database level
            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.EventId, e.ParticipantId })//this code configures a unique index on the combination of EventId and ParticipantId in the Enrollment entity
                .IsUnique();//this code configures a unique index on the combination of EventId and ParticipantId in the Enrollment entity
        }
        //we configure the relationships between entities because it allows us to define how the entities are related to each other and how they should behave when certain actions are performed on them.
        //For example, we can define how the entities should behave when a related entity is deleted or updated,
        //and we can enforce constraints on the relationships between entities to ensure data integrity.
    }
}
