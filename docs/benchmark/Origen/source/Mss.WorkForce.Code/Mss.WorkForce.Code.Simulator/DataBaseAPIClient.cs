using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.MetricssModel;
using Mss.WorkForce.Code.Models.ModelSimulation;

namespace Mss.WorkForce.Code.Simulator
{
    public class DataBaseAPIClient(HttpClient client)
    {
        public async Task<BasicRequest?> PostMessage(string? json)
        {
            try
            {
                var configRequest = new BasicRequest { Json = json };
                var response = await client.PostAsJsonAsync("/", configRequest);

                BasicRequest? configResponse = await response.Content.ReadFromJsonAsync<BasicRequest>();

                return configResponse;

            }

            catch (Exception ex)
            {
                return new BasicRequest();
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

        public async Task<DataSimulatorTablaRequest?> GetMessageSim(Guid warehouseId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/sim");
                request.Headers.Add("WarehouseID", warehouseId.ToString());

                var response = await client.SendAsync(request);

                DataSimulatorTablaRequest? configResponse = await response.Content.ReadFromJsonAsync<DataSimulatorTablaRequest>();

                return configResponse;

            }

            catch (Exception ex)
            {
                return new DataSimulatorTablaRequest();
            }
        }


        public async Task<DataSimulatorTablaRequest?> PostMessageSim(PreviewDto? requestData, Guid warehouseId)
        {
            try
            {
                client.DefaultRequestHeaders.Add("WarehouseID", warehouseId.ToString());
                var response = await client.PostAsJsonAsync("/sim", requestData);
                client.DefaultRequestHeaders.Remove("WarehouseID");

                DataSimulatorTablaRequest? configResponse = await response.Content.ReadFromJsonAsync<DataSimulatorTablaRequest>();

                return configResponse;

            }

            catch (Exception ex)
            {
                return new DataSimulatorTablaRequest();
            }
        }


        public async void PostMetrics(PerformanceMetrics? requestData)
        {
            try
            {
                //var response = await client.PostAsJsonAsync("/metrics", requestData);
            }
            catch (Exception ex)
            {
            }
        }


    }
}
