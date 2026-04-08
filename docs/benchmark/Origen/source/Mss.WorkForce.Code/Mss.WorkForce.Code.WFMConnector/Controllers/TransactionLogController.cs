using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.WFMConnector.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionLogController : ControllerBase
    {

        private readonly HttpClient dbHttpClient;

        public TransactionLogController(IHttpClientFactory factory)
        {
            dbHttpClient = factory.CreateClient("DataBaseClient");
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(Transaction transaction)
        {
            try
            {
                if (transaction == null) return NoContent();

                var DataBaseService = new DataBaseAPIClient(dbHttpClient);

                bool result = await DataBaseService.AddTransanction(transaction);

                if (result) return Ok(result);
                else return Problem("Error while trying to register an entry into TransactionLog");
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
