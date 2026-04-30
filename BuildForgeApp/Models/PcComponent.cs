using System.ComponentModel.DataAnnotations;

namespace BuildForgeApp.Models
{
    // represents a single PC part (CPU, GPU, RAM, etc.)
    public class PcComponent
    {
        public int Id { get; set; } // primary key

        [Required] // component must have a name
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required] // manufacturer (Intel, AMD, NVIDIA, etc.)
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required] // type/category (CPU, GPU, RAM, Motherboard, PSU)
        [StringLength(50)]
        public string ComponentType { get; set; } = string.Empty;

        [Range(0.01, 100000)] // ensures price is valid and not zero/negative
        public decimal Price { get; set; }

        [StringLength(50)]
        public string? SocketType { get; set; } // used for CPU/Motherboard compatibility

        [StringLength(50)]
        public string? FormFactor { get; set; } // used for motherboard/case compatibility

        public int? Wattage { get; set; } // used for PSU + total power calculations

        public int? CapacityGB { get; set; } // used for RAM/storage components

        public int StockQuantity { get; set; } // inventory tracking

        public bool IsActive { get; set; } = true; // soft delete flag (hidden if false)

        // join relationship: component can appear in many builds
        public ICollection<BuildComponent> BuildComponents { get; set; } = new List<BuildComponent>();
    }
}