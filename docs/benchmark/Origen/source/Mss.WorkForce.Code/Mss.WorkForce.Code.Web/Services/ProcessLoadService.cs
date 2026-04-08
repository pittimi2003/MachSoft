using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Model;

namespace Mss.WorkForce.Code.Web.Services
{
    public static class ProcessLoadService
    {
        /// <summary>
        /// Procesa una lista de órdenes cargadas y filtra aquellas que coinciden con el tipo de operación (entrada o salida).
        /// </summary>
        /// <param name="load">Lista de objetos de tipo OrderSchedule a procesar.</param>
        /// <param name="IsOutput">Indica si el proceso debe filtrar órdenes de salida (true) o entrada (false).</param>
        /// <returns>Una lista de LoadDto con las órdenes procesadas.</returns>
        public static List<LoadDto> Process(List<OrderSchedule> load, bool IsOutput)
        {
            List<LoadDto> orders = new List<LoadDto>();

            foreach (var item in load)
            {
                if (item.IsOut == IsOutput)
                    orders.Add(new LoadDto()
                    {
                        hour = item.InitHour,
                        endHour = item.EndHour,
                        id = item.Id,
                        load = item.Load.Name,
                        loadId = item.Load.Id,
                        numberVehicle = item.NumberVehicles,
                        vehicle = item.Vehicle.Name,
                        vehicleId = item.Vehicle.Id
                    });
            }
            return orders;
        }

        /// <summary>
        /// Procesa una lista de equipos y los convierte en un formato de recursos actualizados.
        /// </summary>
        /// <param name="equipments">Colección de equipos de tipo TypeEquipment a procesar.</param>
        /// <returns>Una lista de ResourceDto que representa los recursos procesados.</returns>
        public static List<ResourceDto> ProcessUpdateEquipments(IEnumerable<TypeEquipment> equipments)
        {
            return equipments.Select(e => new ResourceDto()
            {
                resource = e.Name,
                value = e.Quantity.ToString(),
            }).ToList();
        }

        /// <summary>
        /// Procesa una lista de operadores por proceso y los convierte en un formato de recursos actualizados.
        /// </summary>
        /// <param name="operators">Colección de operadores de tipo OperatorsByProcess a procesar.</param>
        /// <returns>Una lista de ResourceDto que representa los recursos procesados.</returns>
        public static List<ResourceDto> ProcessUpdateWorkers(IEnumerable<OperatorsByProcess> operators)
        {
            return operators.Select(x => new ResourceDto()
            {
                resource = x.Process,
                value = x.Quantity.ToString(),
            }).ToList();
        }

        /// <summary>
        /// Procesa una colección de perfiles de carga y los convierte en una lista de tipos de carga.
        /// </summary>
        /// <param name="profiles">Colección de perfiles de carga de tipo LoadProfile a procesar.</param>
        /// <returns>Una lista de LoadType con los perfiles de carga procesados.</returns>
        public static List<LoadType> ProcessLoadTypes(IEnumerable<LoadProfile> profiles)
        {
            return profiles.Select(x => new LoadType()
            {
                Id = x.Id,
                Name = x.Name,
            }).ToList();
        }

        /// <summary>
        /// Procesa una colección de perfiles de vehículos y los convierte en una lista de tipos de vehículos.
        /// </summary>
        /// <param name="profiles">Colección de perfiles de vehículos de tipo VehicleProfile a procesar.</param>
        /// <returns>Una lista de VehicleType con los perfiles de vehículos procesados.</returns>
        public static List<VehicleType> ProcessVehicleTypes(IEnumerable<VehicleProfile> profiles)
        {
            return profiles.Select(x => new VehicleType()
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }

        /// <summary>
        /// Procesa una colección de shifts y lo convierte en su dto.
        /// </summary>
        /// <param name="profiles">Colección de perfiles de vehículos de tipo VehicleProfile a procesar.</param>
        /// <returns>Una lista de VehicleType con los perfiles de vehículos procesados.</returns>
        public static List<ShiftDto> ProcessShifts(IEnumerable<Shift> shifts)
        {
            return shifts.Select(x => new ShiftDto()
            {
                id = x.Id,
                name = x.Name,
                initHour = TimeSpan.FromHours(x.InitHour),
                endHour = TimeSpan.FromHours(x.EndHour),
                schedules = ProcessSchedules(x.Schedules ?? new()),
            }).ToList();
        }

        public static List<ShiftSheduleDto> ProcessSchedules(IEnumerable<Schedule> schedules)
        {
            return schedules.Select(x => new ShiftSheduleDto
            {
                id = x.Id,
                workerId = x.AvailableWorkerId,
                name = x.AvailableWorker.Name,
                initHour = DateTime.Today.AddHours(x.CustomInitHour ?? x.Shift.InitHour),
                endHour = DateTime.Today.AddHours(x.CustomEndHour ?? x.Shift.EndHour),
                breakProfileId = x.BreakProfileId,
            }).ToList();
        }
    }
}