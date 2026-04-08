using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Services.Interfaces;
using Newtonsoft.Json;

namespace Mss.WorkForce.Code.Web.Services
{
    public class PivotGridService : IPivotGridService
    {
        #region Fields

        private readonly DataAccess _dataAccess;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        #endregion

        #region Constructors

        public PivotGridService(IHttpClientFactory httpClientFactory, DataAccess dataAccess, ILogger<IPivotGridService> logger)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Methods

        public IEnumerable<ItemPlanning> GetPlanningData(Guid planningID)
        {
            IEnumerable<ItemPlanning> resp = new List<ItemPlanning>();
            try
            {
                resp =_dataAccess.GetPlannings(planningID);
            }
            catch (Exception ex) {
                _logger.LogWarning($"{nameof(PivotGridService)} => GetPlanningData {ex.Message}");
            }

            return resp;
        }

        public async Task<IEnumerable<ItemPlanning>> GetPlanningForWarehouse(Guid warehouseID)
        {
            var client = _httpClientFactory.CreateClient(Constants.ApiClientName);
            client.DefaultRequestHeaders.Add("WarehouseID", warehouseID.ToString());
            client.DefaultRequestHeaders.Add("Simulation-Case", SimulationCase.Planning.ToString());
            IEnumerable<ItemPlanning> resp = new List<ItemPlanning>();

            try
            {
                var response = await client.GetAsync("simulate");
                response.EnsureSuccessStatusCode();
                string planningString = await response.Content.ReadAsStringAsync();
                Guid planningID = JsonConvert.DeserializeObject<Guid>(planningString);

                resp = _dataAccess.GetPlannings(planningID);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"{nameof(PivotGridService)} => GetPlanningForWarehouse {ex.Message}");
            }

            return resp;
        }

        #endregion
    }
}