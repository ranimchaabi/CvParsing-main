using Microsoft.AspNetCore.Mvc;
using CvParsing.Models.Dtos;
using CvParsing.Services;

namespace CvParsing.Controllers;

[ApiController]
[Route("api")]
public class NavSearchController : ControllerBase
{
    private readonly NavigationSearchService _navigationSearch;

    public NavSearchController(NavigationSearchService navigationSearch)
    {
        _navigationSearch = navigationSearch;
    }

    [HttpGet("nav-search")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public ActionResult<NavSearchResponseDto> Get([FromQuery] string? q) =>
        Ok(_navigationSearch.Search(q));
}
