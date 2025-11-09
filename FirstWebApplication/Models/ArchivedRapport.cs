using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirstWebApplication.Models
{
    // ArchivedRapport representerer en arkivert rapport som tilhører en arkivert hindring
    public class ArchivedRapport
    {
        [Key]
        public int ArchivedRapportId { get; set; }

        // FK to ArchivedReport
        [ForeignKey(nameof(ArchivedReport))]
        public int ArchivedReportId { get; set; }

        public ArchivedReport ArchivedReport { get; set; }

        // Original RapportID fra Rapports tabellen
        public int OriginalRapportId { get; set; }

        [MaxLength(1000)]
        public string RapportComment { get; set; } = string.Empty;
    }
}

