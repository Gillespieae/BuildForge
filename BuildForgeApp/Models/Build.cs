using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BuildForgeApp.Models
{ 
    // represents a PC build created by a user
    public class Build
    {
        public int Id { get; set; } // primary key

        [Required] // build must have a name
        [StringLength(100)] // limits name length in DB + validation
        public string BuildName { get; set; } = string.Empty;

        // timestamp for when the build was created
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // total cost of all components in the build
        public decimal TotalPrice { get; set; }

        [Required] // every build must be tied to a user
        public string UserId { get; set; } = string.Empty;

        // navigation property to the Identity user who owns the build
        public IdentityUser? User { get; set; }

        // join relationship: one build can have many components
        public ICollection<BuildComponent> BuildComponents { get; set; } = new List<BuildComponent>();
    }
}