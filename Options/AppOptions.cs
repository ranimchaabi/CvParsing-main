namespace CvParsing.Options;

public class AppOptions
{
    public const string SectionName = "App";

    /// <summary>Public site URL used in emails (e.g. https://jobs.example.com). Falls back to current request if empty.</summary>
    public string PublicBaseUrl { get; set; } = "";
}
