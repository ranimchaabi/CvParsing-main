namespace CvParsing.Models.ViewModels;

public class ProfilePageViewModel
{
    public string NomComplet { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Telephone { get; set; }
    public string? Departement { get; set; }
    public string? Designation { get; set; }
    public string? Langues { get; set; }
    public string? Bio { get; set; }
    public string? PhotoUrl { get; set; }

    public IReadOnlyList<string> DepartementOptions { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> DesignationOptions { get; set; } = Array.Empty<string>();

    public IReadOnlyList<ApplicationRowViewModel> Applications { get; set; } = Array.Empty<ApplicationRowViewModel>();
    public int ApplicationsTotal { get; set; }
    public int ApplicationsPage { get; set; } = 1;
    public int ApplicationsPageSize { get; set; } = 8;
    public int ApplicationsTotalPages => Math.Max(1, (int)Math.Ceiling(ApplicationsTotal / (double)ApplicationsPageSize));
    public string? StatusFilter { get; set; }
}

public class ApplicationRowViewModel
{
    public int CvId { get; set; }
    public int OffreId { get; set; }
    public string TitrePoste { get; set; } = "";
    public string DepartementOuEntreprise { get; set; } = "";
    public DateTime? DateCandidature { get; set; }
    /// <summary>Accepted, Rejected, or Pending</summary>
    public string Statut { get; set; } = "Pending";
}
