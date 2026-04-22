using System.ComponentModel.DataAnnotations;

namespace BuildForgeApp.Models
{
    public class PcComponent
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ComponentType { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [StringLength(50)]
        public string? SocketType { get; set; }

        [StringLength(50)]
        public string? FormFactor { get; set; }

        public int? Wattage { get; set; }

        public int? CapacityGB { get; set; }

        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<BuildComponent> BuildComponents { get; set; } = new List<BuildComponent>();
    }
}
