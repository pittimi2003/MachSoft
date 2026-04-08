using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.HeaderEnums;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.WMSCommunications;

namespace Mss.WorkForce.Code.WFMConnector
{
    public class DataBaseAPIClient(HttpClient client)
    {
        public async Task<bool?> CheckConfigAsync(Guid warehouseId)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "/configurationCheckBackground");
                request.Headers.Add("WarehouseID", warehouseId.ToString());

                var response = await client.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<bool>();

                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task WaitForHealthAsync(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var response = await client.GetAsync("/health", token);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[DatabaseAPI] Health OK");
                        return;
                    }

                    Console.WriteLine("[DatabaseAPI] Health NOT ready... retrying");
                }
                catch
                {
                    Console.WriteLine("[DatabaseAPI] Health unreachable... retrying");
                }

                await Task.Delay(1000, token);
            }
        }

        public async Task<DateTime?> GetLastUpdateDateInputOrderProcessesClosing(string warehousecode)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "/getLastUpdateDateInputOrderProcessesClosing");
                request.Headers.Add("WarehouseCode", Uri.EscapeDataString(warehousecode));

                var response = await client.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var result = response.Content.Headers.ContentLength > 0 ? await response.Content.ReadFromJsonAsync<DateTime?>() : null;

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task<DateTime?> GetLastUpdateDateInputOrders(string warehousecode)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "/getLastUpdateDateInputOrders");
                request.Headers.Add("WarehouseCode", Uri.EscapeDataString(warehousecode));

                var response = await client.SendAsync(request);

                response.EnsureSuccessStatusCode();          

                var result = response.Content.Headers.ContentLength > 0 ? await response.Content.ReadFromJsonAsync<DateTime?>() : null;

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

        }

        public async Task<List<ReleaseDateOrder>> GetReleaseDateOrders(string warehousecode)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/getReleaseDateOutboundOrder");
                request.Headers.Add("WarehouseCode", Uri.EscapeDataString(warehousecode));

                var response = await client.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var result = response.Content.Headers.ContentLength > 0 ? await response.Content.ReadFromJsonAsync<List<ReleaseDateOrder>>() : null;

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

        }

        public async Task UpdateInputOrderProcessesClosing(List<InputOrderProcessesClosingCommunication> inputOrderProcessClosingCommunications, string warehouseCode, string? user, string? sender)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "/updateInputOrderProcessesClosing")
                {
                    Content = JsonContent.Create(inputOrderProcessClosingCommunications)
                };

                request.Headers.Add("Warehouse", Uri.EscapeDataString(warehouseCode));
                request.Headers.Add(HeaderEnums.UserName.ToString(), user ?? ConstStrings.UnknownUser);
                request.Headers.Add(HeaderEnums.Sender.ToString(), sender ?? ConstStrings.UnknownSender);

                await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<Guid> UpdateInputOrderStatus(List<InputOrderCommunication> inputOrderCommunications, string? user, string? sender)
        {
            //Update InputOrders with the arrival information

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "/updateInputOrders")
                {
                    Content = JsonContent.Create(inputOrderCommunications)
                };

                request.Headers.Add(HeaderEnums.UserName.ToString(), user ?? ConstStrings.UnknownUser);
                request.Headers.Add(HeaderEnums.Sender.ToString(), sender ?? ConstStrings.UnknownSender);

                var response = await client.SendAsync(request);

                Guid warehouseId = await response.Content.ReadFromJsonAsync<Guid>();

                return warehouseId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Guid.Empty;
            }
        }

        internal async Task<bool> AddTransanction(Transaction transaction)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "/addTransaction")
                {
                    Content = JsonContent.Create(transaction)
                };

                var response = await client.SendAsync(request);

                bool result = await response.Content.ReadFromJsonAsync<bool>();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        internal async Task<Warehouse> GetWarehouse(string warehouseCode)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "/getWarehouse");

                request.Headers.Add("Warehouse", Uri.EscapeDataString(warehouseCode));

                var response = await client.SendAsync(request);

                var result = await response.Content.ReadFromJsonAsync<Warehouse>();

                if (result == null) throw new Exception($"Error trying to get warehouse with code {warehouseCode}. Response: {result}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task<DateTime?> GetLastUpdateWarehouseProcess(string warehousecode)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "/getlastupdatewarehouseprocess");
                request.Headers.Add("WarehouseCode", Uri.EscapeDataString(warehousecode));

                var response = await client.SendAsync(request);

                response.EnsureSuccessStatusCode();

                var result = response.Content.Headers.ContentLength > 0 ? await response.Content.ReadFromJsonAsync<DateTime?>() : null;

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task UpdateWarehouseProcesss(List<WarehouseProcessClosingCommunicaation> warehouseProcessClosingCommunicaations, string? user, string? sender)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "/updatewarehouseprocess")
                {
                    Content = JsonContent.Create(warehouseProcessClosingCommunicaations)
                };

                request.Headers.Add(HeaderEnums.UserName.ToString(), user ?? ConstStrings.UnknownUser);
                request.Headers.Add(HeaderEnums.Sender.ToString(), sender ?? ConstStrings.UnknownSender);

                var response = await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
