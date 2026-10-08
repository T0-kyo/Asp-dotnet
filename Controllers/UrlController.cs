using Microsoft.AspNetCore.Mvc;
using UrlShortner.Services;


namespace UrlShortner.Controllers;


[ApiController]
[Route("api/v1/url")]
public class UrlController : ControllerBase
{
    private readonly IUrlService _urlService;


    public UrlController(IUrlService urlService)
    {
        _urlService = urlService;
    }
}