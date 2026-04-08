using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Labor.Helper;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Labor.Core
{
    public class LaborCalculations
    {
        #region Public

        /// <summary>
        /// Calculates the WFMLabor from the simulation planning 
        /// </summary>
        /// <param name="itemPlannings">List of item plannings returned by the simulation</param>
        /// <param name="schedules">IEnumerable of the schedules filtered by warehouse</param>
        /// <param name="breaks">IEnumerable of the breaks filtered by warehouse</param>
        /// <param name="steps">IEnumerable of the steps filtered by warehouse</param>
        /// <param name="processes">IEnumerable of the processes filtered by warehouse</param>
        /// <param name="now">Actual datetime to compare the data</param>
        /// <returns>IEnumerable with the necessary registers of the WFMLabor object</returns>
        public static IEnumerable<WFMLaborWorker> WFMLabor(
            IEnumerable<ItemPlanning> itemPlannings,
            IEnumerable<Schedule> schedules,
            IEnumerable<Break> breaks,
            IEnumerable<Step> steps,
            IEnumerable<Process> processes,
            DateTime now)
        {
            var items = itemPlannings as IList<ItemPlanning> ?? itemPlannings.ToList();
            var sched = schedules as IList<Schedule> ?? schedules.ToList();
            var brks = breaks as IList<Break> ?? breaks.ToList();
            var stps = steps as IList<Step> ?? steps.ToList();
            var procs = processes as IList<Process> ?? processes.ToList();

            // Índices
            var scheduleById = sched.ToDictionary(s => s.Id);
            var scheduleIdsByWorker = sched
                .Where(s => s.AvailableWorker?.WorkerId != null)
                .ToLookup(s => s.AvailableWorker.WorkerId, s => s.Id);

            var processById = procs.ToDictionary(p => p.Id);

            var stepEdgeTimeByProcessId = stps
                .Where(s => s.InitProcess || s.EndProcess)
                .GroupBy(s => s.ProcessId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TimeMin));

            var theoreticalByProcessId = procs.ToDictionary(
                p => p.Id,
                p => p.MinTime + (p.PreprocessTime ?? 0) + (p.PostprocessTime ?? 0)
                     + (stepEdgeTimeByProcessId.TryGetValue(p.Id, out var t) ? t : 0)
            );

            var breaksByProfile = brks.GroupBy(b => b.BreakProfileId)
                                      .ToDictionary(g => g.Key, g => (IReadOnlyList<Break>)g.ToList());

            var breakCountByProfile = brks.GroupBy(b => b.BreakProfileId)
                                          .ToDictionary(g => g.Key, g => g.Count());

            // (igual que Join)
            var laborData =
                from ip in items
                where ip.WorkerId.HasValue
                from scheduleId in scheduleIdsByWorker[ip.WorkerId.Value]
                let p = processById[ip.ProcessId]
                select new
                {
                    WorkerId = ip.WorkerId!.Value,
                    ScheduleId = scheduleId,
                    PlanningId = ip.WorkOrderPlanning.PlanningId,
                    Progress = ip.Progress,
                    WorkTime = ip.WorkTime,
                    Direct = p.IsEffective ? ip.WorkTime : 0.0,
                    Indirect = !p.IsEffective ? ip.WorkTime : 0.0,
                    Theoretical = theoreticalByProcessId[p.Id],
                    InputOrderId = ip.WorkOrderPlanning.InputOrderId,
                    InputOrderClosed = ip.WorkOrderPlanning.InputOrder.Status == OrderStatus.Closed
                };

            var grouped = laborData
                .GroupBy(x => (x.WorkerId, x.ScheduleId, x.PlanningId))
                .Select(g =>
                {
                    double closedWork = 0, openWork = 0;
                    double closedDirect = 0, openDirect = 0;
                    double closedIndirect = 0, openIndirect = 0;
                    int theoretical = 0;
                    int closedProc = 0, openProc = 0;
                    var totalOrders = new HashSet<Guid?>();
                    var closedOrders = new HashSet<Guid?>();

                    foreach (var x in g)
                    {
                        if (x.Progress == 100)
                        {
                            closedWork += x.WorkTime;
                            closedDirect += x.Direct;
                            closedIndirect += x.Indirect;
                            theoretical += (int)x.Theoretical;
                            closedProc++;
                        }
                        else if (x.Progress == 0)
                        {
                            openWork += x.WorkTime;
                            openDirect += x.Direct;
                            openIndirect += x.Indirect;
                            openProc++;
                        }

                        totalOrders.Add(x.InputOrderId);
                        if (x.InputOrderClosed) closedOrders.Add(x.InputOrderId);
                    }

                    var sch = scheduleById[g.Key.ScheduleId];
                    var bs = breaksByProfile.TryGetValue(sch.BreakProfileId, out var arr) ? arr : Array.Empty<Break>();

                    var closedWST = CalculateWorkShiftTime(100, sch, bs, now);
                    var openWST = CalculateWorkShiftTime(0, sch, bs, now);

                    return new
                    {
                        g.Key.WorkerId,
                        g.Key.ScheduleId,
                        g.Key.PlanningId,
                        ClosedWorkTime = closedWork,
                        OpenWorkTime = openWork,
                        ClosedDirectTime = closedDirect,
                        OpenDirectTime = openDirect,
                        ClosedIndirectTime = closedIndirect,
                        OpenIndirectTime = openIndirect,
                        TheoreticalWorkTime = theoretical,
                        ClosedProcesses = closedProc,
                        OpenProcesses = openProc,
                        ClosedWorkShiftTime = closedWST,
                        OpenWorkShiftTime = openWST,
                        TotalOrders = totalOrders.Count,
                        ClosedOrders = closedOrders.Count,
                        Breaks = breakCountByProfile.TryGetValue(sch.BreakProfileId, out var bc) ? bc : 0
                    };
                });

            return grouped.Select(x => new WFMLaborWorker
            {
                Id = Guid.NewGuid(),
                WorkerId = x.WorkerId,
                ScheduleId = x.ScheduleId,
                PlanningId = x.PlanningId,
                WorkTime = Math.Round(x.ClosedWorkTime + x.OpenWorkTime, 2),
                TotalOrders = x.TotalOrders,
                ClosedOrders = x.ClosedOrders,
                ClosedProcesses = x.ClosedProcesses,
                TotalProcesses = x.ClosedProcesses + x.OpenProcesses,
                Breaks = x.Breaks,
                Ranking = 0,
                Productivity = x.ClosedWorkShiftTime == 0 ? 0 : Math.Round((x.ClosedDirectTime / x.ClosedWorkShiftTime) * 100, 2),
                Efficiency = x.ClosedWorkTime == 0 ? 0 : Math.Round((double)(x.TheoreticalWorkTime / x.ClosedWorkTime) * 100, 2),
                Utility = x.ClosedWorkShiftTime == 0 ? 0 : Math.Round(((x.ClosedDirectTime + x.ClosedIndirectTime) / x.ClosedWorkShiftTime) * 100, 2),
                TotalProductivity = (x.ClosedWorkShiftTime + x.OpenWorkShiftTime) != 0
                    ? Math.Round(((x.ClosedDirectTime + x.OpenDirectTime) / (x.ClosedWorkShiftTime + x.OpenWorkShiftTime)) * 100, 2)
                    : 0,
                TotalUtility = (x.ClosedWorkShiftTime + x.OpenWorkShiftTime) != 0
                    ? Math.Round(((x.ClosedDirectTime + x.OpenDirectTime + x.ClosedIndirectTime + x.OpenIndirectTime) / (x.ClosedWorkShiftTime + x.OpenWorkShiftTime)) * 100, 2)
                    : 0,
                Progress = (x.ClosedWorkTime + x.OpenWorkTime) == 0 ? 0 : Math.Round((x.ClosedWorkTime / (x.ClosedWorkTime + x.OpenWorkTime) * 100), 2)
            });
        }

        /// <summary>
        /// Calculates the WFMLaborData from the simulation planning
        /// </summary>
        /// <param name="itemPlannings">List of item plannings returned by the simulation</param>
        /// <param name="schedules">IEnumerable of the schedules filtered by warehouse</param>
        /// <param name="wfmLaborPerProcessType">Previously calculated WFMLaborPerProcessType</param>
        /// <param name="wfmLaborEquipmentPerProcessType">Previously calculated WFMLaborEquipmentPerProcessType</param>
        /// <returns>IEnumerable with the necessary registers of the WFMLaborData object</returns>
        public static IEnumerable<WFMLaborItemPlanning> WFMLaborItemPlanning(
            IEnumerable<ItemPlanning> itemPlannings,
            IEnumerable<Schedule> schedules,
            IEnumerable<WFMLaborWorkerPerProcessType> wfmLaborPerProcessType,
            IEnumerable<WFMLaborEquipmentPerProcessType> wfmLaborEquipmentPerProcessType)
        {
            var items = itemPlannings as IList<ItemPlanning> ?? itemPlannings.ToList();
            var sched = schedules as IList<Schedule> ?? schedules.ToList();

            // Índice 1:N: WorkerId -> Schedules.Id (un worker puede tener varios schedules)
            var scheduleIdsByWorker = sched
                .Where(s => s.AvailableWorker?.WorkerId != null)
                .ToLookup(s => s.AvailableWorker!.WorkerId!, s => s.Id);

            // Índices 1:1 para los dos "joins" que tenías antes (nos quedamos solo con los Id que necesitamos)
            var perProcessIndex = wfmLaborPerProcessType
                .ToDictionary(lpt => (lpt.WorkerId, lpt.ScheduleId, lpt.ProcessType, lpt.PlanningId),
                              lpt => lpt.Id);

            var equipPerProcessIndex = wfmLaborEquipmentPerProcessType
                .ToDictionary(lep => (lep.EquipmentGroupId, lep.ProcessType, lep.PlanningId),
                              lep => lep.Id);

            // Base (equivalente al primer from + join con schedules por worker)
            var baseQ =
                from ip in items
                where ip.WorkerId.HasValue
                where ip.EquipmentGroupId.HasValue
                from scheduleId in scheduleIdsByWorker[ip.WorkerId!.Value]
                select new
                {
                    WorkerId = ip.WorkerId!.Value,
                    ScheduleId = scheduleId,
                    InitDate = ip.InitDate,
                    EndDate = ip.EndDate,
                    EquipmentGroupId = ip.EquipmentGroupId!.Value,
                    InputOrderId = ip.WorkOrderPlanning.InputOrderId!.Value,
                    ProcessType = ip.Process.Name,
                    PlanningId = ip.WorkOrderPlanning.PlanningId,
                    Progress = ip.Progress,
                    WorkTime = ip.WorkTime,
                };

            // Sustituye los dos Join por consultas a diccionarios (O(1))
            return baseQ
                .Where(ip =>
                    perProcessIndex.ContainsKey((ip.WorkerId, ip.ScheduleId, ip.ProcessType, ip.PlanningId)) &&
                    equipPerProcessIndex.ContainsKey((ip.EquipmentGroupId, ip.ProcessType, ip.PlanningId)))
                .Select(ip => new WFMLaborItemPlanning
                {
                    Id = Guid.NewGuid(),
                    WorkerId = ip.WorkerId,
                    ScheduleId = ip.ScheduleId,
                    InitDate = ip.InitDate,
                    EndDate = ip.EndDate,
                    EquipmentGroupId = ip.EquipmentGroupId,
                    InputOrderId = ip.InputOrderId,
                    WFMLaborPerProcessTypeId = perProcessIndex[(ip.WorkerId, ip.ScheduleId, ip.ProcessType, ip.PlanningId)],
                    WFMLaborEquipmentPerProcessTypeId = equipPerProcessIndex[(ip.EquipmentGroupId, ip.ProcessType, ip.PlanningId)],
                    Progress = Math.Round(ip.Progress, 2),
                    WorkTime = Math.Round(ip.WorkTime, 2)
                });
        }


        /// <summary>
        /// Calculates the WFMLaborPerFlow from the simulation planning
        /// </summary>
        /// <param name="itemPlannings">IEnumerable of all the processes from the simulation</param>
        /// <param name="schedules">IEnumerable of the schedules filtered by warehouse</param>
        /// <param name="breaks">IEnumerable of the breaks filtered by warehouse</param>
        /// <param name="steps">IEnumerable of the steps filtered by warehouse</param>
        /// <param name="processes">IEnumerable of the processes filtered by warehouse</param>
        /// <param name="now">Actual datetime to compare the data</param>
        /// <param name="wfmLabor">Previously calculated WFMLabor</param>
        /// <returns>IEnumerable with the necessary registers of the WFMLaborPerProcessType object</returns>
        public static IEnumerable<WFMLaborWorkerPerFlow> WFMLaborPerFlow(
            IEnumerable<ItemPlanning> itemPlannings,
            IEnumerable<Schedule> schedules,
            IEnumerable<Break> breaks,
            IEnumerable<Step> steps,
            IEnumerable<Process> processes,
            DateTime now,
            IEnumerable<WFMLaborWorker> wfmLabor)
        {
            var items = itemPlannings as IList<ItemPlanning> ?? itemPlannings.ToList();
            var sched = schedules as IList<Schedule> ?? schedules.ToList();
            var brks = breaks as IList<Break> ?? breaks.ToList();
            var stps = steps as IList<Step> ?? steps.ToList();
            var procs = processes as IList<Process> ?? processes.ToList();
            var labor = wfmLabor as IList<WFMLaborWorker> ?? wfmLabor.ToList();

            var scheduleById = sched.ToDictionary(s => s.Id);
            var scheduleIdsByWorker = sched
                .Where(s => s.AvailableWorker?.WorkerId != null)
                .ToLookup(s => s.AvailableWorker.WorkerId, s => s.Id);

            var processById = procs.ToDictionary(p => p.Id);

            var stepEdgeTimeByProcessId = stps
                .Where(s => s.InitProcess || s.EndProcess)
                .GroupBy(s => s.ProcessId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TimeMin));

            var theoreticalByProcessId = procs.ToDictionary(
                p => p.Id,
                p => p.MinTime + (p.PreprocessTime ?? 0) + (p.PostprocessTime ?? 0)
                     + (stepEdgeTimeByProcessId.TryGetValue(p.Id, out var t) ? t : 0)
            );

            var breaksByProfile = brks.GroupBy(b => b.BreakProfileId)
                                      .ToDictionary(g => g.Key, g => (IReadOnlyList<Break>)g.ToList());

            var breakCountByProfile = brks.GroupBy(b => b.BreakProfileId)
                                          .ToDictionary(g => g.Key, g => g.Count());

            var laborData =
                from ip in items
                where ip.WorkerId.HasValue
                from scheduleId in scheduleIdsByWorker[ip.WorkerId.Value]
                let p = processById[ip.ProcessId]
                select new
                {
                    WorkerId = ip.WorkerId!.Value,
                    ScheduleId = scheduleId,
                    PlanningId = ip.WorkOrderPlanning.PlanningId,
                    IsOutbound = ip.IsOutbound,
                    Progress = ip.Progress,
                    WorkTime = ip.WorkTime,
                    Direct = p.IsEffective ? ip.WorkTime : 0.0,
                    Indirect = !p.IsEffective ? ip.WorkTime : 0.0,
                    Theoretical = theoreticalByProcessId[p.Id],
                    InputOrderId = ip.WorkOrderPlanning.InputOrderId,
                    InputOrderClosed = ip.WorkOrderPlanning.InputOrder.Status == OrderStatus.Closed
                };

            var grouped = laborData
                .GroupBy(x => (x.WorkerId, x.ScheduleId, x.PlanningId, x.IsOutbound))
                .Select(g =>
                {
                    double closedWork = 0, openWork = 0;
                    double closedDirect = 0, openDirect = 0;
                    double closedIndirect = 0, openIndirect = 0;
                    double theoretical = 0;
                    int closedProc = 0, openProc = 0;
                    var totalOrders = new HashSet<Guid?>();
                    var closedOrders = new HashSet<Guid?>();

                    foreach (var x in g)
                    {
                        if (x.Progress == 100)
                        {
                            closedWork += x.WorkTime;
                            closedDirect += x.Direct;
                            closedIndirect += x.Indirect;
                            theoretical += (double)x.Theoretical;
                            closedProc++;
                        }
                        else if (x.Progress == 0)
                        {
                            openWork += x.WorkTime;
                            openDirect += x.Direct;
                            openIndirect += x.Indirect;
                            openProc++;
                        }

                        totalOrders.Add(x.InputOrderId);
                        if (x.InputOrderClosed) closedOrders.Add(x.InputOrderId);
                    }

                    var sch = scheduleById[g.Key.ScheduleId];
                    var bs = breaksByProfile.TryGetValue(sch.BreakProfileId, out var arr) ? arr : Array.Empty<Break>();
                    var closedWST = CalculateWorkShiftTime(100, sch, bs, now);
                    var openWST = CalculateWorkShiftTime(0, sch, bs, now);

                    return new
                    {
                        g.Key.WorkerId,
                        g.Key.ScheduleId,
                        g.Key.PlanningId,
                        g.Key.IsOutbound,
                        ClosedWorkTime = closedWork,
                        OpenWorkTime = openWork,
                        ClosedDirectTime = closedDirect,
                        OpenDirectTime = openDirect,
                        ClosedIndirectTime = closedIndirect,
                        OpenIndirectTime = openIndirect,
                        TheoreticalWorkTime = theoretical,
                        ClosedProcesses = closedProc,
                        OpenProcesses = openProc,
                        ClosedWorkShiftTime = closedWST,
                        OpenWorkShiftTime = openWST,
                        TotalOrders = totalOrders.Count,
                        ClosedOrders = closedOrders.Count,
                        Breaks = breakCountByProfile.TryGetValue(sch.BreakProfileId, out var bc) ? bc : 0
                    };
                });

            var laborByKey = labor.ToDictionary(
                l => (l.WorkerId, l.ScheduleId, l.PlanningId), l => l.Id);

            return grouped
                .Where(x => laborByKey.ContainsKey((x.WorkerId, x.ScheduleId, x.PlanningId)))
                .Select(x => new WFMLaborWorkerPerFlow
                {
                    Id = Guid.NewGuid(),
                    WorkerId = x.WorkerId,
                    ScheduleId = x.ScheduleId,
                    PlanningId = x.PlanningId,
                    IsOutbound = x.IsOutbound,
                    WorkTime = Math.Round(x.ClosedWorkTime + x.OpenWorkTime, 2),
                    TotalOrders = x.TotalOrders,
                    ClosedOrders = x.ClosedOrders,
                    ClosedProcesses = x.ClosedProcesses,
                    TotalProcesses = x.ClosedProcesses + x.OpenProcesses,
                    Breaks = x.Breaks,
                    Ranking = 0,
                    Productivity = x.ClosedWorkShiftTime == 0 ? 0 : Math.Round((x.ClosedDirectTime / x.ClosedWorkShiftTime) * 100, 2),
                    Efficiency = x.ClosedWorkTime == 0 ? 0 : Math.Round((double)(x.TheoreticalWorkTime / x.ClosedWorkTime) * 100, 2),
                    Utility = x.ClosedWorkShiftTime == 0 ? 0 : Math.Round(((x.ClosedDirectTime + x.ClosedIndirectTime) / x.ClosedWorkShiftTime) * 100, 2),
                    TotalProductivity = (x.ClosedWorkShiftTime + x.OpenWorkShiftTime) != 0
                        ? Math.Round(((x.ClosedDirectTime + x.OpenDirectTime) / (x.ClosedWorkShiftTime + x.OpenWorkShiftTime)) * 100, 2)
                        : 0,
                    TotalUtility = (x.ClosedWorkShiftTime + x.OpenWorkShiftTime) != 0
                        ? Math.Round(((x.ClosedDirectTime + x.OpenDirectTime + x.ClosedIndirectTime + x.OpenIndirectTime) / (x.ClosedWorkShiftTime + x.OpenWorkShiftTime)) * 100, 2)
                        : 0,
                    WFMLaborId = laborByKey[(x.WorkerId, x.ScheduleId, x.PlanningId)],
                    Progress = (x.ClosedWorkTime + x.OpenWorkTime) == 0 ? 0 : Math.Round((x.ClosedWorkTime / (x.ClosedWorkTime + x.OpenWorkTime) * 100), 2)
                });
        }

        /// <summary>
        /// Calculates the WFMLaborPerProcessType from the simulation planning
        /// </summary>
        /// <param name="itemPlannings">IEnumerable of all the processes from the simulation</param>
        /// <param name="schedules">IEnumerable of the schedules filtered by warehouse</param>
        /// <param name="breaks">IEnumerable of the breaks filtered by warehouse</param>
        /// <param name="steps">IEnumerable of the steps filtered by warehouse</param>
        /// <param name="processes">IEnumerable of the processes filtered by warehouse</param>
        /// <param name="now">Actual datetime to compare the data</param>
        /// <param name="laborPerFlow">Previously calculated WFMLaborPerFlow</param>
        /// <returns>IEnumerable with the necessary registers of the WFMLaborPerProcessType object</returns>
        public static IEnumerable<WFMLaborWorkerPerProcessType> WFMLaborPerProcessType(
            IEnumerable<ItemPlanning> itemPlannings,
            IEnumerable<Schedule> schedules,
            IEnumerable<Break> breaks,
            IEnumerable<Step> steps,
            IEnumerable<Process> processes,
            DateTime now,
            IEnumerable<WFMLaborWorkerPerFlow> laborPerFlow)
        {
            var items = itemPlannings as IList<ItemPlanning> ?? itemPlannings.ToList();
            var sched = schedules as IList<Schedule> ?? schedules.ToList();
            var brks = breaks as IList<Break> ?? breaks.ToList();
            var stps = steps as IList<Step> ?? steps.ToList();
            var procs = processes as IList<Process> ?? processes.ToList();
            var perFlow = laborPerFlow as IList<WFMLaborWorkerPerFlow> ?? laborPerFlow.ToList();

            var scheduleById = sched.ToDictionary(s => s.Id);
            var scheduleIdsByWorker = sched
                .Where(s => s.AvailableWorker?.WorkerId != null)
                .ToLookup(s => s.AvailableWorker.WorkerId, s => s.Id);

            var processById = procs.ToDictionary(p => p.Id);
            var isOutByName = procs.ToDictionary(p => p.Name, p => p.IsOut);


            var stepEdgeTimeByProcessId = stps
                .Where(s => s.InitProcess || s.EndProcess)
                .GroupBy(s => s.ProcessId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TimeMin));

            var theoreticalByProcessId = procs.ToDictionary(
                p => p.Id,
                p => p.MinTime + (p.PreprocessTime ?? 0) + (p.PostprocessTime ?? 0)
                     + (stepEdgeTimeByProcessId.TryGetValue(p.Id, out var t) ? t : 0)
            );

            var breaksByProfile = brks.GroupBy(b => b.BreakProfileId)
                                      .ToDictionary(g => g.Key, g => (IReadOnlyList<Break>)g.ToList());

            var breakCountByProfile = brks.GroupBy(b => b.BreakProfileId)
                                          .ToDictionary(g => g.Key, g => g.Count());

            var laborData =
                from ip in items
                where ip.WorkerId.HasValue
                from scheduleId in scheduleIdsByWorker[ip.WorkerId.Value]
                let p = processById[ip.ProcessId]
                select new
                {
                    WorkerId = ip.WorkerId!.Value,
                    ScheduleId = scheduleId,
                    PlanningId = ip.WorkOrderPlanning.PlanningId,
                    ProcessType = ip.Process.Name,
                    Progress = ip.Progress,
                    WorkTime = ip.WorkTime,
                    Direct = p.IsEffective ? ip.WorkTime : 0.0,
                    Indirect = !p.IsEffective ? ip.WorkTime : 0.0,
                    Theoretical = theoreticalByProcessId[p.Id],
                    InputOrderId = ip.WorkOrderPlanning.InputOrderId,
                    InputOrderClosed = ip.WorkOrderPlanning.InputOrder.Status == OrderStatus.Closed
                };

            var grouped = laborData
                .GroupBy(x => (x.WorkerId, x.ScheduleId, x.PlanningId, x.ProcessType))
                .Select(g =>
                {
                    double closedWork = 0, openWork = 0;
                    double closedDirect = 0, openDirect = 0;
                    double closedIndirect = 0, openIndirect = 0;
                    int theoretical = 0;
                    int closedProc = 0, openProc = 0;
                    var totalOrders = new HashSet<Guid?>();
                    var closedOrders = new HashSet<Guid?>();

                    foreach (var x in g)
                    {
                        if (x.Progress == 100)
                        {
                            closedWork += x.WorkTime;
                            closedDirect += x.Direct;
                            closedIndirect += x.Indirect;
                            theoretical += (int)x.Theoretical;
                            closedProc++;
                        }
                        else if (x.Progress == 0)
                        {
                            openWork += x.WorkTime;
                            openDirect += x.Direct;
                            openIndirect += x.Indirect;
                            openProc++;
                        }

                        totalOrders.Add(x.InputOrderId);
                        if (x.InputOrderClosed) closedOrders.Add(x.InputOrderId);
                    }

                    var sch = scheduleById[g.Key.ScheduleId];
                    var bs = breaksByProfile.TryGetValue(sch.BreakProfileId, out var arr) ? arr : Array.Empty<Break>();
                    var closedWST = CalculateWorkShiftTime(100, sch, bs, now);
                    var openWST = CalculateWorkShiftTime(0, sch, bs, now);

                    return new
                    {
                        g.Key.WorkerId,
                        g.Key.ScheduleId,
                        g.Key.PlanningId,
                        g.Key.ProcessType,
                        IsOutbound = isOutByName.TryGetValue(g.Key.ProcessType, out var isOut) && isOut,
                        ClosedWorkTime = closedWork,
                        OpenWorkTime = openWork,
                        ClosedDirectTime = closedDirect,
                        OpenDirectTime = openDirect,
                        ClosedIndirectTime = closedIndirect,
                        OpenIndirectTime = openIndirect,
                        TheoreticalWorkTime = theoretical,
                        ClosedProcesses = closedProc,
                        OpenProcesses = openProc,
                        ClosedWorkShiftTime = closedWST,
                        OpenWorkShiftTime = openWST,
                        TotalOrders = totalOrders.Count,
                        ClosedOrders = closedOrders.Count,
                        Breaks = breakCountByProfile.TryGetValue(sch.BreakProfileId, out var bc) ? bc : 0
                    };
                });

            var perFlowKey = perFlow.ToDictionary(
                l => (l.WorkerId, l.ScheduleId, l.PlanningId, l.IsOutbound), l => l.Id);

            return grouped
                .Where(x => perFlowKey.ContainsKey((x.WorkerId, x.ScheduleId, x.PlanningId, x.IsOutbound)))
                .Select(x => new WFMLaborWorkerPerProcessType
                {
                    Id = Guid.NewGuid(),
                    WorkerId = x.WorkerId,
                    ScheduleId = x.ScheduleId,
                    PlanningId = x.PlanningId,
                    ProcessType = x.ProcessType,
                    WorkTime = Math.Round(x.ClosedWorkTime + x.OpenWorkTime, 2),
                    TotalOrders = x.TotalOrders,
                    ClosedOrders = x.ClosedOrders,
                    ClosedProcesses = x.ClosedProcesses,
                    TotalProcesses = x.ClosedProcesses + x.OpenProcesses,
                    Breaks = x.Breaks,
                    Ranking = 0,
                    Productivity = x.ClosedWorkShiftTime == 0 ? 0 : Math.Round((x.ClosedDirectTime / x.ClosedWorkShiftTime) * 100, 2),
                    Efficiency = x.ClosedWorkTime == 0 ? 0 : Math.Round((double)(x.TheoreticalWorkTime / x.ClosedWorkTime) * 100, 2),
                    Utility = x.ClosedWorkShiftTime == 0 ? 0 : Math.Round(((x.ClosedDirectTime + x.ClosedIndirectTime) / x.ClosedWorkShiftTime) * 100, 2),
                    TotalProductivity = (x.ClosedWorkShiftTime + x.OpenWorkShiftTime) != 0
                        ? Math.Round(((x.ClosedDirectTime + x.OpenDirectTime) / (x.ClosedWorkShiftTime + x.OpenWorkShiftTime)) * 100, 2)
                        : 0,
                    TotalUtility = (x.ClosedWorkShiftTime + x.OpenWorkShiftTime) != 0
                        ? Math.Round(((x.ClosedDirectTime + x.OpenDirectTime + x.ClosedIndirectTime + x.OpenIndirectTime) / (x.ClosedWorkShiftTime + x.OpenWorkShiftTime)) * 100, 2)
                        : 0,
                    WFMLaborPerFlowId = perFlowKey[(x.WorkerId, x.ScheduleId, x.PlanningId, x.IsOutbound)],
                    Progress = (x.ClosedWorkTime + x.OpenWorkTime) == 0 ? 0 : Math.Round((x.ClosedWorkTime / (x.ClosedWorkTime + x.OpenWorkTime) * 100), 2)
                });
        }

        /// <summary>
        /// Calculates the general data of WFMLaborEquipment
        /// </summary>
        /// <param name="itemPlannings">ItemPlannings to calculate the labor stats</param>
        /// <param name="shifts">Shifts data from database</param>
        /// <param name="steps">Steps data from database</param>
        /// <param name="processes">Porcesses data from database</param>
        /// <param name="now">UTC now DateTime</param>
        /// <returns>IEnumerable of WFMLaborEquipment stats</returns>
        public static IEnumerable<WFMLaborEquipment> WFMLaborEquipment(
            IEnumerable<ItemPlanning> itemPlannings,
            List<Shift> shifts,
            List<Step> steps,
            List<Process> processes,
            DateTime now)
        {
            var items = itemPlannings as IList<ItemPlanning> ?? itemPlannings.ToList();
            var stps = steps;
            var procs = processes;

            var stepEdgeTimeByProcessId = stps
                .Where(s => s.InitProcess || s.EndProcess)
                .GroupBy(s => s.ProcessId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TimeMin));

            var procById = procs.ToDictionary(p => p.Id);

            var all = items.Where(x => x.EquipmentGroup != null && x.EquipmentGroupId != null).Select(x => new
            {
                EquipmentGroupId = x.EquipmentGroupId,
                TypeEquipmentId = x.EquipmentGroup!.TypeEquipmentId,
                Equipments = x.EquipmentGroup!.Equipments,
                InitDate = x.InitDate,
                EndDate = x.EndDate,
                PlanningId = x.WorkOrderPlanning.PlanningId,
                WorkTime = x.WorkTime,
                TotalOrders = x.WorkOrderPlanning.InputOrderId,
                ClosedOrders = x.WorkOrderPlanning.InputOrder.Status == OrderStatus.Closed ? x.WorkOrderPlanning.InputOrderId : Guid.Empty,
                WorkTimeEffectiveProcess = x.Process.IsEffective ? x.WorkTime : 0
            })
            .GroupBy(x => (x.EquipmentGroupId, x.TypeEquipmentId, x.Equipments, x.PlanningId))
            .Select(g => new
            {
                EquipmentGroupId = g.Key.EquipmentGroupId,
                TypeEquipmentId = g.Key.TypeEquipmentId,
                Equipments = g.Key.Equipments,
                InitDate = g.Min(x => x.InitDate),
                EndDate = g.Max(x => x.EndDate),
                PlanningId = g.Key.PlanningId,
                WorkTime = g.Sum(m => m.WorkTime),
                TotalOrders = g.Select(m => m.TotalOrders).Distinct().Count(),
                ClosedOrders = g.Where(m => m.ClosedOrders != Guid.Empty).Select(x => x.ClosedOrders).Distinct().Count(),
                WorkTimeEffectiveProcess = g.Sum(m => m.WorkTimeEffectiveProcess),
                TotalWorkTime = CalculateTotalScheduleTime(false, shifts, now)
            })
            .Select(x => new
            {
                x.EquipmentGroupId,
                x.TypeEquipmentId,
                x.Equipments,
                x.InitDate,
                x.EndDate,
                x.PlanningId,
                x.WorkTime,
                x.TotalOrders,
                x.ClosedOrders,
                TotalProductivity = x.TotalWorkTime * x.Equipments == 0 ? 0 : x.WorkTimeEffectiveProcess / (x.TotalWorkTime * x.Equipments),
                TotalUtility = x.TotalWorkTime * x.Equipments == 0 ? 0 : x.WorkTime / (x.TotalWorkTime * x.Equipments)
            });

            var closed = items.Where(x => x.EquipmentGroup != null && x.EquipmentGroupId != null).Where(m => m.Progress == 100)
                .Select(x =>
                {
                    var p = procById[x.ProcessId];
                    var stepEdges = stepEdgeTimeByProcessId.TryGetValue(x.ProcessId, out var te) ? te : 0;
                    return new
                    {
                        EquipmentGroupId = x.EquipmentGroupId,
                        TypeEquipmentId = x.EquipmentGroup.TypeEquipmentId,
                        Equipments = x.EquipmentGroup.Equipments,
                        InitDate = x.InitDate,
                        EndDate = x.EndDate,
                        PlanningId = x.WorkOrderPlanning.PlanningId,
                        WorkTime = x.WorkTime,
                        WorkTimeEffectiveProcess = p.IsEffective ? x.WorkTime : 0,
                        WorkTheoricalTime = p.MinTime + (p.PreprocessTime ?? 0) + (p.PostprocessTime ?? 0) + stepEdges
                    };
                })
                .GroupBy(x => (x.EquipmentGroupId, x.Equipments, x.TypeEquipmentId, x.PlanningId))
                .Select(g => new
                {
                    EquipmentGroupId = g.Key.EquipmentGroupId,
                    TypeEquipmentId = g.Key.TypeEquipmentId,
                    Equipments = g.Key.Equipments,
                    InitDate = g.Min(x => x.InitDate),
                    EndDate = g.Max(x => x.EndDate),
                    PlanningId = g.Key.PlanningId,
                    TotalTime = CalculateTotalScheduleTime(true, shifts, now),
                    WorkTime = g.Sum(m => m.WorkTime),
                    WorkTimeEffectiveProcess = g.Sum(m => m.WorkTimeEffectiveProcess),
                    WorkTheoricalTime = g.Sum(m => m.WorkTheoricalTime)
                })
                .Select(x => new
                {
                    x.EquipmentGroupId,
                    x.TypeEquipmentId,
                    x.Equipments,
                    x.InitDate,
                    x.EndDate,
                    x.PlanningId,
                    x.WorkTime,
                    Efficiency = x.WorkTheoricalTime == 0 ? 0 : x.WorkTheoricalTime / x.WorkTime,
                    Productivity = x.TotalTime * x.Equipments == 0 ? 0 : x.WorkTimeEffectiveProcess / (x.TotalTime * x.Equipments),
                    Utility = x.TotalTime * x.Equipments == 0 ? 0 : x.WorkTime / (x.TotalTime * x.Equipments)
                });

            var closedKey = closed.ToDictionary(
                c => (c.EquipmentGroupId, c.TypeEquipmentId, c.Equipments),
                c => c);

            return all.Select(a =>
            {
                closedKey.TryGetValue((a.EquipmentGroupId, a.TypeEquipmentId, a.Equipments), out var cd);
                var eff = cd?.Efficiency ?? 0;
                var prod = cd?.Productivity ?? 0;
                var util = cd?.Utility ?? 0;
                var workClosed = cd?.WorkTime ?? 0;

                return new WFMLaborEquipment
                {
                    Id = Guid.NewGuid(),
                    EquipmentGroupId = a.EquipmentGroupId!.Value,
                    TypeEquipmentId = a.TypeEquipmentId,
                    Equipments = a.Equipments,
                    InitDate = a.InitDate,
                    EndDate = a.EndDate,
                    PlanningId = a.PlanningId,
                    WorkTime = Math.Round(a.WorkTime, 2),
                    Efficiency = Math.Round(eff * 100, 2),
                    Productivity = Math.Round(prod * 100, 2),
                    Utility = Math.Round(util * 100, 2),
                    TotalOrders = a.TotalOrders,
                    ClosedOrders = a.ClosedOrders,
                    TotalUtility = Math.Round(a.TotalUtility * 100, 2),
                    TotalProductivity = Math.Round(a.TotalProductivity * 100, 2),
                    Progress = a.WorkTime == 0 ? 0 : Math.Round((workClosed / a.WorkTime) * 100, 2)
                };
            });
        }

        /// <summary>
        /// Calculates the general data of WFMLaborEquipmentPerProcessType
        /// </summary>
        /// <param name="itemPlannings">ItemPlannings to calculate the labor stats</param>
        /// <param name="shifts">Shifts data from database</param>
        /// <param name="steps">Steps data from database</param>
        /// <param name="processes">Porcesses data from database</param>
        /// <param name="wfmLaborEquipmentsPerFlow">IEnumerable of the previously calculated WFMLaborEquipmentPerFlow to relate with these new stats</param>
        /// <param name="now">UTC now DateTime</param>
        /// <returns>IEnumerable of WFMLaborEquipmentPerProcessType stats</returns>
        public static IEnumerable<WFMLaborEquipmentPerProcessType> WFMLaborPerProcessEquipment(
            IEnumerable<ItemPlanning> itemPlannings,
            List<Shift> shifts,
            List<Step> steps,
            List<Process> processes,
            IEnumerable<WFMLaborEquipmentPerFlow> wfmLaborEquipmentsPerFlow,
            DateTime now)
        {
            var items = itemPlannings as IList<ItemPlanning> ?? itemPlannings.ToList();
            var stps = steps;
            var procs = processes;
            var perFlow = wfmLaborEquipmentsPerFlow as IList<WFMLaborEquipmentPerFlow> ?? wfmLaborEquipmentsPerFlow.ToList();

            var stepEdgeTimeByProcessId = stps
                .Where(s => s.InitProcess || s.EndProcess)
                .GroupBy(s => s.ProcessId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TimeMin));

            var procById = procs.ToDictionary(p => p.Id);
            var isOutByName = procs.ToDictionary(p => p.Name, p => p.IsOut);

            var all = items.Where(x => x.EquipmentGroup != null && x.EquipmentGroupId != null).Select(x => new
            {
                EquipmentGroupId = x.EquipmentGroupId,
                TypeEquipmentId = x.EquipmentGroup!.TypeEquipmentId,
                Equipments = x.EquipmentGroup.Equipments,
                InitDate = x.InitDate,
                EndDate = x.EndDate,
                PlanningId = x.WorkOrderPlanning.PlanningId,
                ProcessType = x.Process.Name,
                WorkTime = x.WorkTime,
                TotalOrders = x.WorkOrderPlanning.InputOrderId,
                ClosedOrders = x.WorkOrderPlanning.InputOrder.Status == OrderStatus.Closed ? x.WorkOrderPlanning.InputOrderId : Guid.Empty,
                TotalProcesses = x.ProcessId,
                ClosedProcesses = x.Progress == 100 ? x.ProcessId : Guid.Empty,
                WorkTimeEffectiveProcess = x.Process.IsEffective ? x.WorkTime : 0
            })
            .GroupBy(x => (x.ProcessType, x.EquipmentGroupId, x.TypeEquipmentId, x.Equipments, x.PlanningId))
            .Select(g => new
            {
                EquipmentGroupId = g.Key.EquipmentGroupId,
                TypeEquipmentId = g.Key.TypeEquipmentId,
                Equipments = g.Key.Equipments,
                InitDate = g.Min(x => x.InitDate),
                EndDate = g.Max(x => x.EndDate),
                PlanningId = g.Key.PlanningId,
                ProcessType = g.Key.ProcessType,
                WorkTime = g.Sum(m => m.WorkTime),
                TotalOrders = g.Select(m => m.TotalOrders).Distinct().Count(),
                ClosedOrders = g.Where(m => m.ClosedOrders != Guid.Empty).Select(x => x.ClosedOrders).Distinct().Count(),
                TotalProcesses = g.Select(m => m.TotalProcesses).Count(),
                ClosedProcesses = g.Where(m => m.ClosedProcesses != Guid.Empty).Select(x => x.ClosedProcesses).Count(),
                WorkTimeEffectiveProcess = g.Sum(m => m.WorkTimeEffectiveProcess),
                TotalWorkTime = CalculateTotalScheduleTime(false, shifts, now)
            })
            .Select(x => new
            {
                x.EquipmentGroupId,
                x.TypeEquipmentId,
                x.Equipments,
                x.InitDate,
                x.EndDate,
                x.PlanningId,
                x.ProcessType,
                x.WorkTime,
                x.TotalOrders,
                x.ClosedOrders,
                x.TotalProcesses,
                x.ClosedProcesses,
                TotalProductivity = x.TotalWorkTime * x.Equipments == 0 ? 0 : x.WorkTimeEffectiveProcess / (x.TotalWorkTime * x.Equipments),
                TotalUtility = x.TotalWorkTime * x.Equipments == 0 ? 0 : x.WorkTime / (x.TotalWorkTime * x.Equipments)
            });

            var closed = items.Where(x => x.EquipmentGroup != null && x.EquipmentGroupId != null).Where(m => m.Progress == 100)
                .Select(x =>
                {
                    var p = procById[x.ProcessId];
                    var stepEdges = stepEdgeTimeByProcessId.TryGetValue(x.ProcessId, out var te) ? te : 0;
                    return new
                    {
                        EquipmentGroupId = x.EquipmentGroupId,
                        TypeEquipmentId = x.EquipmentGroup.TypeEquipmentId,
                        Equipments = x.EquipmentGroup.Equipments,
                        InitDate = x.InitDate,
                        EndDate = x.EndDate,
                        PlanningId = x.WorkOrderPlanning.PlanningId,
                        ProcessType = x.Process.Name,
                        WorkTime = x.WorkTime,
                        WorkTimeEffectiveProcess = p.IsEffective ? x.WorkTime : 0,
                        WorkTheoricalTime = p.MinTime + (p.PreprocessTime ?? 0) + (p.PostprocessTime ?? 0) + stepEdges
                    };
                })
                .GroupBy(x => (x.ProcessType, x.EquipmentGroupId, x.TypeEquipmentId, x.Equipments, x.PlanningId))
                .Select(g => new
                {
                    EquipmentGroupId = g.Key.EquipmentGroupId,
                    TypeEquipmentId = g.Key.TypeEquipmentId,
                    Equipments = g.Key.Equipments,
                    InitDate = g.Min(x => x.InitDate),
                    EndDate = g.Max(x => x.EndDate),
                    PlanningId = g.Key.PlanningId,
                    ProcessType = g.Key.ProcessType,
                    TotalTime = CalculateTotalScheduleTime(true, shifts, now),
                    WorkTime = g.Sum(m => m.WorkTime),
                    WorkTimeEffectiveProcess = g.Sum(m => m.WorkTimeEffectiveProcess),
                    WorkTheoricalTime = g.Sum(m => m.WorkTheoricalTime)
                })
                .Select(x => new
                {
                    x.EquipmentGroupId,
                    x.TypeEquipmentId,
                    x.Equipments,
                    x.InitDate,
                    x.EndDate,
                    x.PlanningId,
                    x.ProcessType,
                    x.WorkTime,
                    Efficiency = x.WorkTheoricalTime == 0 ? 0 : x.WorkTheoricalTime / x.WorkTime,
                    Productivity = x.TotalTime * x.Equipments == 0 ? 0 : x.WorkTimeEffectiveProcess / (x.TotalTime * x.Equipments),
                    Utility = x.TotalTime * x.Equipments == 0 ? 0 : x.WorkTime / (x.TotalTime * x.Equipments)
                });

            var closedKey = closed.ToDictionary(
                c => (c.EquipmentGroupId, c.TypeEquipmentId, c.Equipments, c.ProcessType),
                c => c);

            var perFlowKey = perFlow.ToDictionary(
                l => (l.EquipmentGroupId, l.TypeEquipmentId, l.IsOutbound), l => l.Id);

            var laborEquipmentPerProcessType = all.Select(a =>
            {
                closedKey.TryGetValue((a.EquipmentGroupId, a.TypeEquipmentId, a.Equipments, a.ProcessType), out var cd);
                var eff = cd?.Efficiency ?? 0;
                var prod = cd?.Productivity ?? 0;
                var util = cd?.Utility ?? 0;
                var workClosed = cd?.WorkTime ?? 0;
                var isOut = isOutByName.TryGetValue(a.ProcessType, out var v) ? v : false;

                return new
                {
                    EquipmentGroupId = a.EquipmentGroupId!.Value,
                    a.TypeEquipmentId,
                    a.Equipments,
                    a.InitDate,
                    a.EndDate,
                    a.PlanningId,
                    a.ProcessType,
                    IsOutbound = isOut,
                    WorkTime = a.WorkTime,
                    Efficiency = eff,
                    Productivity = prod,
                    Utility = util,
                    TotalOrders = a.TotalOrders,
                    ClosedOrders = a.ClosedOrders,
                    TotalProcesses = a.TotalProcesses,
                    ClosedProcesses = a.ClosedProcesses,
                    TotalUtility = a.TotalUtility,
                    TotalProductivity = a.TotalProductivity,
                    Progress = a.WorkTime == 0 ? 0 : Math.Round((workClosed / a.WorkTime) * 100, 2)
                };
            });

            return laborEquipmentPerProcessType
                .Where(lpp => perFlowKey.ContainsKey((lpp.EquipmentGroupId, lpp.TypeEquipmentId, lpp.IsOutbound)))
                .Select(lpp => new WFMLaborEquipmentPerProcessType
                {
                    Id = Guid.NewGuid(),
                    EquipmentGroupId = lpp.EquipmentGroupId,
                    TypeEquipmentId = lpp.TypeEquipmentId,
                    Equipments = lpp.Equipments,
                    InitDate = lpp.InitDate,
                    EndDate = lpp.EndDate,
                    PlanningId = lpp.PlanningId,
                    ProcessType = lpp.ProcessType,
                    WorkTime = Math.Round(lpp.WorkTime, 2),
                    Efficiency = Math.Round(lpp.Efficiency * 100, 2),
                    Productivity = Math.Round(lpp.Productivity * 100, 2),
                    Utility = Math.Round(lpp.Utility * 100, 2),
                    TotalOrders = lpp.TotalOrders,
                    ClosedOrders = lpp.ClosedOrders,
                    TotalProcesses = lpp.TotalProcesses,
                    ClosedProcesses = lpp.ClosedProcesses,
                    WFMLaborEquipmentPerFlowId = perFlowKey[(lpp.EquipmentGroupId, lpp.TypeEquipmentId, lpp.IsOutbound)],
                    TotalUtility = Math.Round(lpp.TotalUtility * 100, 2),
                    TotalProductivity = Math.Round(lpp.TotalProductivity * 100, 2),
                    Progress = lpp.Progress
                });
        }

        /// <summary>
        /// Calculates the general data of WFMLaborEquipmentPerFlow
        /// </summary>
        /// <param name="itemPlannings">ItemPlannings to calculate the labor stats</param>
        /// <param name="shifts">Shifts data from database</param>
        /// <param name="steps">Steps data from database</param>
        /// <param name="processes">Porcesses data from database</param>
        /// <param name="wfmLaborEquipment">IEnumerable of the previously calculated WFMLaborEquipment to relate with these new stats</param>
        /// <param name="now">UTC now DateTime</param>
        /// <returns>IEnumerable of WFMLaborEquipmentPerFlow stats</returns>
        public static IEnumerable<WFMLaborEquipmentPerFlow> WFMLaborEquipmentPerFlow(
            IEnumerable<ItemPlanning> itemPlannings,
            List<Shift> shifts,
            List<Step> steps,
            List<Process> processes,
            IEnumerable<WFMLaborEquipment> wfmLaborEquipment,
            DateTime now)
        {
            var items = itemPlannings as IList<ItemPlanning> ?? itemPlannings.ToList();
            var stps = steps;
            var procs = processes;
            var baseEquip = wfmLaborEquipment as IList<WFMLaborEquipment> ?? wfmLaborEquipment.ToList();

            var stepEdgeTimeByProcessId = stps
                .Where(s => s.InitProcess || s.EndProcess)
                .GroupBy(s => s.ProcessId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TimeMin));

            var procById = procs.ToDictionary(p => p.Id);

            var all = items.Where(x => x.EquipmentGroup != null && x.EquipmentGroupId != null).Select(x => new
            {
                EquipmentGroupId = x.EquipmentGroupId,
                TypeEquipmentId = x.EquipmentGroup!.TypeEquipmentId,
                Equipments = x.EquipmentGroup.Equipments,
                InitDate = x.InitDate,
                EndDate = x.EndDate,
                PlanningId = x.WorkOrderPlanning.PlanningId,
                IsOutbound = x.IsOutbound,
                WorkTime = x.WorkTime,
                TotalOrders = x.WorkOrderPlanning.InputOrderId,
                ClosedOrders = x.WorkOrderPlanning.InputOrder.Status == OrderStatus.Closed ? x.WorkOrderPlanning.InputOrderId : Guid.Empty,
                TotalProcesses = x.ProcessId,
                ClosedProcesses = x.Progress == 100 ? x.ProcessId : Guid.Empty,
                WorkTimeEffectiveProcess = x.Process.IsEffective ? x.WorkTime : 0
            })
            .GroupBy(x => (x.IsOutbound, x.EquipmentGroupId, x.TypeEquipmentId, x.Equipments, x.PlanningId))
            .Select(g => new
            {
                EquipmentGroupId = g.Key.EquipmentGroupId,
                TypeEquipmentId = g.Key.TypeEquipmentId,
                Equipments = g.Key.Equipments,
                InitDate = g.Min(x => x.InitDate),
                EndDate = g.Max(x => x.EndDate),
                PlanningId = g.Key.PlanningId,
                IsOutbound = g.Key.IsOutbound,
                WorkTime = g.Sum(m => m.WorkTime),
                TotalOrders = g.Select(m => m.TotalOrders).Distinct().Count(),
                ClosedOrders = g.Where(m => m.ClosedOrders != Guid.Empty).Select(x => x.ClosedOrders).Distinct().Count(),
                TotalProcesses = g.Select(m => m.TotalProcesses).Count(),
                ClosedProcesses = g.Where(m => m.ClosedProcesses != Guid.Empty).Select(x => x.ClosedProcesses).Count(),
                WorkTimeEffectiveProcess = g.Sum(m => m.WorkTimeEffectiveProcess),
                TotalWorkTime = CalculateTotalScheduleTime(false, shifts, now)
            })
            .Select(x => new
            {
                x.EquipmentGroupId,
                x.TypeEquipmentId,
                x.Equipments,
                x.InitDate,
                x.EndDate,
                x.PlanningId,
                x.IsOutbound,
                x.WorkTime,
                x.TotalOrders,
                x.ClosedOrders,
                x.TotalProcesses,
                x.ClosedProcesses,
                TotalProductivity = x.TotalWorkTime * x.Equipments == 0 ? 0 : x.WorkTimeEffectiveProcess / (x.TotalWorkTime * x.Equipments),
                TotalUtility = x.TotalWorkTime * x.Equipments == 0 ? 0 : x.WorkTime / (x.TotalWorkTime * x.Equipments)
            });

            var closed = items.Where(x => x.EquipmentGroup != null && x.EquipmentGroupId != null).Where(m => m.Progress == 100)
                .Select(x =>
                {
                    var p = procById[x.ProcessId];
                    var stepEdges = stepEdgeTimeByProcessId.TryGetValue(x.ProcessId, out var te) ? te : 0;
                    return new
                    {
                        EquipmentGroupId = x.EquipmentGroupId,
                        TypeEquipmentId = x.EquipmentGroup.TypeEquipmentId,
                        Equipments = x.EquipmentGroup.Equipments,
                        InitDate = x.InitDate,
                        EndDate = x.EndDate,
                        PlanningId = x.WorkOrderPlanning.PlanningId,
                        IsOutbound = x.IsOutbound,
                        WorkTime = x.WorkTime,
                        WorkTimeEffectiveProcess = p.IsEffective ? x.WorkTime : 0,
                        WorkTheoricalTime = p.MinTime + (p.PreprocessTime ?? 0) + (p.PostprocessTime ?? 0) + stepEdges
                    };
                })
                .GroupBy(x => (x.IsOutbound, x.EquipmentGroupId, x.TypeEquipmentId, x.Equipments, x.PlanningId))
                .Select(g => new
                {
                    EquipmentGroupId = g.Key.EquipmentGroupId,
                    TypeEquipmentId = g.Key.TypeEquipmentId,
                    Equipments = g.Key.Equipments,
                    InitDate = g.Min(x => x.InitDate),
                    EndDate = g.Max(x => x.EndDate),
                    PlanningId = g.Key.PlanningId,
                    IsOutbound = g.Key.IsOutbound,
                    TotalTime = CalculateTotalScheduleTime(true, shifts, now),
                    WorkTime = g.Sum(m => m.WorkTime),
                    WorkTimeEffectiveProcess = g.Sum(m => m.WorkTimeEffectiveProcess),
                    WorkTheoricalTime = g.Sum(m => m.WorkTheoricalTime)
                })
                .Select(x => new
                {
                    x.EquipmentGroupId,
                    x.TypeEquipmentId,
                    x.Equipments,
                    x.InitDate,
                    x.EndDate,
                    x.PlanningId,
                    x.IsOutbound,
                    x.WorkTime,
                    Efficiency = x.WorkTheoricalTime == 0 ? 0 : x.WorkTheoricalTime / x.WorkTime,
                    Productivity = x.TotalTime * x.Equipments == 0 ? 0 : x.WorkTimeEffectiveProcess / (x.TotalTime * x.Equipments),
                    Utility = x.TotalTime * x.Equipments == 0 ? 0 : x.WorkTime / (x.TotalTime * x.Equipments)
                });

            var closedKey = closed.ToDictionary(
                c => (c.EquipmentGroupId, c.TypeEquipmentId, c.Equipments, c.IsOutbound),
                c => c);

            var equipKey = baseEquip.ToDictionary(
                e => (e.EquipmentGroupId, e.TypeEquipmentId), e => e.Id);

            var equipmentPerFlow = all.Select(a =>
            {
                closedKey.TryGetValue((a.EquipmentGroupId, a.TypeEquipmentId, a.Equipments, a.IsOutbound), out var cd);
                var eff = cd?.Efficiency ?? 0;
                var prod = cd?.Productivity ?? 0;
                var util = cd?.Utility ?? 0;
                var workClosed = cd?.WorkTime ?? 0;

                return new
                {
                    EquipmentGroupId = a.EquipmentGroupId!.Value,
                    a.TypeEquipmentId,
                    a.Equipments,
                    a.InitDate,
                    a.EndDate,
                    a.PlanningId,
                    a.IsOutbound,
                    a.WorkTime,
                    Efficiency = eff,
                    Productivity = prod,
                    Utility = util,
                    a.TotalOrders,
                    a.ClosedOrders,
                    a.TotalProcesses,
                    a.ClosedProcesses,
                    a.TotalProductivity,
                    a.TotalUtility,
                    Progress = a.WorkTime == 0 ? 0 : Math.Round((workClosed / a.WorkTime) * 100, 2)
                };
            });

            return equipmentPerFlow
                .Where(epf => equipKey.ContainsKey((epf.EquipmentGroupId, epf.TypeEquipmentId)))
                .Select(epf => new WFMLaborEquipmentPerFlow
                {
                    Id = Guid.NewGuid(),
                    EquipmentGroupId = epf.EquipmentGroupId,
                    TypeEquipmentId = epf.TypeEquipmentId,
                    Equipments = epf.Equipments,
                    InitDate = epf.InitDate,
                    EndDate = epf.EndDate,
                    PlanningId = epf.PlanningId,
                    IsOutbound = epf.IsOutbound,
                    WorkTime = Math.Round(epf.WorkTime, 2),
                    Efficiency = Math.Round(epf.Efficiency * 100, 2),
                    Productivity = Math.Round(epf.Productivity * 100, 2),
                    Utility = Math.Round(epf.Utility * 100, 2),
                    TotalOrders = epf.TotalOrders,
                    ClosedOrders = epf.ClosedOrders,
                    TotalProcesses = epf.TotalProcesses,
                    ClosedProcesses = epf.ClosedProcesses,
                    WFMLaborEquipmentId = equipKey[(epf.EquipmentGroupId, epf.TypeEquipmentId)],
                    TotalProductivity = Math.Round(epf.TotalProductivity * 100, 2),
                    TotalUtility = Math.Round(epf.TotalUtility * 100, 2),
                    Progress = epf.Progress
                });
        }

        #endregion

        #region Private

        /// <summary>
        /// Calculates the work time based on the shifts for the warehouse
        /// </summary>
        /// <param name="isClosed">Boolean to define if the calculations should be made for closed processes or the total planning</param>
        /// <param name="shifts">Shifts of the specific warehouse</param>
        /// <param name="now">Now DateTime to have as a reference</param>
        /// <returns></returns>
        private static double CalculateTotalScheduleTime(bool isClosed, IEnumerable<Shift> shifts, DateTime now)
        {
            double startDate = now.Date.TimeOfDay.TotalHours;
            double endDate = isClosed ? now.TimeOfDay.TotalHours : now.Date.AddDays(1).AddTicks(-1).TimeOfDay.TotalHours;
            return CalculateWorkTime(shifts, startDate, endDate);
        }

        /// <summary>
        /// Calculates the work time of the shifts based on the reference dates given as parameters
        /// </summary>
        /// <param name="shifts">Shifts of the specific warehouse</param>
        /// <param name="startDate">Start date of the part to calculate the work time</param>
        /// <param name="endDate">End date of the part to calculate the work time</param>
        /// <returns></returns>
        private static double CalculateWorkTime(IEnumerable<Shift> shifts, double startDate, double endDate)
        {
            double workTime = 0;

            foreach (var s in shifts)
            {
                double shiftStart = s.InitHour;
                double shiftEnd = s.EndHour;

                if (shiftEnd >= shiftStart)
                {
                    double effectiveStart = Math.Max(shiftStart, startDate);
                    double effectiveEnd = Math.Min(shiftEnd, endDate);

                    if (effectiveStart < effectiveEnd)
                        workTime += effectiveEnd - effectiveStart;
                }
                else
                {
                    double effectiveStart1 = Math.Max(shiftStart, startDate);
                    double effectiveEnd1 = endDate;

                    if (effectiveStart1 < effectiveEnd1)
                        workTime += effectiveEnd1 - effectiveStart1;
                }
            }

            return workTime * 3600;
        }

        /// <summary>
        /// Calculates the workshift time, taking into account the breaks of the shift
        /// </summary>
        /// <param name="progress">Progress of the ItemPlanning to know if the process is closed or open</param>
        /// <param name="schedule">Schedule asociated to the worker</param>
        /// <param name="breaks">List of breaks to find the ones in the schedule</param>
        /// <param name="now">Reference Utc now DateTime</param>
        /// <returns>Seconds of the warokshift in the period of the processes</returns>
        private static double CalculateWorkShiftTime(double progress, Schedule schedule, IEnumerable<Break> breaks, DateTime now)
        {
            var initShift = schedule.Shift.InitHour;
            var endShift = schedule.Shift.EndHour;

            bool midnightCross = initShift > endShift;
            endShift += midnightCross ? 24 : 0;

            var nowHours = now.TimeOfDay.TotalHours;
            var breaksInSchedule = breaks.Where(x => x.BreakProfileId == schedule.BreakProfileId);

            if (progress == 100)
            {
                var limitTime = Math.Min(endShift, nowHours);

                double breaksTime = breaksInSchedule
                    .Select(x => new
                    {
                        InitBreak = x.InitBreak,
                        EndBreak = x.InitBreak > x.EndBreak ? x.EndBreak + 24 : x.EndBreak
                    })
                    .Where(x => x.InitBreak >= initShift && x.InitBreak < limitTime)
                    .Sum(x => Math.Min(x.EndBreak, limitTime) - x.InitBreak) * 3600;

                return ((limitTime - initShift) * 3600) - breaksTime;
            }
            else
            {
                if (initShift < nowHours)
                {
                    double breaksTime = breaksInSchedule
                        .Select(x => new
                        {
                            InitBreak = x.InitBreak,
                            EndBreak = x.InitBreak > x.EndBreak ? x.EndBreak + 24 : x.EndBreak
                        })
                        .Sum(x => x.EndBreak - x.InitBreak);

                    return (endShift - initShift) * 3600 - breaksTime;
                }
                else
                {
                    var limitTime = Math.Max(initShift, nowHours);

                    double breaksTime = breaksInSchedule
                        .Select(x => new
                        {
                            InitBreak = x.InitBreak,
                            EndBreak = x.InitBreak > x.EndBreak ? x.EndBreak + 24 : x.EndBreak
                        })
                        .Where(x => x.EndBreak > limitTime)
                        .Sum(x => x.EndBreak - Math.Max(x.InitBreak, limitTime)) * 3600;

                    return ((endShift - limitTime) * 3600) - breaksTime;
                }
            }
        }

        #endregion
    }
}
