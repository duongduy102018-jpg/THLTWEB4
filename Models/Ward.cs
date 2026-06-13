using System.ComponentModel.DataAnnotations;

namespace Webbanhang.Models
{
    public class Ward
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public int DistrictId { get; set; }
        public District? District { get; set; }
    }
}
