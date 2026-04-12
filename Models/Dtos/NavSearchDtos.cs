namespace CvParsing.Models.Dtos;

public sealed class NavSearchResultDto
{
    public string Name { get; set; } = "";
    public string Route { get; set; } = "";
    public int Score { get; set; }
    public IReadOnlyList<NavSearchSegmentDto> Segments { get; set; } = Array.Empty<NavSearchSegmentDto>();
}

public sealed class NavSearchSegmentDto
{
    public string Text { get; set; } = "";
    public bool Highlight { get; set; }

    public NavSearchSegmentDto(string text, bool highlight)
    {
        Text = text;
        Highlight = highlight;
    }
}

public sealed class NavSearchResponseDto
{
    public IReadOnlyList<NavSearchResultDto> Results { get; set; } = Array.Empty<NavSearchResultDto>();
}
