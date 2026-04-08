using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.ModelSimulation;

namespace Mss.WorkForce.Code.ApiService
{
    public class SimulatorAPIClient(HttpClient client)
	{
		public async Task<PreviewData?> PostMessage(PreviewDto requestData, Guid warehouseId, string userFormat)
		{
			try
			{
				client.DefaultRequestHeaders.Add("WarehouseID", warehouseId.ToString());
                client.DefaultRequestHeaders.Add("UserFormat", userFormat);
                var response = await client.PostAsJsonAsync("/", requestData);
				client.DefaultRequestHeaders.Remove("WarehouseID");
                client.DefaultRequestHeaders.Remove("UserFormat");

                PreviewData? simResponse = await response.Content.ReadFromJsonAsync<PreviewData>();
                return simResponse;

			}

			catch (Exception ex)
			{
                client.DefaultRequestHeaders.Remove("WarehouseID");
                client.DefaultRequestHeaders.Remove("UserFormat");
                return new PreviewData();
			}
		}

        public async Task<List<SimulationLogCheck>?> PostMessage(PreviewDto requestData, Guid warehouseId)
        {
            try
            {
                client.DefaultRequestHeaders.Add("WarehouseID", warehouseId.ToString());
                var response = await client.PostAsJsonAsync("/previewLog", requestData);
                client.DefaultRequestHeaders.Remove("WarehouseID");

                List<SimulationLogCheck>? simResponse = await response.Content.ReadFromJsonAsync<List<SimulationLogCheck>>();
                return simResponse;

            }

            catch (Exception ex)
            {
                return new List<SimulationLogCheck>();
            }
        }

        public async Task<List<WorkerWhatIf>?> PostMessageWhatIf(PreviewDto requestData, Guid warehouseId)
        {
            try
            {
                client.DefaultRequestHeaders.Add("WarehouseID", warehouseId.ToString());
                var response = await client.PostAsJsonAsync("/whatIf", requestData);
                client.DefaultRequestHeaders.Remove("WarehouseID");

                List<WorkerWhatIf>? simResponse = await response.Content.ReadFromJsonAsync<List<WorkerWhatIf>>();
                return simResponse;

            }

            catch (Exception ex)
            {
                return new List<WorkerWhatIf>();
            }
        }

        public async Task<DataResponseSimulation?> GetMessage(Guid WarehouseId)
		{
			try
			{
                client.DefaultRequestHeaders.Add("WarehouseID", WarehouseId.ToString());
                
                var response = await client.GetAsync("/");
				response.EnsureSuccessStatusCode();
                client.DefaultRequestHeaders.Remove("WarehouseID");



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
