using Median.IntranetTemplate.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Median.IntranetTemplate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BaseController : ControllerBase
    {
        protected new ActionResult Ok()
        {
            return base.Ok(Envelope.Ok());
        }

        protected ActionResult Ok<T>(T result)
        {
            return base.Ok(Envelope.Ok(result));
        }
        protected ActionResult Error(List<string> errorMessages)
        {
            string errors = string.Join(";", errorMessages);
            return BadRequest(Envelope.Error(errors));
        }

        protected ActionResult Error(string errorMessage)
        {
            return BadRequest(Envelope.Error(errorMessage));
        }

        protected ActionResult Exception(Exception ex)
        {
            return ServerError($"Server Error: {ex.Message}");
        }

        protected ActionResult ServerError(string errorMessage)
        {
            return StatusCode(500, Envelope.Error(errorMessage));
        }
    }
}
