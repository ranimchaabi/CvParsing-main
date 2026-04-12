using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CvParsing.Models;

[Table("Cv")]
public class Cv
{
    [Key]
    public int id { get; set; }
    public int? id_offre { get; set; }
    public string? chemin_fichier { get; set; }
    public DateTime? upload_date { get; set; }
    public int? id_candidat { get; set; }

    /// <summary>Accepted, Rejected, or Pending (displayed as En attente).</summary>
    public string? statut_candidature { get; set; }

    [ForeignKey("id_offre")]
    public OffreEmploi? OffreEmploi { get; set; }

    [ForeignKey("id_candidat")]
    public Candidat? Candidat { get; set; }
}