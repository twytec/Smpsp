using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Smpsp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingSignInController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}
