using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Yard.Core
{
    public class YardStageCalculations
    {
        #region Methods

        #region Public

        public static IEnumerable<YardMetricsPerStage> YardMetricsPerStages(IEnumerable<ItemPlanning> itemPlannings, IEnumerable<YardAppointmentsNotifications> appointmentsNotifications)
        {
            var itemPlanningsWithAppointments = itemPlannings.Where(x => x.StageId != null) // DIFF: Loading/Inbound por Stage not null
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
                    StageId = ip.StageId, // DIFF: DockId de WorkOrder por StageId de ItemPlanning
                    PlanningId = ip.WorkOrderPlanning.PlanningId
                });

            var appointments = itemPlanningsWithAppointments.GroupBy(x => (x.AppointmentDate, x.VehicleCode, x.AppointmentInitDate, x.AppointmentEndDate,
                                                                            x.StageId, x.PlanningId, x.ProcessType)) // DIFF: Agrupación por StageId en vez de DockId
                .Select(x => new
                {
                    AppointmentDate = x.Key.AppointmentDate,
                    AppointmentInitDate = x.Key.AppointmentInitDate,
                    AppointmentEndDate = x.Key.AppointmentEndDate,
                    ItemPlanningInitDate = x.Min(m => m.ItemPlanningInitDate),
                    ItemPlanningEndDate = x.Max(m => m.ItemPlanningEndDate),
                    ProcessType = x.Key.ProcessType,
                    VehicleCode = x.Key.VehicleCode,
                    StageId = x.Key.StageId, // DIFF: Seleccionamos StageId en lugar de DockId
                    PlanningId = x.Key.PlanningId
                })
                .Select(x => new
                {
                    StartDate = x.AppointmentInitDate == null ? x.ItemPlanningInitDate : x.AppointmentInitDate,
                    EndDate = x.AppointmentEndDate == null ? x.ItemPlanningEndDate : x.AppointmentEndDate,
                    StageId = x.StageId, // DIFF: Seleccionamos StageId en lugar de DockId
                    ProcessType = x.ProcessType,
                    PlanningId = x.PlanningId,
                    IsAttended = x.AppointmentEndDate == null ? false : true
                });

            return appointments.GroupBy(x => ((x.StageId, x.ProcessType, x.PlanningId))) // DIFF: Agrupamos por StageId en lugar de DockId
                .Select(x => new YardMetricsPerStage
                {
                    Id = Guid.NewGuid(),
                    ProcessType = x.Key.ProcessType,
                    StartDate = x.Min(m => m.StartDate.Value),
                    EndDate = x.Max(m => m.EndDate.Value),
                    StageId = x.Key.StageId.Value, // DIFF: StageId en lugar de DockId
                    AttendedAppointments = x.Count(m => m.IsAttended),
                    TotalAppointments = x.Count(m => m.IsAttended) + x.Count(m => !m.IsAttended),
                    Saturation = 0,
                    PlanningId = x.Key.PlanningId
                });
        }

        public static IEnumerable<YardStageUsagePerHour> YardStageUsagePerHour(IEnumerable<ItemPlanning> itemPlannings, IEnumerable<Stage> stages, DateTime now)
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
            var totalCapacity = stages.Select(x => new
            {
                StageId = x.Id, // DIFF: StageId por DockId
                Time = 3600 * x.Zone.MaxEquipments.GetValueOrDefault(0)
            });

            // Dividir los procesos en bloques de horas enteras
            var dividedItemPlannings = HourDivider(itemPlannings.Where(x => x.StageId != null)); // DIFF: Cambio en el where para stages

            // Calculamos la utilización y saturación de la parte real
            var realInitDate = initDate;
            var realEndDate = nowDate;

            var realItemPlannings = dividedItemPlannings.Where(x => x.InitDate >= realInitDate && x.EndDate <= realEndDate);

            var realCalculations = totalCapacity.Join(realItemPlannings, tc => tc.StageId, ip => ip.StageId, (tc, ip) => new // DIFF: Join por StageId en vez de DockId
            {
                StageId = tc.StageId, // DIFF: StageId por DockId
                InitDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0),
                EndDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0).AddHours(1),
                WorkTime = ip.WorkTime,
                Time = tc.Time
            })
            .GroupBy(x => (x.StageId, x.Time, x.InitDate, x.EndDate)).Select(x => new // DIFF: StageId por DockId
            {
                StageId = x.Key.StageId, // DIFF: StageId por DockId
                Time = x.Key.Time,
                InitDate = x.Key.InitDate,
                EndDate = x.Key.EndDate,
                WorkTime = x.Sum(m => m.WorkTime)
            }).Select(x => new
            {
                InitHour = x.InitDate,
                EndHour = x.EndDate,
                StageId = x.StageId, // DIFF: StageId por DockId
                RealUsage = x.Time == 0 ? 0 : x.WorkTime / x.Time,
                PlannedUsage = 0.0,
                Saturation = x.Time == 0 ? 0 : x.WorkTime / x.Time,
            });

            // Calculamos la utilización y la saturación de la parte planificada
            var plannedInitDate = nowDate.AddHours(1);
            var plannedEndDate = endDate;

            var plannedItemPlannings = dividedItemPlannings.Where(x => x.InitDate >= plannedInitDate && x.EndDate <= plannedEndDate);

            var plannedCalculations = totalCapacity.Join(plannedItemPlannings, tc => tc.StageId, ip => ip.StageId, (tc, ip) => new // DIFF: Join por StageId en vez de DockId
            {
                StageId = tc.StageId, // DIFF: StageId en lugar de DockId
                InitDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0),
                EndDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0).AddHours(1),
                WorkTime = ip.WorkTime,
                Time = tc.Time
            })
            .GroupBy(x => (x.StageId, x.Time, x.InitDate, x.EndDate)).Select(x => new // DIFF: GroupBy StageId en vez de DockId
            {
                StageId = x.Key.StageId, // DIFF: StageId en lugar de DockId
                Time = x.Key.Time,
                InitDate = x.Key.InitDate,
                EndDate = x.Key.EndDate,
                WorkTime = x.Sum(m => m.WorkTime)
            }).Select(x => new
            {
                InitHour = x.InitDate,
                EndHour = x.EndDate,
                StageId = x.StageId, // DIFF: StageId por DockId
                RealUsage = 0.0,
                PlannedUsage = x.Time == 0 ? 0 : x.WorkTime / x.Time,
                Saturation = x.Time == 0 ? 0 : x.WorkTime / x.Time,
            });

            // Calculamos la utilización y la saturación de la hora del ahora
            var nowInitDate = nowDate;
            var nowEndDate = nowDate.AddHours(1);

            var nowItemPlannings = dividedItemPlannings.Where(x => x.InitDate >= nowInitDate && x.EndDate <= nowEndDate);

            var realNowItemPlannings = totalCapacity.Join(nowItemPlannings.Where(x => x.Progress == 100),
                tc => tc.StageId, ip => ip.StageId, (tc, ip) => new // DIFF: Join por StageId
                {
                    StageId = tc.StageId, // DIFF: StageId en lugar de DockId
                    InitDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0),
                    EndDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0).AddHours(1),
                    WorkTime = ip.WorkTime,
                    Time = tc.Time
                })
            .GroupBy(x => (x.StageId, x.Time, x.InitDate, x.EndDate)).Select(x => new // DIFF: GroupBy StageId
            {
                StageId = x.Key.StageId, // DIFF: StageId en lugar de DockId
                Time = x.Key.Time,
                InitDate = x.Key.InitDate,
                EndDate = x.Key.EndDate,
                WorkTime = x.Sum(m => m.WorkTime)
            }).Select(x => new
            {
                InitHour = x.InitDate,
                EndHour = x.EndDate,
                StageId = x.StageId, // DIFF: StageId en lugar de DockId
                RealUsage = x.Time == 0 ? 0 : x.WorkTime / x.Time,
                PlannedUsage = 0.0,
                Saturation = 0.0
            });

            var plannedNowItemPlannings = totalCapacity.Join(nowItemPlannings.Where(x => x.Progress == 0),
                tc => tc.StageId, ip => ip.StageId, (tc, ip) => new // DIFF: Join por StageId
                {
                    StageId = tc.StageId, // DIFF: StageId por DockId
                    InitDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0),
                    EndDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0).AddHours(1),
                    WorkTime = ip.WorkTime,
                    Time = tc.Time
                })
            .GroupBy(x => (x.StageId, x.Time, x.InitDate, x.EndDate)).Select(x => new // DIFF: GroupBy StageId
            {
                StageId = x.Key.StageId, // DIFF: StageId en lugar de DockId
                Time = x.Key.Time,
                InitDate = x.Key.InitDate,
                EndDate = x.Key.EndDate,
                WorkTime = x.Sum(m => m.WorkTime)
            }).Select(x => new
            {
                InitHour = x.InitDate,
                EndHour = x.EndDate,
                StageId = x.StageId, // DIFF: StageId en lugar de DockId
                RealUsage = 0.0,
                PlannedUsage = x.Time == 0 ? 0 : x.WorkTime / x.Time,
                Saturation = 0.0
            });

            var nowSaturation = totalCapacity.Join(nowItemPlannings, tc => tc.StageId, ip => ip.StageId, (tc, ip) => new // DIFF: Join por StageId
            {
                StageId = tc.StageId, // DIFF: StageId por DockId
                InitDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0),
                EndDate = new DateTime(ip.InitDate.Year, ip.InitDate.Month, ip.InitDate.Day, ip.InitDate.Hour, 0, 0).AddHours(1),
                WorkTime = ip.WorkTime,
                Time = tc.Time
            })
            .GroupBy(x => (x.StageId, x.Time, x.InitDate, x.EndDate)).Select(x => new // DIFF: GroupBy StageId
            {
                StageId = x.Key.StageId, // DIFF: StageId en lugar de DockId
                Time = x.Key.Time,
                InitDate = x.Key.InitDate,
                EndDate = x.Key.EndDate,
                WorkTime = x.Sum(m => m.WorkTime)
            }).Select(x => new
            {
                InitHour = x.InitDate,
                EndHour = x.EndDate,
                StageId = x.StageId, // DIFF: StageId en lugar de DockId
                RealUsage = 0.0,
                PlannedUsage = 0.0,
                Saturation = x.Time == 0 ? 0 : x.WorkTime / x.Time
            });

            var groupedNow = realNowItemPlannings.Concat(plannedNowItemPlannings).Concat(nowSaturation)
                .GroupBy(x => (x.InitHour, x.EndHour, x.StageId)).Select(x => new // DIFF: GroupBy StageId
                {
                    InitHour = x.Key.InitHour,
                    EndHour = x.Key.EndHour,
                    StageId = x.Key.StageId, // DIFF: StageId por DockId
                    RealUsage = x.Sum(m => m.RealUsage),
                    PlannedUsage = x.Sum(m => m.PlannedUsage),
                    Saturation = x.Sum(m => m.Saturation)
                });

            var totalGroup = realCalculations.Concat(plannedCalculations).Concat(groupedNow)
                .Join(stages, c => c.StageId, s => s.Id, (c, s) => new YardStageUsagePerHour() // DIFF: Join por StageId
                {
                    Id = Guid.NewGuid(),
                    InitHour = DateTime.SpecifyKind(c.InitHour, DateTimeKind.Utc),
                    EndHour = DateTime.SpecifyKind(c.EndHour, DateTimeKind.Utc),
                    StageId = c.StageId, // DIFF: StageId por DockId
                    TotalCapacity = Math.Round(((s.Zone.MaxEquipments.GetValueOrDefault(0) / 2.0) * 100), 2),
                    RealUsage = Math.Round((c.RealUsage * 100), 2),
                    PlannedUsage = Math.Round((c.PlannedUsage * 100), 2),
                    Saturation = Math.Round((c.Saturation * 100), 2),
                    PlanningId = itemPlannings.FirstOrDefault().WorkOrderPlanning.PlanningId,
                    WarehouseId = itemPlannings.FirstOrDefault().WorkOrderPlanning.Planning.WarehouseId
                });

            return totalGroup;
        }

        public static IEnumerable<YardMetricsAppointmentsPerStage> YardMetricsAppointmentsPerStage(IEnumerable<YardAppointmentsNotifications> appointments,
            IEnumerable<ItemPlanning> itemPlannings, IEnumerable<YardMetricsPerStage> yardPerStage, IEnumerable<OutboundFlowGraph> outboundFlow, 
            IEnumerable<InboundFlowGraph> inboundFlow)
        {
            var itemPlanningsWithAppointments = itemPlannings.Where(x => x.StageId != null)
                .Join(appointments,
                ip => (ip.WorkOrderPlanning.AppointmentDate, ip.WorkOrderPlanning.InputOrder.VehicleCode),
                a => (a.AppointmentDate, a.VehicleCode),
                (ip, a) => new
                {
                    AppointmentDate = a.AppointmentDate,
                    ItemPlanningInitDate = ip.InitDate,
                    ItemPlanningEndDate = ip.EndDate,
                    ProcessType = ip.Process.Type,
                    WorkOrder = ip.WorkOrderPlanning,
                    VehicleCode = a.VehicleCode,
                    StageId = ip.StageId,
                    PlanningId = ip.WorkOrderPlanning.PlanningId,
                    AppointmentCode = a.AppointmentCode,
                    YardCode = a.YardCode,
                    VehicleType = a.VehicleType,
                    Customer = a.Customer,
                    License = a.License
                });

            return itemPlanningsWithAppointments
                .GroupBy(x => (x.AppointmentDate, x.VehicleCode, x.AppointmentCode, x.YardCode, x.VehicleType,
                                x.Customer, x.License, x.StageId, x.WorkOrder, x.ProcessType))
                .Select(x => new
                {
                    AppointmentDate = x.Key.AppointmentDate,
                    ItemPlanningInitDate = x.Min(m => m.ItemPlanningInitDate),
                    ItemPlanningEndDate = x.Max(m => m.ItemPlanningEndDate),
                    ProcessType = x.Key.ProcessType,
                    VehicleCode = x.Key.VehicleCode,
                    StageId = x.Key.StageId,
                    PlanningId = x.FirstOrDefault().PlanningId,
                    AppointmentCode = x.Key.AppointmentCode,
                    YardCode = x.Key.YardCode,
                    VehicleType = x.Key.VehicleType,
                    Customer = x.Key.Customer,
                    License = x.Key.License,
                    Progress = x.Key.WorkOrder.Progress
                })
                .GroupBy(x => (x.AppointmentDate, x.VehicleCode, x.AppointmentCode, x.YardCode, x.VehicleType,
                            x.Customer, x.License, x.StageId, x.ProcessType, x.PlanningId))
                .Select(x => new
                {
                    AppointmentCode = x.Key.AppointmentCode,
                    ProcessType = x.Key.ProcessType,
                    AppointmentDate = x.Key.AppointmentDate,
                    ItemPlanningStartDate = x.Min(m => m.ItemPlanningInitDate),
                    ItemPlanningEndDate = x.Max(m => m.ItemPlanningEndDate),
                    Customer = x.Key.Customer,
                    YardCode = x.Key.YardCode,
                    StageId = x.Key.StageId.Value,
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
                    StartDate = x.ItemPlanningStartDate,
                    EndDate = x.ItemPlanningEndDate,
                    Customer = x.Customer,
                    YardCode = x.YardCode,
                    StageId = x.StageId,
                    VehicleCode = x.VehicleCode,
                    VehicleType = x.VehicleType,
                    License = x.License,
                    PlanningId = x.PlanningId,
                    TotalOrders = x.TotalOrders,
                    CompletedOrders = x.CompletedOrders
                })
                .Join(yardPerStage, ya => ya.StageId, ys => ys.StageId, (ya, ys) => new
                {
                    AppointmentCode = ya.AppointmentCode,
                    ProcessType = ya.ProcessType,
                    AppointmentDate = ya.AppointmentDate,
                    StartDate = ya.StartDate,
                    EndDate = ya.EndDate,
                    Customer = ya.Customer,
                    YardCode = ya.YardCode,
                    StageId = ya.StageId,
                    VehicleCode = ya.VehicleCode,
                    VehicleType = ya.VehicleType,
                    License = ya.License,
                    PlanningId = ya.PlanningId,
                    TotalOrders = ya.TotalOrders,
                    CompletedOrders = ya.CompletedOrders,
                    YardMetricsPerStageId = ys.Id
                })
                .Select(x => new YardMetricsAppointmentsPerStage
                {
                    Id = Guid.NewGuid(),
                    AppointmentCode = x.AppointmentCode,
                    ProcessType = x.ProcessType,
                    AppointmentDate = x.AppointmentDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Customer = x.Customer,
                    YardCode = x.YardCode,
                    StageId = x.StageId,
                    VehicleType = x.VehicleType,
                    License = x.License,
                    PlanningId = x.PlanningId,
                    TotalOrders = x.TotalOrders,
                    CompletedOrders = x.CompletedOrders,
                    Progress = Math.Round((double)(x.CompletedOrders / x.TotalOrders) * 100, 2),
                    YardMetricsPerStageId = x.YardMetricsPerStageId
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
                        Shift = ip.Shift,
                        StageId = ip.StageId
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
