using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CvParsing.Models;

[Table("Candidat")]
public class Candidat
{
    [Key]
    public int id { get; set; }

    public string? telephone { get; set; }
    public string? departement { get; set; }
    public string? designation { get; set; }
    public string? langues { get; set; }
    public string? bio { get; set; }
    public string? photo_chemin { get; set; }

    [ForeignKey("id")]
    public Utilisateur? Utilisateur { get; set; }
}