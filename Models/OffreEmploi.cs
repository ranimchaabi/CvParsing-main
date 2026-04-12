using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CvParsing.Models;

[Table("OffreEmploi")]
public class OffreEmploi
{
    [Key]
    public int id { get; set; }
    public string? titre { get; set; }
    public string? description { get; set; }
    public string? departement { get; set; }
    public string? type { get; set; }
    public int? experience { get; set; }
    public string? niveau_education { get; set; }
    public string? statut { get; set; }
    public DateTime? date_creation { get; set; }
    public int? id_responsable { get; set; }
}