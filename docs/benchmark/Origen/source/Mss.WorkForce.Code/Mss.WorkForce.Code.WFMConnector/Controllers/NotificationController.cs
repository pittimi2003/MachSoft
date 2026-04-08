using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.Models.ConnectorModel;

namespace Mss.WorkForce.Code.WFMConnector.Controllers
{
    [ApiController]
    [Route("notification")]
    public class NotificationController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // Placeholder for future implementation of GET logic.
            return Ok(new { Message = "GET method is not implemented yet." });
        }

        [HttpPost]
        public IActionResult Post([FromBody] NotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Handle the deserialized object (e.g., save to database, process logic, etc.)
            return Ok(new { Message = "Notification received successfully.", NotificationId = request.NotificationId });
        }
    }
}
