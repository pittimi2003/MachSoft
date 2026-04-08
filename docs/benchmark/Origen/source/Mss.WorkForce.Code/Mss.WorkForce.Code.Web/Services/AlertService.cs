using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Model;
using Mss.WorkForce.Code.Web.Services.Interfaces;
using System.Reflection;
using Mss.WorkForce.Code.Web.Common;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web.Components.Pages.Planning;
using System.Text.Json;

namespace Mss.WorkForce.Code.Web.Services
{
    public class AlertService : IAlertService
    {

        #region Fields

        private readonly DataAccess _dataAccess;
        private readonly ILogger _logger;
        private readonly ISimulateService _simulateService;
        private readonly ILocalizationService _localizationService;
        private readonly IServiceScopeFactory _scopeFactory;

        #endregion

        #region Constructors

        public AlertService(
            DataAccess dataAccess,
            ISimulateService simulateService,
            ILogger<AlertService> logger,
            ILocalizationService localizationService,
            IServiceScopeFactory scopeFactory)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
            _localizationService = localizationService;
            _scopeFactory = scopeFactory;
        }

        #endregion

        #region Methods

        public async Task AddAlert(AlertDto alert)
        {
            try
            {
                OperationDB operation = new OperationDB();
                var entity = MapperAlertManagament(alert);
                entity.CreationDate = DateTime.UtcNow;
                operation.AddNew("Alerts", entity);

                foreach (var alertFilter in alert.AlertFilters)
                {
                    AlertFilter alertFilterEntity = new()
                    {
                        Id = alertFilter.Id,
                        AlertId = alert.Id,
                        Operator = alertFilter.Operator,
                        FilterField = alertFilter.FilterField,
                        FilterReference = alertFilter.FilterReference,
                        FilterFixedValue = alertFilter.FilterFixedValue,
                        IsFixed = alertFilter.IsFixed,
                    };

                    operation.AddNew("AlertFilters", alertFilterEntity);
                }

                foreach (var aConf in alert.Configurations)
                {

                    AlertConfiguration aConfEntity = new()
                    {
                        Id = aConf.Id,
                        AlertId = alert.Id,
                        Severity = aConf.Severity,
                        Type = (AlertType)aConf.Type,
                    };

                    //operation.AddNew("AlertConfigurations", aConfEntity);
                    operation.AddNew("AlertConfigurations", aConfEntity);
                }

                await _simulateService.SaveChangesInDataBase(operation);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AlertService::AddUser => Error updating alert to database");
                throw;
            }
        }

        public async Task DeleteAlert(List<AlertDto> alerts)
        {
            try
            {
                OperationDB operation = new OperationDB();

                foreach (var item in alerts)
                    operation.AddDelete("Alerts", new EntityDto() { Id = item.Id });

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<AlertDto> GetAlertManagement()
        {
            List<AlertDto> alerts = new();

            foreach (var alertModel in _dataAccess.GetAlertManagement().ToList())
            {
                alerts.Add(MapperAlertManagament(alertModel));
            }

            alerts = alerts.OrderBy(x => x.Warehouse?.Name ?? string.Empty).ToList();

            return alerts;
        }

        public async Task<IEnumerable<AlertMessageDto>> GetAlertNotificationsByPlanningAsync(Guid IdPlanning)
        {
            List<AlertMessageDto> alertMessages = new();
            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                    var ScopeDataAccess = scope.ServiceProvider.GetRequiredService<DataAccess>();


                    var alertCollection = ScopeDataAccess.GetAllAlertResponsesByPlanningId(IdPlanning).Where(a => a.Type != AlertType.Email).ToList();
                    var workerOrderPlanningIds = alertCollection.Select(a => a.EntityId).Distinct().ToList();
                    var workerOrderPlanningCollection = ScopeDataAccess.GetWorkOrderPlanningsByIds(workerOrderPlanningIds).ToDictionary(p => p.Id);
                    foreach (var alertNotify in alertCollection)
                    {
                        var alertDto = AlertResponseToAlertMessage(alertNotify, workerOrderPlanningCollection);
                        if (alertDto != null)
                            alertMessages.Add(alertDto);
                    }
                    return alertMessages;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AlertService::GetAlertNotificationsByPlanningAsync - Error retrieving or processing bell type alerts for all warehouses.");
                    return Enumerable.Empty<AlertMessageDto>();
                }
            }
        }


        public async Task<IEnumerable<AlertMessageDto>> GetBellAlertsForAllWarehousesAsync()
        {
            List<AlertMessageDto> alertMessages = new();

            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                    var ScopeDataAccess = scope.ServiceProvider.GetRequiredService<DataAccess>();
                    var alertCollection = ScopeDataAccess.GetAllAlertResponses().Where(a => a.Type == AlertType.Bell).ToList();
                    var workerOrderPlanningIds = alertCollection.Select(a => a.EntityId).Distinct().ToList();
                    var workerOrderPlanningCollection = ScopeDataAccess.GetWorkOrderPlanningsByIds(workerOrderPlanningIds).ToDictionary(p => p.Id);

                    foreach (var alert in alertCollection)
                    {
                        var alertDto = AlertResponseToAlertMessage(alert, workerOrderPlanningCollection);
                        if (alertDto != null)
                            alertMessages.Add(alertDto);
                    }
                    return alertMessages;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AlertService::GetBellAlertsForAllWarehousesAsync - Error retrieving or processing bell type alerts for all warehouses.");
                    return Enumerable.Empty<AlertMessageDto>();
                }
            }

        }

        public async Task UpdateAlert(List<AlertDto> alerts)
        {
            try
            {
                OperationDB operation = new OperationDB();
                foreach (AlertDto alert in alerts)
                {
                    var entity = MapperAlertManagament(alert);
                    entity.UpdateDate = DateTime.UtcNow;
                    operation.AddUpdate("Alerts", entity);

                    _dataAccess.AlertFilterRemoveAll(alert.Id);

                    foreach (var alertFilter in alert.AlertFilters)
                    {

                        AlertFilter alertFilterEntity = new()
                        {
                            Id = alertFilter.Id,
                            AlertId = alert.Id,
                            Operator = alertFilter.Operator,
                            FilterField = alertFilter.FilterField,
                            FilterReference = alertFilter.FilterReference,
                            FilterFixedValue = alertFilter.FilterFixedValue,
                            IsFixed = alertFilter.IsFixed,
                        };


                        operation.AddNew("AlertFilters", alertFilterEntity);
                    }

                    _dataAccess.AlertConfigurationRemoveAll(alert.Id);

                    foreach (var aConf in alert.Configurations)
                    {

                        AlertConfiguration aConfEntity = new()
                        {
                            Id = aConf.Id,
                            AlertId = alert.Id,
                            Severity = aConf.Severity,
                            Type = (AlertType)aConf.Type,
                        };

                        //operation.AddNew("AlertConfigurations", aConfEntity);
                        _dataAccess.AddAlertConfiguration(aConfEntity);
                    }
                }

                await _simulateService.SaveChangesInDataBase(operation);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AlertService::UpdateAlert => Error adding alert to database");
                throw;
            }
        }

        private AlertMessageDto AlertResponseToAlertMessage(AlertResponse alertNotify, IDictionary<Guid, WorkOrderPlanning> workerOrdersPlanning)
        {
            var basePath = AppContext.BaseDirectory;
            var jsonPath = Path.Combine(basePath, "Data", "TaskDataDescription.json");

            var message = new AlertMessageDto
            {
                Id = alertNotify.Id,
                AlertId = alertNotify.AlertId,
                EntityCode = alertNotify.EntityId.ToString(),
                CreationDate = alertNotify.TriggerDate,
                IsRead = false,
                EntityId = alertNotify.EntityId,
                AlertSeverity = alertNotify.Severity,
                AlertType = alertNotify.Type,
            };

            if (alertNotify.Alert is not null)
            {

                if (!workerOrdersPlanning.TryGetValue(alertNotify.EntityId, out var orderPlanning))
                {
                    _logger.LogError($"AlertService::AlertResponseToAlertMessage => No order for alert response {alertNotify.Id}");
                    return null;
                }

                string conditionValue;

                if (alertNotify.Alert.IsFixed)
                    conditionValue = alertNotify.Alert.FixedValue;
                else
                {
                    conditionValue = alertNotify.Alert.Reference;
                }

                var AllProperties = GetPropertiesAndSubProperties(typeof(WorkOrderPlanning));

                var mapper = new TaskDataMapper(jsonPath);
                var PropertiesValues = GetPropertyValues(orderPlanning, AllProperties);
                var MapperPropertiesValues = mapper.ApplyAliases(PropertiesValues);

                var MessagePropValues = FieldValueFormatter.Format(MapperPropertiesValues);

                if (!string.IsNullOrEmpty(alertNotify.Alert.Message))
                    message.Message = BuildMessage(mapper, alertNotify.Alert.Message, MessagePropValues, alertNotify.Alert.EntityField, EnumHelper.GetItemDescription<AlertOperator>(alertNotify.Alert.Operator), conditionValue);

            }
            else
            {
                _logger.LogError($"AlertService::GetAlertNotificationsByPlanningIdAsync =>  Alert not found {alertNotify.Id}");
                return null;
            }

            return message;
        }


        private List<string> GetPropertiesAndSubProperties(Type type)
        {
            var properties = new List<string>();
            try
            {
                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var propType = prop.PropertyType;
                    if (!propType.IsClass || propType == typeof(string))
                    {
                        properties.Add(prop.Name);
                    }
                    else
                    {
                        foreach (var subProp in propType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            var subType = subProp.PropertyType;
                            if (!subType.IsClass || subType == typeof(string))
                            {
                                properties.Add($"{prop.Name}.{subProp.Name}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                properties = new List<string>();
            }

            return properties;
        }

        private Dictionary<string, string> GetPropertyValues(object target, IEnumerable<string> fields)
        {
            var extras = new Dictionary<string, string>();
            try
            {
                foreach (var fieldPath in fields)
                {
                    object currentObject = target;
                    PropertyInfo property = null;

                    foreach (var part in fieldPath.Split('.'))
                    {
                        if (currentObject == null) break;

                        property = currentObject.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                        if (property == null) break;

                        currentObject = property.GetValue(currentObject);
                    }

                    if (property == null || currentObject == null)
                    {
                        extras[fieldPath] = "(No data)";
                        continue;
                    }

                    var measureAttr = property.GetCustomAttribute<MeasureAttributes>();
                    if (measureAttr != null)
                    {
                        var unit = GetEnumDescription(measureAttr.MeasuresType);
                        extras[fieldPath] = $"{currentObject} {unit}";
                    }
                    else
                    {
                        extras[fieldPath] = currentObject.ToString();
                    }
                }
            }
            catch
            {

            }
            return extras;
        }

        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                 .FirstOrDefault() as DescriptionAttribute;
            return attribute?.Description ?? string.Empty;
        }

        private AlertFilterDto MapperAlertFilter(AlertFilter alertEntity)
        {
            return new AlertFilterDto
            {
                Id = alertEntity.Id,
                Operator = alertEntity.Operator,
                FilterField = alertEntity.FilterField,
                FilterReference = alertEntity.FilterReference,
                FilterFixedValue = alertEntity.FilterFixedValue,
                IsFixed = alertEntity.IsFixed
            };
        }

        private AlertConfigurationDto MapperAlertConfiguration(AlertConfiguration alertEntity)
        {
            return new AlertConfigurationDto
            {
                Id = alertEntity.Id,
                Severity = alertEntity.Severity,
                Type = alertEntity.Type,
            };
        }

        private AlertDto MapperAlertManagament(Alert alertEntity)
        {
            AlertDto alertDto = new()
            {
                Id = alertEntity.Id,
            };

            alertDto.Condition.FilterField = alertEntity.EntityField;
            alertDto.Condition.Operator = alertEntity.Operator;
            alertDto.Condition.FilterReference = alertEntity.Reference;
            alertDto.Condition.FilterFixedValue = alertEntity.FixedValue;
            alertDto.Condition.IsFixed = alertEntity.IsFixed;
            alertDto.Message = alertEntity.Message;


            EntityTypeEnum? entityCode = EnumHelper.ConvertStringToEnum<EntityTypeEnum>(alertEntity.EntityCode);

            if (entityCode.HasValue)
                alertDto.EntityCode = entityCode.Value;

            if (alertDto.Warehouse == null)
                alertDto.Warehouse = new LayoutWarehouseDto();

            if (alertEntity.Warehouse != null)
            {
                alertDto.Warehouse.Id = alertEntity.Warehouse.Id;
                alertDto.Warehouse.Name = alertEntity.Warehouse.Name;
            }

            foreach (var alertFilter in _dataAccess.GetAlertFilters(alertEntity.Id))
                alertDto.AlertFilters.Add(MapperAlertFilter(alertFilter));

            foreach (var aConf in _dataAccess.GetAlertConfigurations(alertEntity.Id))
                alertDto.Configurations.Add(MapperAlertConfiguration(aConf));

            alertDto.CreationDate = alertEntity.CreationDate;
            alertDto.UpdateDate = alertEntity.UpdateDate;

            return alertDto;
        }

        private Alert MapperAlertManagament(AlertDto alertdto)
        {
            Alert alertEntity = new();

            alertEntity.Id = alertdto.Id;
            alertEntity.WarehouseId = (Guid)alertdto.Warehouse.Id;
            alertEntity.EntityCode = alertdto.EntityCode.ToString();
            alertEntity.EntityField = alertdto.Condition.FilterField;
            alertEntity.Operator = alertdto.Condition.Operator;
            alertEntity.Reference = alertdto.Condition.FilterReference;
            alertEntity.FixedValue = alertdto.Condition.FilterFixedValue;
            alertEntity.IsFixed = alertdto.Condition.IsFixed;
            alertEntity.CreationDate = alertdto.CreationDate;
            alertEntity.UpdateDate = alertEntity.UpdateDate;
            alertEntity.Message = alertdto.Message;

            return alertEntity;
        }

        private string BuildMessage(TaskDataMapper mapper, string ConfigMessage, Dictionary<string, string> PropertiesValues, string field, string condition, string reference)
        {
            try
            {
                var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "FieldLabels.json");
                var json = File.ReadAllText(jsonPath);

                var labels = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json)?["WorkOrderPlanning"]
                             ?? new Dictionary<string, string>();

                field = labels.TryGetValue(field, out var fieldLabel) ? fieldLabel : field;
                reference = labels.TryGetValue(reference, out var referenceLabel) ? referenceLabel : reference;

                var traslate = _localizationService.Loc(field);
                ConfigMessage = ConfigMessage
                    .Replace("[Field]", traslate)
                    .Replace("[Operator]", condition)
                    .Replace("[Reference]", reference);

                ConfigMessage = Regex.Replace(ConfigMessage, @"\[(.*?)\]", match =>
                {
                    var key = match.Groups[1].Value;
                    return $"[V_{key}]";
                });

                foreach (var (key, value) in PropertiesValues)
                    ConfigMessage = ConfigMessage.Replace($"[{key}]", value);

                ConfigMessage = Regex.Replace(ConfigMessage, @"\[[^\]]+\]", "(Invalid value)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AlertService::BuildMessageAlert");
            }
            return ConfigMessage;
        }

        public async Task CloneAlert(AlertDto originalAlert, SiteModel newWarehouse)
        {
            if (originalAlert == null || newWarehouse == null)
                return;

            var cloned = (AlertDto)originalAlert.Clone();

            cloned.Id = Guid.NewGuid();
            cloned.Warehouse = new LayoutWarehouseDto
            {
                Id = newWarehouse.Id,
                Name = newWarehouse.Name
            };

            cloned.CreationDate = DateTime.UtcNow;
            cloned.UpdateDate = null;

            foreach (var filter in cloned.AlertFilters)
            {
                filter.Id = Guid.NewGuid();
            }

            foreach (var config in cloned.Configurations)
            {
                config.Id = Guid.NewGuid();
            }

            await AddAlert(cloned);
        }

        #endregion
    }
}