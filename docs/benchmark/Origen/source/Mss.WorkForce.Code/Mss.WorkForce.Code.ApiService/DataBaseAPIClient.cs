using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Models.ModelUpdate;
using Mss.WorkForce.Code.Models.Resources;
using System.Net;
using System.Text;

namespace Mss.WorkForce.Code.ApiService
{
    public class DataBaseAPIClient(HttpClient client)
	{
		public async Task<string?> PostSaveConfig(string dataBaseRequest)
		{
			try
			{
                var content = new StringContent(dataBaseRequest, Encoding.UTF8);
                var response = await client.PostAsync("/config", content);

                string? databaseResponse = await response.Content.ReadFromJsonAsync<string>();

                return databaseResponse;
			}

			catch (Exception ex)
			{
				return string.Empty;
			}
		}

        public async Task PostSavePreview(string dataBaseRequest, Guid warehouseId)
        {
            try
            {
                client.DefaultRequestHeaders.Add("WarehouseID", warehouseId.ToString());
                var content = new StringContent(dataBaseRequest, Encoding.UTF8);
                var response = await client.PostAsync("/Scenarioplanner", content);
                client.DefaultRequestHeaders.Remove("WarehouseID");              
            }

            catch (Exception ex)
            {
				throw ex;
            }
        }

        public async Task<DataResponseSimulation> PostSaveSimulation(DataResponseSimulation responseSimulation, SimulationCase simCase = SimulationCase.Planning)
		{
			try
			{
                client.DefaultRequestHeaders.Add("Simulation-Case", simCase.ToString());
                var response = await client.PostAsJsonAsync("/result", responseSimulation);
                client.DefaultRequestHeaders.Remove("Simulation-Case");
                DataResponseSimulation? databaseResponse = await response.Content.ReadFromJsonAsync<DataResponseSimulation>();

                return databaseResponse;

            }

			catch (Exception ex)
			{
                return new DataResponseSimulation();
            }
		}

        public async Task<DataSimulatorTablaRequest> UpdateInputOrderStatus(InputOrderStatusChangesInformation statusChangesInformation)
        {
            try
            {
                var response = await client.PostAsJsonAsync("/status", statusChangesInformation);

                DataSimulatorTablaRequest? databaseResponse = await response.Content.ReadFromJsonAsync<DataSimulatorTablaRequest>();

                return databaseResponse;

            }

            catch (Exception ex)
            {
                return new DataSimulatorTablaRequest();
            }
        }

        public async Task<BasicRequest?> GetMessage(string? id)
		{
			try
			{
                var url = string.IsNullOrEmpty(id) ? "/" : $"/?id={id}";
                var response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode();

				return await response.Content.ReadFromJsonAsync<BasicRequest>();
			}
			catch (Exception ex)
			{
				
				return new BasicRequest();
			}
		}

        public async Task<Dictionary<string, List<ResourceMessage>>> GetConfigCheck(Guid warehouseId)
        {
            try
            {
                client.DefaultRequestHeaders.Add("WarehouseID", warehouseId.ToString());
                var response = await client.GetAsync("/configurationCheck");
                response.EnsureSuccessStatusCode();
                client.DefaultRequestHeaders.Remove("WarehouseID");
                return await response.Content.ReadFromJsonAsync<Dictionary<string, List<ResourceMessage>>>();

            }
            catch (Exception ex)
            {
                return new Dictionary<string, List<ResourceMessage>>();
            }
        }


        public async Task<Guid> GetClone(Guid LayoutId)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("LayoutId", LayoutId.ToString());
                var response = await client.GetAsync("/clone");
                response.EnsureSuccessStatusCode();
                client.DefaultRequestHeaders.Remove("LayoutId");
                return (await response.Content.ReadFromJsonAsync<Guid?>()) ?? Guid.Empty;
            }
            catch (Exception ex)
            {
                return (Guid.Empty);
            }
        }

        public async Task BlockOrder(string body)
        {
            try
            {
                var content = new StringContent(body, Encoding.UTF8);
                var response = await client.PostAsync("/blockOrder", content);
                response.EnsureSuccessStatusCode();
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ChangePriority(string body, Guid warehouseId)
        {
            client.DefaultRequestHeaders.Add("WarehouseID", warehouseId.ToString());
            var content = new StringContent(body, Encoding.UTF8);
            var response = await client.PostAsync("/changepriority", content);
            client.DefaultRequestHeaders.Remove("WarehouseID");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                switch (response.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new ArgumentException(errorMessage);
                    case HttpStatusCode.InternalServerError:
                        throw new Exception(errorMessage);
                    default:
                        throw new Exception($"Unexpected error: {(int)response.StatusCode} - {errorMessage}");
                }
            }
        }

        public async Task<HttpResponseMessage> CancelOrder(string body)
        {
            var content = new StringContent(body, Encoding.UTF8);
            var response = await client.PostAsync("/cancelOrder", content);
            response.EnsureSuccessStatusCode();
            return response;
        }
        public async Task<DataResponseSimulation> AlertCheck(DataResponseSimulation responseSimulation)
        {
            try
            {
                var response = await client.PostAsJsonAsync("/alertcheck", responseSimulation);
                DataResponseSimulation? databaseResponse = await response.Content.ReadFromJsonAsync<DataResponseSimulation>();

                return databaseResponse;

            }

            catch (Exception ex)
            {
                return new DataResponseSimulation();
            }
        }

        public async Task<Guid> GetLastPlanning(Guid warehouseId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/getLastPlanning");
                request.Headers.Add("WarehouseID", warehouseId.ToString());

                var response = await client.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<Guid>();

                return result;
            }
            catch
            {
                return Guid.Empty;
            }

        }

    }
}
