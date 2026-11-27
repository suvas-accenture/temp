using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone.DAL.Models
{
    [Table("ServiceRequest")]
    public class ServiceRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [Required]
        [StringLength(50)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Details { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RaisedBy { get; set; } = string.Empty;

        [Required]
        public DateTime RaisedOn { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Justification { get; set; } = string.Empty;

        [Required]
        public int ReqStatus { get; set; }

        [ForeignKey(nameof(RaisedBy))]
        public User? RaisedByUser { get; set; }

        [ForeignKey(nameof(ReqStatus))]
        public Status? Status { get; set; }
    }
}
