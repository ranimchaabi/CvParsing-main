using CvParsing.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CvParsing.ViewModels;

public class OffreSearchResultsViewModel
{
    public string? Query { get; set; }
    public string? Departement { get; set; }
    public string? TypeContrat { get; set; }

    public List<SelectListItem> Departements { get; set; } = new();
    public List<SelectListItem> TypesContrat { get; set; } = new();

    public List<OffreEmploi> Results { get; set; } = new();
}

