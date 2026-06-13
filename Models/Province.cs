using System.ComponentModel.DataAnnotations;

namespace Webbanhang.Models
{
    public class Province
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public ICollection<District> Districts { get; set; } = new List<District>();
    }
}
