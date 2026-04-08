using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Yard
{
    public class YardDockCalculations
    {
        #region Methods

        #region Public

        public static IEnumerable<YardMetricsPerDock> YardMetricsPerDocks(IEnumerable<ItemPlanning> itemPlannings, IEnumerable<YardAppointmentsNotifications> appointmentsNotifications)
        {
            var itemPlanningsWithAppointments = itemPlannings
                .Where(x => x.Process.Type == ProcessType.Loading || x.Process.Type == ProcessType.Inbound)
                .Where(x => x.WorkOrderPlanning.AssignedDock != null && x.WorkOrderPlanning.AssignedDockId != null)
                .Join(appointmentsNotifications,
                ip => (ip.WorkOrderPlanning.AppointmentDate, ip.WorkOrderPlanning.InputOrder.VehicleCode),
                a => (a.AppointmentDate, a.VehicleCode),
                (ip, a) => new
                {
                    AppointmentDate = a.AppointmentDate,
                    AppointmentInitDate = a.InitDate,
                    AppointmentEndDate = a.EndDate,
                    ItemPlanningInitDate = ip.InitDate,
                    ItemPlanningEndDate = ip.EndDate,
                    ProcessType = ip.Process.Type,
                    WorkOrderId = ip.WorkOrderPlanningId,
                    VehicleCode = a.VehicleCode,
                    DockId = ip.WorkOrderPlanning.AssignedDockId,
                    PlanningId = ip.WorkOrderPlanning.PlanningId
                });

            var appointments = itemPlanningsWithAppointments.GroupBy(x => (x.AppointmentDate, x.VehicleCode, x.AppointmentInitDate, x.AppointmentEndDate, 
                                                                            x.DockId, x.PlanningId, x.ProcessType))
                .Select(x => new
                {
                    AppointmentDate = x.Key.AppointmentDate,
                    AppointmentInitDate = x.Key.AppointmentInitDate, // Es único por appointment
                    AppointmentEndDate = x.Key.AppointmentEndDate, // Es único por appointment
                    ItemPlanningInitDate = x.Min(m => m.ItemPlanningInitDate),
                    ItemPlanningEndDate = x.Max(m => m.ItemPlanningEndDate),
                    ProcessType = x.Key.ProcessType, // Es único por ser todas las citas de entrada o salida
                    VehicleCode = x.Key.VehicleCode,
                    DockId = x.Key.DockId, // La cita tiene el mismo Dock
                    PlanningId = x.Key.PlanningId
                })
                .Select(x => new
                {
                    StartDate = x.AppointmentInitDate == null ? x.ItemPlanningInitDate : x.AppointmentInitDate,
                    EndDate = x.AppointmentEndDate == null ? x.ItemPlanningEndDate : x.AppointmentEndDate,
                    DockId = x.DockId,
                    ProcessType = x.ProcessType,
                    PlanningId = x.PlanningId,
                    IsAttended = x.AppointmentEndDate == null ? false : true
                });

            return appointments.GroupBy(x => ((x.DockId, x.ProcessType, x.PlanningId)))
                .Select(x => new YardMetricsPerDock
                {
                    Id = Guid.NewGuid(),
                    ProcessType = x.Key.ProcessType,
                    StartDate = x.Min(m => m.StartDate.Value),
                    EndDate = x.Max(m => m.EndDate.Value),
                    DockId = x.Key.DockId.Value,
                    AttendedAppointments = x.Count(m => m.IsAttended),
                    TotalAppointments = x.Count(m => m.IsAttended) + x.Count(m => !m.IsAttended),
                    Saturation = 0,
                    PlanningId = x.Key.PlanningId
                });
        }

        public static IEnumerable<YardDockUsagePerHour> YardDockUsagePerHour(IEnumerable<ItemPlanning> itemPlannings, IEnumerable<Dock> docks, DateTime now)
        {
            /*
             * 1. Capacidad total. Vamos a calcularlo lo primero debido a que lo necesitaremos para futuros cálculos
             * 2. Calcularemos el número de procesos reales por hora desde el inicio hasta el inicio de la hora del ahora
             * 3. Calcularemos el número de procesos planificados por hora desde el inicio hasta el fin de la hora del ahora
             * 4. Calculamos utilización real
             * 5. Calculamos utilización planificada
             * 6. SATURACIÓN
             *  - Dentro de la hora actual, vamos a diferenciar el porcentaje de la hora que ya ha pasado (real) y el porcentaje que queda (planificado)
             */

            var nowDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);

            var minDate = itemPlannings.Min(x => x.InitDate);
            var initDate = new DateTime(minDate.Year, minDate.Month, minDate.Day, minDate.Hour, 0, 0);
            var maxDate = itemPlannings.Max(x => x.EndDate);
            var endDate = new DateTime(maxDate.Year, maxDate.Month, maxDate.Day, maxDate.Hour, 0, 0).AddHours(1);

            // Capacidad total
            var totalCapacity = docks.Select(x => new
            {
                DockId = x.Id,
                Time = 3600 * x.Zone.MaxEquipments.GetValueOrDefault(0)
            });

            // Dividir los procesos en bloques de horas enteras
            var dividedItemPlannings = HourDivider(itemPlannings.Where(x => x.Process.Type == ProcessType.Inbound || x.Process.Type == ProcessType.Loading));

            // Calculamos la utilización y saturación de la parte real
            var realInitDate = initDate;
            var realEndDate = nowDate;

            var realItemPlannings = dividedItemPlannings.Where(x => x.InitDate >= realInitDate && x.EndDate <= realEndDate);

            var realCalculations = totalCapacity.Join(realItemPlannings, tc => tc.DockId, ip => ip.WorkOrderPlanning.AssignedDockId, (tc, ip) => new
            {
                DockId = tc.DockId,
                InitDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0 ,0),
                EndDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0).AddHours(1),
                WorkTime = ip.WorkTime,
                Time = tc.Time
            })
            .GroupBy(x => (x.DockId, x.Time, x.InitDate, x.EndDate)).Select(x => new
            {
                DockId = x.Key.DockId,
                Time = x.Key.Time,
                InitDate = x.Key.InitDate,
                EndDate = x.Key.EndDate,
                WorkTime = x.Sum(m => m.WorkTime)
            }).Select(x => new
            {
                InitHour = x.InitDate,
                EndHour = x.EndDate,
                DockId = x.DockId,
                RealUsage = x.Time == 0 ? 0 : x.WorkTime / x.Time,
                PlannedUsage = 0.0,
                Saturation = x.Time == 0 ? 0 : x.WorkTime / x.Time,
            });

            // Calculamos la utilización y la saturación de la parte planificada
            var plannedInitDate = nowDate.AddHours(1);
            var plannedEndDate = endDate;

            var plannedItemPlannings = dividedItemPlannings.Where(x => x.InitDate >= plannedInitDate && x.EndDate <= plannedEndDate);

            var plannedCalculations = totalCapacity.Join(plannedItemPlannings, tc => tc.DockId, ip => ip.WorkOrderPlanning.AssignedDockId, (tc, ip) => new
            {
                DockId = tc.DockId,
                InitDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0),
                EndDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0).AddHours(1),
                WorkTime = ip.WorkTime,
                Time = tc.Time
            })
            .GroupBy(x => (x.DockId, x.Time, x.InitDate, x.EndDate)).Select(x => new
            {
                DockId = x.Key.DockId,
                Time = x.Key.Time,
                InitDate = x.Key.InitDate,
                EndDate = x.Key.EndDate,
                WorkTime = x.Sum(m => m.WorkTime)
            }).Select(x => new
            {
                InitHour = x.InitDate,
                EndHour = x.EndDate,
                DockId = x.DockId,
                RealUsage = 0.0,
                PlannedUsage = x.Time == 0 ? 0 : x.WorkTime / x.Time,
                Saturation = x.Time == 0 ? 0 : x.WorkTime / x.Time,
            });

            // Calculamos la utilización y la saturación de la hora del ahora
            var nowInitDate = nowDate;
            var nowEndDate = nowDate.AddHours(1);

            var nowItemPlannings = dividedItemPlannings.Where(x => x.InitDate >= nowInitDate && x.EndDate <= nowEndDate);

            var realNowItemPlannings = totalCapacity.Join(nowItemPlannings.Where(x => x.Progress == 100), 
                tc => tc.DockId, ip => ip.WorkOrderPlanning.AssignedDockId, (tc, ip) => new
            {
                DockId = tc.DockId,
                InitDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0),
                EndDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0).AddHours(1),
                WorkTime = ip.WorkTime,
                Time = tc.Time
            })
            .GroupBy(x => (x.DockId, x.Time, x.InitDate, x.EndDate)).Select(x => new
            {
                DockId = x.Key.DockId,
                Time = x.Key.Time,
                InitDate = x.Key.InitDate,
                EndDate = x.Key.EndDate,
                WorkTime = x.Sum(m => m.WorkTime)
            }).Select(x => new
            {
                InitHour = x.InitDate,
                EndHour = x.EndDate,
                DockId = x.DockId,
                RealUsage = x.Time == 0 ? 0 : x.WorkTime / x.Time,
                PlannedUsage = 0.0,
                Saturation = 0.0
            });

            var plannedNowItemPlannings = totalCapacity.Join(nowItemPlannings.Where(x => x.Progress == 0),
                tc => tc.DockId, ip => ip.WorkOrderPlanning.AssignedDockId, (tc, ip) => new
            {
                DockId = tc.DockId,
                InitDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0),
                EndDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0).AddHours(1),
                WorkTime = ip.WorkTime,
                Time = tc.Time
            })
            .GroupBy(x => (x.DockId, x.Time, x.InitDate, x.EndDate)).Select(x => new
            {
                DockId = x.Key.DockId,
                Time = x.Key.Time,
                InitDate = x.Key.InitDate,
                EndDate = x.Key.EndDate,
                WorkTime = x.Sum(m => m.WorkTime)
            }).Select(x => new
            {
                InitHour = x.InitDate,
                EndHour = x.EndDate,
                DockId = x.DockId,
                RealUsage = 0.0,
                PlannedUsage = x.Time == 0 ? 0 : x.WorkTime / x.Time,
                Saturation = 0.0
            });

            var nowSaturation = totalCapacity.Join(nowItemPlannings, tc => tc.DockId, ip => ip.WorkOrderPlanning.AssignedDockId, (tc, ip) => new
            {
                DockId = tc.DockId,
                InitDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0),
                EndDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0).AddHours(1),
                WorkTime = ip.WorkTime,
                Time = tc.Time
            })
            .GroupBy(x => (x.DockId, x.Time, x.InitDate, x.EndDate)).Select(x => new
            {
                DockId = x.Key.DockId,
                Time = x.Key.Time,
                InitDate = x.Key.InitDate,
                EndDate = x.Key.EndDate,
                WorkTime = x.Sum(m => m.WorkTime)
            }).Select(x => new
            {
                InitHour = x.InitDate,
                EndHour = x.EndDate,
                DockId = x.DockId,
                RealUsage = 0.0,
                PlannedUsage = 0.0,
                Saturation = x.Time == 0 ? 0 : x.WorkTime / x.Time
            });

            var groupedNow = realNowItemPlannings.Concat(plannedNowItemPlannings).Concat(nowSaturation)
                .GroupBy(x => (x.InitHour, x.EndHour, x.DockId)).Select(x => new
                {
                    InitHour = x.Key.InitHour,
                    EndHour = x.Key.EndHour,
                    DockId = x.Key.DockId,
                    RealUsage = x.Sum(m => m.RealUsage),
                    PlannedUsage = x.Sum(m => m.PlannedUsage),
                    Saturation = x.Sum(m => m.Saturation)
                });

            var totalGroup = realCalculations.Concat(plannedCalculations).Concat(groupedNow)
                .Join(docks, c => c.DockId, d => d.Id, (c, d) => new YardDockUsagePerHour()
                {
                    Id = Guid.NewGuid(),
                    InitHour = DateTime.SpecifyKind(c.InitHour, DateTimeKind.Utc),
                    EndHour = DateTime.SpecifyKind(c.EndHour, DateTimeKind.Utc),
                    DockId = c.DockId,
                    AllowInbound = d.AllowInbound,
                    AllowOutbound = d.AllowOutbound,
                    TotalCapacity = Math.Round(((d.Zone.MaxEquipments.GetValueOrDefault(0) / 2.0) * 100), 2),
                    RealUsage = Math.Round((c.RealUsage * 100), 2),
                    PlannedUsage = Math.Round((c.PlannedUsage * 100), 2),
                    Saturation = Math.Round((c.Saturation * 100), 2),
                    PlanningId = itemPlannings.FirstOrDefault().WorkOrderPlanning.PlanningId,
                    WarehouseId = itemPlannings.FirstOrDefault().WorkOrderPlanning.Planning.WarehouseId
                });

            return totalGroup;
        }

        public static IEnumerable<YardMetricsAppointmentsPerDock> YardMetricsAppointmentsPerDock(IEnumerable<YardAppointmentsNotifications> appointments,
            IEnumerable<ItemPlanning> itemPlannings, IEnumerable<YardMetricsPerDock> yardPerDock, IEnumerable<OutboundFlowGraph> outboundFlow, 
            IEnumerable<InboundFlowGraph> inboundFlow)
        {
            var itemPlanningsWithAppointments = itemPlannings
                .Where(x => x.Process.Type == ProcessType.Inbound || x.Process.Type == ProcessType.Loading)
                .Where(x => x.WorkOrderPlanning.AssignedDock != null && x.WorkOrderPlanning.AssignedDockId != null)
                .Join(appointments,
                ip => (ip.WorkOrderPlanning.AppointmentDate, ip.WorkOrderPlanning.InputOrder.VehicleCode),
                a => (a.AppointmentDate, a.VehicleCode),
                (ip, a) => new
                {
                    AppointmentDate = a.AppointmentDate,
                    AppointmentInitDate = a.InitDate,
                    AppointmentEndDate = a.EndDate,
                    ItemPlanningInitDate = ip.InitDate,
                    ItemPlanningEndDate = ip.EndDate,
                    ProcessType = ip.Process.Type,
                    WorkOrder = ip.WorkOrderPlanning,
                    VehicleCode = a.VehicleCode,
                    DockId = ip.WorkOrderPlanning.AssignedDockId,
                    PlanningId = ip.WorkOrderPlanning.PlanningId,
                    AppointmentCode = a.AppointmentCode,
                    YardCode = a.YardCode,
                    VehicleType = a.VehicleType,
                    Customer = a.Customer,
                    License = a.License
                });

            return itemPlanningsWithAppointments
                .GroupBy(x => (x.AppointmentDate, x.VehicleCode, x.AppointmentInitDate, x.AppointmentEndDate, x.AppointmentCode, x.YardCode, x.VehicleType,
                                x.Customer, x.License, x.DockId, x.WorkOrder, x.ProcessType))
                .Select(x => new
                {
                    AppointmentDate = x.Key.AppointmentDate,
                    AppointmentInitDate = x.Key.AppointmentInitDate, // Es único por appointment
                    AppointmentEndDate = x.Key.AppointmentEndDate, // Es único por appointment
                    ItemPlanningInitDate = x.Min(m => m.ItemPlanningInitDate),
                    ItemPlanningEndDate = x.Max(m => m.ItemPlanningEndDate),
                    ProcessType = x.Key.ProcessType,
                    VehicleCode = x.Key.VehicleCode,
                    DockId = x.Key.DockId, // La cita tiene el mismo Dock
                    PlanningId = x.FirstOrDefault().PlanningId,
                    AppointmentCode = x.Key.AppointmentCode,
                    YardCode = x.Key.YardCode,
                    VehicleType = x.Key.VehicleType,
                    Customer = x.Key.Customer,
                    License = x.Key.License,
                    Progress = x.Key.WorkOrder.Progress
                })
                .GroupBy(x => (x.AppointmentDate, x.VehicleCode, x.AppointmentCode, x.YardCode, x.VehicleType,
                            x.Customer, x.License, x.DockId, x.ProcessType, x.PlanningId))
                .Select(x => new
                {
                    AppointmentCode = x.Key.AppointmentCode,
                    ProcessType = x.Key.ProcessType,
                    AppointmentDate = x.Key.AppointmentDate,
                    AppointmentStartDate = x.Min(m => m.AppointmentInitDate), // Unico
                    AppointmentEndDate = x.Max(m => m.AppointmentEndDate), // Unico
                    ItemPlanningStartDate = x.Min(m => m.ItemPlanningInitDate),
                    ItemPlanningEndDate = x.Max(m => m.ItemPlanningEndDate),
                    Customer = x.Key.Customer,
                    YardCode = x.Key.YardCode,
                    DockId = x.Key.DockId.Value,
                    VehicleType = x.Key.VehicleType,
                    VehicleCode = x.Key.VehicleCode,
                    License = x.Key.License,
                    PlanningId = x.Key.PlanningId,
                    TotalOrders = x.Count(),
                    CompletedOrders = x.Count(m => m.Progress == 100)
                })
                .Select(x => new
                {
                    AppointmentCode = x.AppointmentCode,
                    ProcessType = x.ProcessType,
                    AppointmentDate = x.AppointmentDate,
                    StartDate = x.AppointmentStartDate ?? x.ItemPlanningStartDate,
                    EndDate = x.AppointmentEndDate ?? x.ItemPlanningEndDate,
                    Customer = x.Customer,
                    YardCode = x.YardCode,
                    DockId = x.DockId,
                    VehicleCode = x.VehicleCode,
                    VehicleType = x.VehicleType,
                    License = x.License,
                    PlanningId = x.PlanningId,
                    TotalOrders = x.TotalOrders,
                    CompletedOrders = x.CompletedOrders
                })
                .Join(yardPerDock, ya => ya.DockId, yd => yd.DockId, (ya, yd) => new
                {
                    AppointmentCode = ya.AppointmentCode,
                    ProcessType = ya.ProcessType,
                    AppointmentDate = ya.AppointmentDate,
                    StartDate = ya.StartDate,
                    EndDate = ya.EndDate,
                    Customer = ya.Customer,
                    YardCode = ya.YardCode,
                    DockId = ya.DockId,
                    VehicleCode = ya.VehicleCode,
                    VehicleType = ya.VehicleType,
                    License = ya.License,
                    PlanningId = ya.PlanningId,
                    TotalOrders = ya.TotalOrders,
                    CompletedOrders = ya.CompletedOrders,
                    YardMetricsPerDockId = yd.Id
                })
                .Select(x => new YardMetricsAppointmentsPerDock
                {
                    Id = Guid.NewGuid(),
                    AppointmentCode = x.AppointmentCode,
                    ProcessType = x.ProcessType,
                    AppointmentDate = x.AppointmentDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Customer = x.Customer,
                    YardCode = x.YardCode,
                    DockId = x.DockId,
                    VehicleType = x.VehicleType,
                    License = x.License,
                    PlanningId = x.PlanningId,
                    TotalOrders = x.TotalOrders,
                    CompletedOrders = x.CompletedOrders,
                    Progress = Math.Round((double)(x.CompletedOrders / x.TotalOrders) * 100, 2),
                    YardMetricsPerDockId = x.YardMetricsPerDockId
                });
        }

        #endregion

        #region Private

        private static List<ItemPlanning> HourDivider(IEnumerable<ItemPlanning> itemPlannings)
        {
            var result = new List<ItemPlanning>();

            foreach (var ip in itemPlannings)
            {
                DateTime start = ip.InitDate;
                DateTime end = ip.EndDate;

                while (start < end)
                {
                    // Calcular el final del tramo actual (fin de la hora)
                    DateTime hourEnd = new DateTime(
                        start.Year, start.Month, start.Day,
                        start.Hour, 0, 0).AddHours(1);

                    // El final real de este tramo es el menor entre finDeLaHora y el fin original
                    DateTime dvisionEnd = (end < hourEnd) ? end : hourEnd;

                    //resultado.Add(new Proceso(inicio, finTramo));
                    result.Add(new ItemPlanning()
                    {
                        Id = ip.Id,
                        ProcessId = ip.ProcessId,
                        Process = ip.Process,
                        IsOutbound = ip.IsOutbound,
                        LimitDate = ip.LimitDate,
                        InitDate = start,
                        EndDate = dvisionEnd,
                        WorkTime = (dvisionEnd - start).TotalSeconds,
                        IsStored = ip.IsStored,
                        IsBlocked = ip.IsBlocked,
                        IsStarted = ip.IsStarted,
                        WorkOrderPlanningId = ip.WorkOrderPlanningId,
                        WorkOrderPlanning = ip.WorkOrderPlanning,
                        WorkerId = ip.WorkerId,
                        Worker = ip.Worker,
                        EquipmentGroupId = ip.EquipmentGroupId,
                        EquipmentGroup = ip.EquipmentGroup,
                        IsFaked = ip.IsFaked,
                        Progress = ip.Progress,
                        ShiftId = ip.ShiftId,
                        Shift = ip.Shift
                    });

                    // El nuevo inicio será el fin del tramo que acabamos de crear
                    start = dvisionEnd;
                }
            }

            return result;
        }

        #endregion

        #endregion
    }
}
