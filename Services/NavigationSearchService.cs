using System.Text;
using CvParsing.Models.Dtos;
using FuzzySharp;

namespace CvParsing.Services;

/// <summary>In-memory navigation index + fuzzy ranking (FuzzySharp). No database.</summary>
public sealed class NavigationSearchService
{
    private const int MinScore = 52;
    private const int MaxResults = 5;

    private static readonly NavSearchItem[] Index =
    {
        new("Dashboard", "/Home/Index"),
        new("Accueil", "/Home/Index"),
        new("Home", "/Home/Index"),
        new("About", "/Home/Index"),
        new("Offre Emploi", "/Offre/Index"),
        new("Offre", "/Offre/Index"),
        new("Emploi", "/Offre/Index"),
        new("Jobs", "/Offre/Index"),
        new("Profile", "/Profile"),
        new("Profil", "/Profile"),
        new("Mon profil", "/Profile"),
        new("Contact", "/Contact"),
        new("Connexion", "/Account/Login"),
        new("Login", "/Account/Login"),
        new("Inscription", "/Account/Register"),
        new("Register", "/Account/Register"),
        new("Mot de passe oublié", "/Account/ForgotPassword"),
        new("Forgot password", "/Account/ForgotPassword"),
    };

    public NavSearchResponseDto Search(string? query)
    {
        var raw = (query ?? "").Trim();
        var q = Normalize(query);
        if (q.Length == 0)
            return new NavSearchResponseDto { Results = Array.Empty<NavSearchResultDto>() };

        var scored = new List<NavSearchResultDto>(Index.Length);
        foreach (var item in Index)
        {
            var nameNorm = Normalize(item.Name);
            var score = Score(q, nameNorm);
            if (score < MinScore)
                continue;

            scored.Add(new NavSearchResultDto
            {
                Name = item.Name,
                Route = item.Route,
                Score = score,
                Segments = BuildHighlightSegments(item.Name, raw)
            });
        }

        var bestPerRoute = scored
            .GroupBy(r => r.Route, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(x => x.Score).ThenBy(x => x.Name.Length).First())
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Name.Length)
            .Take(MaxResults)
            .ToList();

        return new NavSearchResponseDto { Results = bestPerRoute };
    }

    private static int Score(string queryNorm, string nameNorm)
    {
        if (nameNorm.Length == 0)
            return 0;

        var weighted = Fuzz.WeightedRatio(queryNorm, nameNorm);
        var partial = Fuzz.PartialRatio(queryNorm, nameNorm);
        var tokenSet = Fuzz.TokenSetRatio(queryNorm, nameNorm);
        var tokenSort = Fuzz.TokenSortRatio(queryNorm, nameNorm);
        return Math.Max(Math.Max(weighted, partial), Math.Max(tokenSet, tokenSort));
    }

    private static string Normalize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return "";

        var sb = new StringBuilder(s.Length);
        var prevSpace = false;
        foreach (var ch in s.Trim().ToLowerInvariant())
        {
            var isSpace = char.IsWhiteSpace(ch);
            if (isSpace)
            {
                if (!prevSpace)
                    sb.Append(' ');
                prevSpace = true;
            }
            else
            {
                prevSpace = false;
                sb.Append(ch);
            }
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Highlights a subsequence of letters in <paramref name="name"/> matching the query letters in order (spaces ignored in query for alignment).
    /// </summary>
    private static IReadOnlyList<NavSearchSegmentDto> BuildHighlightSegments(string name, string rawQuery)
    {
        if (string.IsNullOrEmpty(name))
            return Array.Empty<NavSearchSegmentDto>();

        var qLetters = string.Concat(rawQuery.Where(c => !char.IsWhiteSpace(c)));
        if (qLetters.Length == 0)
            return new[] { new NavSearchSegmentDto(name, false) };

        var hl = new bool[name.Length];
        var qi = 0;
        for (var ni = 0; ni < name.Length && qi < qLetters.Length; ni++)
        {
            if (char.IsWhiteSpace(name[ni]))
                continue;

            if (char.ToLowerInvariant(name[ni]) == char.ToLowerInvariant(qLetters[qi]))
            {
                hl[ni] = true;
                qi++;
            }
        }

        if (qi < qLetters.Length)
            return new[] { new NavSearchSegmentDto(name, false) };

        var segments = new List<NavSearchSegmentDto>();
        for (var i = 0; i < name.Length;)
        {
            var on = hl[i];
            var j = i + 1;
            while (j < name.Length && hl[j] == on)
                j++;
            segments.Add(new NavSearchSegmentDto(name.Substring(i, j - i), on));
            i = j;
        }

        return segments;
    }

    private readonly record struct NavSearchItem(string Name, string Route);
}
