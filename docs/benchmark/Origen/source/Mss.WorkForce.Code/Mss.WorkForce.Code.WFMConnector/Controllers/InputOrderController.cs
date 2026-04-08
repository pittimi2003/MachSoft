using Microsoft.AspNetCore.Mvc;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.WMSCommunications;
using Mss.WorkForce.Code.WFMConnector.Controllers.ModelBinders;

namespace Mss.WorkForce.Code.WFMConnector.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InputOrderController : ControllerBase
    {
        private readonly HttpClient dbHttpClient;
        private readonly HttpClient simulatorHttpClient;

        public InputOrderController(IHttpClientFactory factory)
        {
            dbHttpClient = factory.CreateClient("DataBaseClient");
            simulatorHttpClient = factory.CreateClient("SimulatorClient");
        }

        #region get

        [HttpGet("lastupdate")]
        public async Task<IActionResult> GetLastUpdateInputOrders([FromUnescapedHeader(Name = "WarehouseCode")] string warehouseCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(warehouseCode))
                {
                    return BadRequest("Invalid or missing WarehouseCode");
                }

                var DataBaseService = new DataBaseAPIClient(dbHttpClient);

                DateTime? LastUpdateDate = await DataBaseService.GetLastUpdateDateInputOrders(warehouseCode);

                Console.WriteLine($"Last update date for {warehouseCode} in InputOrders: {LastUpdateDate}");

                return Ok(LastUpdateDate);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("lastupdateprocessesclosing")]
        public async Task<IActionResult> GetLastUpdateInputOrderProcessesClosing([FromUnescapedHeader(Name = "WarehouseCode")] string warehouseCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(warehouseCode))
                {
                    return BadRequest("Invalid or missing WarehouseCode");
                }

                var DataBaseService = new DataBaseAPIClient(dbHttpClient);

                DateTime? LastUpdateDate = await DataBaseService.GetLastUpdateDateInputOrderProcessesClosing(warehouseCode);

                Console.WriteLine($"Last update date for {warehouseCode} in InputOrderProcessesClosing: {LastUpdateDate}");

                return Ok(LastUpdateDate);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [HttpGet("releasedateoutboundorder")]
        public async Task<IActionResult> GetReleaseDateOutboundOrder([FromUnescapedHeader(Name = "WarehouseCode")] string warehouseCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(warehouseCode))
                {
                    return BadRequest("Invalid or missing WarehouseCode");
                }

                var DataBaseService = new DataBaseAPIClient(dbHttpClient);

                List<ReleaseDateOrder> releaseDateOutboundOrder = await DataBaseService.GetReleaseDateOrders(warehouseCode!);

                return Ok(releaseDateOutboundOrder);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        #endregion
        #region post

        [HttpPost("notifyorders")]
        public async Task<IActionResult> NotifyOrders([FromHeader(Name = "UserName")] string userName, [FromHeader(Name = "Sender")] string sender, 
            List<InputOrderCommunication> inputOrders)
        {
            try
            {
                if (inputOrders == null || inputOrders.Count == 0)
                    return NoContent();

                var DataBaseService = new DataBaseAPIClient(dbHttpClient);

                // TODO: revisar forma de sacar warehouseId
                Guid warehouseId = await DataBaseService.UpdateInputOrderStatus(inputOrders, userName, sender);

                var simulatorClient = new SimulatorAPIClient(simulatorHttpClient);
                await simulatorClient.GetMessage(warehouseId);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpPost("notifyprocesses")]
        public async Task<IActionResult> NotifyProcesses([FromUnescapedHeader(Name = "WarehouseCode")] string warehouseCode, [FromHeader(Name = "UserName")] string userName, [FromHeader(Name = "Sender")] string sender,
            List<InputOrderProcessesClosingCommunication> proccesses)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(warehouseCode))
                {
                    return BadRequest("Invalid or missing WarehouseCode");
                }

                if (proccesses == null || proccesses.Count == 0)
                    return NoContent();

                var DataBaseService = new DataBaseAPIClient(dbHttpClient);

                await DataBaseService.UpdateInputOrderProcessesClosing(proccesses, warehouseCode, userName, sender);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        #endregion
    }
}
