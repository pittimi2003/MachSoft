using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.WMSCommunications;
using Mss.WorkForce.Code.WFMConnector.Controllers.ModelBinders;

namespace Mss.WorkForce.Code.WFMConnector.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {

        private readonly HttpClient dbHttpClient;

        public WarehouseController(IHttpClientFactory factory)
        {
            dbHttpClient = factory.CreateClient("DataBaseClient");
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetWarehouse([FromUnescapedHeader(Name = "WarehouseCode")] string warehouseCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(warehouseCode)) return BadRequest("Invalid or missing WarehouseCode");

                var DataBaseService = new DataBaseAPIClient(dbHttpClient);

                var warehouse = await DataBaseService.GetWarehouse(warehouseCode);

                return Ok(warehouse);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("lastupdateprocesses")]
        public async Task<IActionResult> GetLastUpdateProcess([FromUnescapedHeader(Name = "WarehouseCode")] string warehouseCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(warehouseCode)) return BadRequest("Invalid or missing WarehouseCode");

                var DataBaseService = new DataBaseAPIClient(dbHttpClient);

                var lastUpdate = await DataBaseService.GetLastUpdateWarehouseProcess(warehouseCode);

                return Ok(lastUpdate);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpPost("notifyprocesses")]
        public async Task<IActionResult> NotifyProcesses([FromHeader(Name = "UserName")] string userName, [FromHeader(Name = "Sender")] string sender, List<WarehouseProcessClosingCommunicaation> warehouseProcesses)
        {
            try
            {
                if (warehouseProcesses == null || warehouseProcesses.Count == 0)
                    return NoContent();

                var DataBaseService = new DataBaseAPIClient(dbHttpClient);

               await DataBaseService.UpdateWarehouseProcesss(warehouseProcesses, userName, sender);

                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
