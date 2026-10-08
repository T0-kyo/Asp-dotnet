using Microsoft.AspNetCore.Mvc;
using UrlShortner.DTOs;
using UrlShortner.Services;


namespace UrlShortner.Controllers;


[ApiController]
[Route("api/v1/urls")]
public class UrlController : ControllerBase
{
    private readonly IUrlService _urlService;


    public UrlController(IUrlService urlService)
    {
        _urlService = urlService;
    }

    [HttpGet("{url}")]
    public async Task<IActionResult> GetUrlAsync(string url) => Redirect(await _urlService.GetOriginalUrlAsync(url));

    [HttpGet]
    public async Task<IActionResult> GetAllUrlsAsync() => Ok(await _urlService.GetAllUrlsAsync());

    [HttpPost]
    public async Task<IActionResult> AddUrlAsync(AddUrlDto dto)
    {
        var uri = await _urlService.AddUrlAsync(dto);

        return Created($"api/v1/urls/{uri.GeneratedUrl}", uri);
    }

    [HttpDelete("{url}")]
    public async Task<IActionResult> DeleteUrlAsync(string url)
    {
        await _urlService.RemoveUrlAsync(url);

        return NoContent();
    }
}