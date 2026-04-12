using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CvParsing.Models;

[Table("Utilisateur")]
public class Utilisateur
{
    [Key]
    public int id { get; set; }
    public string? nom_utilisateur { get; set; }
    public string? mot_passe { get; set; }
    public string? email { get; set; }
    public DateTime? date_creation { get; set; }
    public DateTime? date_derniere_connexion { get; set; }
}