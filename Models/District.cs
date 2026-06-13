using System.ComponentModel.DataAnnotations;

namespace Webbanhang.Models
{
    public class District
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public int ProvinceId { get; set; }
        public Province? Province { get; set; }

        public ICollection<Ward> Wards { get; set; } = new List<Ward>();
    }
}
