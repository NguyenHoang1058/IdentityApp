using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoginAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PlayController : ControllerBase
    {
        [HttpGet("get-player")]
        public IActionResult GetPlayer() {
            return Ok(new JsonResult(new { message = "Only authorize user can view players" }));
        }
    }
}
