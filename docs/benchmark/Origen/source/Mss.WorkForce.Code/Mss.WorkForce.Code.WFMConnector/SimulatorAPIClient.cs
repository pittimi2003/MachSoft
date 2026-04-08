using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Newtonsoft.Json;

namespace Mss.WorkForce.Code.WFMConnector
{
    public class SimulatorAPIClient(HttpClient client)
	{
		public async Task<GanttDataConvertDto<TaskData>?> PostMessage(SimulationParametrizedDto requestData, Guid warehouseId, string userFormat)
		{
			try
			{
				client.DefaultRequestHeaders.Add("WarehouseID", warehouseId.ToString());
                client.DefaultRequestHeaders.Add("UserFormat", userFormat);
                var response = await client.PostAsJsonAsync("/", requestData);
				client.DefaultRequestHeaders.Remove("WarehouseID");
                client.DefaultRequestHeaders.Remove("UserFormat");

                GanttDataConvertDto<TaskData>? simResponse = await response.Content.ReadFromJsonAsync<GanttDataConvertDto<TaskData>>();

				return simResponse;

			}

			catch (Exception ex)
			{
				return new GanttDataConvertDto<TaskData>();
			}
		}

        public async Task<DataResponseSimulation?> GetMessage(Guid WarehouseId)
		{
			try
			{
                using var request = new HttpRequestMessage(HttpMethod.Get, "/");
                request.Headers.Add("WarehouseID", WarehouseId.ToString());
                
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<DataResponseSimulation>();
			}
			catch (Exception ex)
			{

				return new DataResponseSimulation();
			}
		}

        public async Task<Guid?> GetMessage2(Guid WarehouseId, SimulationCase simCase)
        {
            try
            {
                client.DefaultRequestHeaders.Add("WarehouseID", WarehouseId.ToString());
                client.DefaultRequestHeaders.Add("Simulation-Case", simCase.ToString());

                var response = await client.GetAsync("/");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Guid>();
            }
            catch (Exception ex)
            {

                return new Guid();
            }
            finally
            {
                client.DefaultRequestHeaders.Remove("WarehouseID");
                client.DefaultRequestHeaders.Remove("Simulation-Case");
            }
        }





    }
}
