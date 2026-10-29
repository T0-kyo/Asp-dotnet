using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using UrlShortner.Services;
using UrlShortner.Settings;
using UrlShortner.DTOs;


namespace UrlShortner.Controllers;


[ApiController]
[Route("api/v1/urls")]
[EnableRateLimiting("fixed_window")]
public class UrlController : ControllerBase
{
    private readonly IUrlService _urlService;
    private readonly IOptions<FilterSettings> _options;


    public UrlController(IUrlService urlService, IOptions<FilterSettings> options)
    {
        _urlService = urlService;
        _options = options;
    }

    [ProducesResponseType(StatusCodes.Status302Found)]
    [HttpGet("{url}")]
    public async Task<IActionResult> GetUrlAsync(string url) => Redirect(await _urlService.GetOriginalUrlAsync(url));

    [HttpGet("admin")]
    public async Task<IActionResult> GetAllUrlsAsync(string? sortBy, string? creator, string? category, int page = 1, int pageSize = 10)
    {
        pageSize = Math.Min(pageSize, _options.Value.MaxPageSize);

        return Ok(await _urlService.GetAllUrlsAsync(sortBy, creator, category, page, pageSize));
    }

    [HttpPost]
    public async Task<IActionResult> AddUrlAsync(AddUrlDto dto)
    {
        var uri = await _urlService.AddUrlAsync(dto);

        return Created($"api/v1/urls/{uri.GeneratedUrl}", uri);
    }

    [HttpDelete("admin/{url}")]
    public async Task<IActionResult> DeleteUrlAsync(string url)
    {
        await _urlService.RemoveUrlAsync(url);

        return NoContent();
    }
}