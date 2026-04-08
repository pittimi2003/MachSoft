using Microsoft.Extensions.Logging;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.ModelInsert;
using Mss.WorkForce.Code.Models.Models;
using System.Linq.Expressions;

namespace Mss.WorkForce.Code.Alerts
{
    public class TriggerMethods
    {
        private readonly ILogger<TriggerMethods> logger;
        private DataAccess dataAccess;
        public PlanningReturn SimulationPlanning { get; set; }

        public TriggerMethods(DataAccess dataAccess, ILogger<TriggerMethods> logger)
        {
            this.dataAccess = dataAccess;
            this.logger = logger;
        }


        public List<AlertResponse> GetTriggeredAlerts()
        {
            var result = new List<AlertResponse>();

            var configuredAlerts = dataAccess.GetAlerts(SimulationPlanning.WarehouseId);

            if (configuredAlerts == null) return result;

            foreach (var configuredAlert in configuredAlerts)
            {
                try
                {
                    var alertResult = AlertCheck(configuredAlert);
                    if (alertResult != null)
                    {
                        result.AddRange(alertResult);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error checking alert {configuredAlert.Id}. Exception: {ex.ToString()}");
                    continue;
                }
            }

            return result;
        }

        // Checks the if an alert should be triggered
        private List<AlertResponse> AlertCheck(Alert alert)
        {
            var result = new List<AlertResponse>();

            var entity = EntityFind(alert.EntityCode);
            if (entity == null)
            {
                throw new Exception("The given entity does not exist for the current planning");
            }

            entity = Filtering(new List<AlertFilter> { new AlertFilter { FilterField = "IsEstimated", Operator = AlertOperator.Equal, FilterFixedValue = "false", IsFixed = true } }, entity);
            entity = Filtering(dataAccess.GetAlertFilters(alert.Id), entity);

            if (entity == null) return null; // If not found, skip

            var entityType = entity.GetType().GetGenericArguments().First();
            var entityField = entityType.GetProperty(alert.EntityField);
            if (entityField == null)
            {
                throw new Exception("The given field does not exist for the selected entity");
            }

            var filter = new AlertFilter
            {
                FilterField = alert.EntityField,
                Operator = alert.Operator,
                IsFixed = alert.IsFixed,
                FilterFixedValue = alert.FixedValue,
                FilterReference = alert.Reference,
            };

            entity = Filtering(new List<AlertFilter> { filter }, entity);

            foreach (var ent in (IEnumerable<object>)entity)
            {
                var responses = GenerateResponses(alert, ent);
                result.AddRange(responses);
            }

            return result;
        }

        public List<AlertResponse> GenerateResponses(Alert alert, object? entity)
        {
            var entityId = (Guid)entity!
                .GetType()
                .GetProperties()
                .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))!
                .GetValue(entity)!;
            Planning planning = (Planning)entity
                .GetType()
                .GetProperties()
                .FirstOrDefault(p => p.Name.Equals("Planning", StringComparison.OrdinalIgnoreCase))!
                .GetValue(entity)!;

            var results = alert.Configurations.Select(c => new AlertResponse
            {
                Id = new Guid(),
                AlertId = alert.Id,
                Alert = alert,
                PlanningId = SimulationPlanning.Id,
                Planning = SimulationPlanning,
                EntityId = entityId,
                TriggerDate = DateTime.UtcNow,
                Severity = c.Severity,
                Type = c.Type,
            }).ToList();

            return results;
        }


        private object? EntityFind(string entityCode)
        {
            var entity = SimulationPlanning.GetType().GetProperty(entityCode);
            if (entity == null)
            {
                throw new Exception("The entity " + entityCode + " does not exist in the current planning");
            }
            var dbSet = entity.GetValue(SimulationPlanning);
            if (dbSet == null)
            {
                throw new Exception("The entity " + entityCode + " does not exist in the current planning");
            }

            return dbSet;
        }

        private object? Filtering(IEnumerable<AlertFilter> filters, object entity)
        {
            if (filters == null || !filters.Any()) return entity;

            foreach (AlertFilter filter in filters)
            {
                // Obtaining the entity type
                var entityType = entity.GetType().GetGenericArguments().FirstOrDefault()!;
                var field = entityType.GetProperty(filter.FilterField)!;
                var enumerableEntity = entity as IEnumerable<object>;
                // We create a dynamic lambda expression
                var parameter = Expression.Parameter(entityType, "x");
                MemberExpression propertyAccess1 = null;
                object propertyAccess2 = null;
                if (!filter.IsFixed)
                {
                    bool hasErrors = false;
                    var fieldToCompare = entityType.GetProperty(filter.FilterReference!)!;
                    try { propertyAccess1 = Expression.Property(parameter, field); }
                    catch (ArgumentNullException ex)
                    {
                        Console.WriteLine($"[Warn] Entity \"{entityType.Name}\" does not contain property \"{filter.FilterField}\"");
                        hasErrors = true;
                    }

                    try { propertyAccess2 = Expression.Property(parameter, fieldToCompare); }
                    catch (ArgumentNullException ex)
                    {
                        Console.WriteLine($"[Warn] Entity \"{entityType.Name}\" does not contain property \"{filter.FilterReference}\"");
                        hasErrors = true;
                    }

                    if (hasErrors) return null;
                }
                else
                {
                    if (field.PropertyType.Name == nameof(DateTime))
                    {
                        var timeSpanField = typeof(DateTime).GetProperty("TimeOfDay")!;

                        propertyAccess1 = Expression.Property(Expression.Property(parameter, field), timeSpanField);
                        var timeOfDay = Convert.ToDateTime(Expression.Constant(Convert.ChangeType(filter.FilterFixedValue, field.PropertyType)).Value).TimeOfDay;
                        propertyAccess2 = Expression.Constant(timeOfDay, typeof(TimeSpan));
                    }
                    else
                    {
                        propertyAccess1 = Expression.Property(parameter, field);
                        propertyAccess2 = Expression.Constant(Convert.ChangeType(filter.FilterFixedValue, field.PropertyType));
                    }
                }

                Expression comparison = GetComparison(filter, propertyAccess1, propertyAccess2);
                var lambda = Expression.Lambda(comparison, parameter);
                // Apply the dynamic .Where()
                var whereMethod = typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(entityType);
                entity = whereMethod.Invoke(null, new object[] { entity, lambda.Compile() })!;

            }

            return entity;

        }

        private Expression GetComparison(AlertFilter filter, MemberExpression propertyAccess1, object propertyAccess2)
        {
            try
            {
                return filter.Operator switch
                {
                    AlertOperator.GreaterThan => Expression.GreaterThan(propertyAccess1, (Expression)propertyAccess2),
                    AlertOperator.GreaterOrEqual => Expression.GreaterThanOrEqual(propertyAccess1, (Expression)propertyAccess2),
                    AlertOperator.Equal => Expression.Equal(propertyAccess1, (Expression)propertyAccess2),
                    AlertOperator.LessOrEqual => Expression.LessThanOrEqual(propertyAccess1, (Expression)propertyAccess2),
                    AlertOperator.LessThan => Expression.LessThan(propertyAccess1, (Expression)propertyAccess2),
                    AlertOperator.NotEqual => Expression.NotEqual(propertyAccess1, (Expression)propertyAccess2),
                    _ => throw new NotImplementedException($"Operator {filter.Operator} is not supported")
                };
            }
            catch (InvalidOperationException)
            {
                throw new Exception($"Operation {filter.Operator.ToString()} is not valid to compare {propertyAccess1} and {propertyAccess2}");
            }
        }
    }
}
