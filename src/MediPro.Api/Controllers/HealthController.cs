using Microsoft.AspNetCore.Mvc;

namespace MediPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "MediPro.Api",
            timestampUtc = DateTime.UtcNow
        });
    }
}
