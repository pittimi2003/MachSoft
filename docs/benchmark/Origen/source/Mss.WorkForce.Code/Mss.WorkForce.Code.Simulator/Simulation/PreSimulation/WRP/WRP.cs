using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Simulator.Helper;
using Warehouse = Mss.WorkForce.Code.Simulator.Core.Layout.Warehouse;

namespace Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.WRP
{
    public class WRP
    {
        #region Attributes
        private Warehouse warehouse;
        private DataSimulatorTablaRequest data;
        private Dictionary<Guid, double> fromRouteCache;
        #endregion

        #region Constructor
        public WRP(Warehouse warehouse, DataSimulatorTablaRequest data)
        {
            this.data = data;
            this.warehouse = warehouse;
            this.fromRouteCache = new Dictionary<Guid, double>();
        }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Calculates the MaxOnTimeDate for having a limit priority date and the MaxMarginDate for having a limit priority date before a process becomes delayed
        /// </summary>
        /// <returns>Warehouse with the processes of its order with the dates calculated</returns>
        public Warehouse CalculateLimitDate(double percentage)
        {
            // Calculation of Shifts and Breaks
            var customTimeTable = GetTimeTable();
            var customTimeTableWithBreaks = GetTimeTableWithBreaks(customTimeTable);

            // Iteration between Orders
            foreach (var o in warehouse.Orders)
            {
                var order = data.InputOrder!.FirstOrDefault(x => x.Id == o.Id);

                var maxPosition = o.Grouping.Max(x => x.Position);
                var minPosition = o.Grouping.Min(x => x.Position);
                var maxPositionGroups = o.Grouping.Where(x => x.Position == maxPosition);
                var minPositionGroups = o.Grouping.Where(x => x.Position == minPosition);

                if (order != null && order.IsOutbound)
                {
                    o.Grouping = o.Grouping.OrderBy(x => x.Position).ToList();

                    var appointment = warehouse.Appointments.FirstOrDefault(x => x.AppointmentDate == order.AppointmentDate && x.VehicleCode == order.VehicleCode);
                    var referenceDate = appointment!.EndDate;

                    var positions = o.Grouping.Select(x => x.Position).Distinct().OrderByDescending(x => x);

                    foreach (var p in positions)
                    {
                        var positionProcesses = o.Grouping.Where(x => x.Position == p);
                        foreach (var process in positionProcesses)
                        {
                            var initSteps = data.Step!.Where(x => x.InitProcess && x.ProcessId == process.Process.Id).Sum(x => x.TimeMin).GetValueOrDefault(0);
                            var endSteps = data.Step!.Where(x => x.EndProcess && x.ProcessId == process.Process.Id).Sum(x => x.TimeMin).GetValueOrDefault(0);
                            var routeTime = GetRouteTime(process.Process.Area.Id);

                            var duration = process.Duration + initSteps + endSteps + routeTime;

                            if (p == maxPosition)
                            {
                                process.MaxOnTimeDate = referenceDate.Add(-TimeSpan.FromSeconds(duration));
                            }
                            else
                            {
                                var previousMaxDate = o.Grouping.Where(x => x.Position == p + 1).Min(x => x.MaxOnTimeDate);
                                process.MaxOnTimeDate = previousMaxDate!.Value.Add(-TimeSpan.FromSeconds(duration));
                            }

                            var lastMaxOnDateValue = process.MaxOnTimeDate.Value;

                            // Delay by Shifts & Breaks
                            process.MaxOnTimeDate = SetDelayByTimeTable(process.MaxOnTimeDate, duration, customTimeTableWithBreaks);

                            // Margin Date Calculation & Breaks
                            var percentageSeconds = (1 + percentage) * duration;
                            process.MaxMarginDate = process.MaxOnTimeDate.Value.Add(-TimeSpan.FromSeconds(percentageSeconds));

                            var lastMaxMarginDateValue = process.MaxMarginDate.Value;
                            process.MaxMarginDate = SetDelayByTimeTable(process.MaxMarginDate, 0, customTimeTableWithBreaks);
                        }
                    }
                }
                else if (order != null && !order.IsOutbound && minPositionGroups.Any(x => x.Process.ProcessType == ProcessType.Inbound))
                {
                    var inboundProcess = o.Grouping[0];

                    // Duration Process Calculation
                    var initSteps = data.Step!.Where(x => x.InitProcess && x.ProcessId == o.Grouping[0].Process.Id).Sum(x => x.TimeMin).GetValueOrDefault(0);
                    var endSteps = data.Step!.Where(x => x.EndProcess && x.ProcessId == o.Grouping[0].Process.Id).Sum(x => x.TimeMin).GetValueOrDefault(0);
                    var routeTime = GetRouteTime(inboundProcess.Process.Area.Id);

                    var duration = o.Grouping[0].Duration + initSteps + endSteps + routeTime;

                    var appointment = warehouse.Appointments.FirstOrDefault(x => x.AppointmentDate == order.AppointmentDate && x.VehicleCode == order.VehicleCode);
                    var referenceDate = appointment!.EndDate;

                    o.Grouping[0].MaxOnTimeDate = referenceDate.Add(-TimeSpan.FromSeconds(duration));

                    // Delay by Shifts & Breaks
                    o.Grouping[0].MaxOnTimeDate = SetDelayByTimeTable(inboundProcess.MaxOnTimeDate, duration, customTimeTableWithBreaks);

                    // Margin Date Calculation & Delay
                    var percentageSeconds = (1 + percentage) * duration;
                    o.Grouping[0].MaxMarginDate = inboundProcess.MaxOnTimeDate.Value.Add(-TimeSpan.FromSeconds(percentageSeconds));
                    o.Grouping[0].MaxMarginDate = SetDelayByTimeTable(inboundProcess.MaxMarginDate, 0, customTimeTableWithBreaks);
                }
            }

            return warehouse;
        }
     
        private double GetRouteTime(Guid to)
        {
            try
            {
                if (fromRouteCache.TryGetValue(to, out var time))
                    return time;
                time = warehouse.Routes
                    .Where(x => x.ArrivalAreaId == to).Max(x => x.Time);
                fromRouteCache[to] = time;
                return time;
            }
            catch (Exception ex) 
            {
                Console.WriteLine("[WRP] [Error] Error getting route time: "+ ex);
                return 0;
            }
        }

        private IEnumerable<TimeTable> GetTimeTable()
        {
            try
            {
                return data.Shift!.Select(x => new TimeTable
                {
                    InitHour = x.InitHour,
                    EndHour = x.EndHour
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WRP] [Error] Error getting time table: " + ex);
                return Enumerable.Empty<TimeTable>();
            }
        }
        private Dictionary<TimeTable,IEnumerable<TimeTable>> GetTimeTableWithBreaks(IEnumerable<TimeTable> TimeTable)
        {
            Dictionary<TimeTable, IEnumerable<TimeTable>> breaksByShift = new Dictionary<TimeTable, IEnumerable<TimeTable>>();

            foreach (TimeTable timeTable in TimeTable)
            {
                try
                {
                    var customBreaks = data.Break!
                        .Where(x => x.InitBreak >= timeTable.InitHour && x.EndBreak <= timeTable.EndHour)
                        .Select(x => new TimeTable
                        {
                            InitHour = x.InitBreak,
                            EndHour = x.EndBreak,
                        });
                    breaksByShift.Add(timeTable, customBreaks);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[WRP] [Error] Error getting time table with breaks: " + ex);
                    breaksByShift.Add(timeTable, Enumerable.Empty<TimeTable>());
                }
            }
            return breaksByShift;
        }
        private DateTime? SetDelayByTimeTable(DateTime? timeDate, double duration, Dictionary<TimeTable, IEnumerable<TimeTable>> customTimeTableWithBreaks)
        {
            if (timeDate == null) return null;

            double initDateHour = timeDate.Value.TimeOfDay.TotalHours;
            var currentShift = customTimeTableWithBreaks.Keys.FirstOrDefault(x => x.InitHour <= initDateHour && x.EndHour >= initDateHour);
            double endDateHour = timeDate.Value.AddSeconds(duration).TimeOfDay.TotalHours;

            if (!customTimeTableWithBreaks.Any(kvp =>
            {
                var insideShift =
                    initDateHour >= kvp.Key.InitHour &&
                    endDateHour <= kvp.Key.EndHour;

                var outsideBreaks =
                    kvp.Value.All(b =>
                        endDateHour <= b.InitHour ||
                        initDateHour >= b.EndHour);

                return insideShift && outsideBreaks;
            }))
            {
                var nearestShifts = customTimeTableWithBreaks.Keys
                    .Where(x => currentShift != null
                        ? x.EndHour <= currentShift.EndHour
                        : x.EndHour <= endDateHour)
                    .Where(x =>
                    {
                        var remainingShiftTime = initDateHour - x.InitHour;

                        var remainingBreaksTime =
                            customTimeTableWithBreaks[x]
                                .Where(b => b.InitHour < initDateHour)
                                .Sum(b => Math.Min(initDateHour, b.EndHour) - b.InitHour);
                        var test = remainingShiftTime - remainingBreaksTime;
                        return remainingShiftTime - remainingBreaksTime >= (duration/3600);
                    })
                    .OrderByDescending(x => x.EndHour);

                // Check if there is no nearest shift for the current day
                if (!nearestShifts.Any())
                {
                    // Configure the last shift of the previous day
                    nearestShifts = customTimeTableWithBreaks.Keys
                    .Where(x => x.EndHour <= timeDate.Value.Hour + 24)
                    .OrderByDescending(x => x.EndHour);

                    timeDate = timeDate.Value.AddDays(-1);
                }

                // Iterate on each shift (in case the nearest one can't process the order)
                foreach (var nearestShift in nearestShifts)
                {
                    var endShift = timeDate.Value.Date.AddHours(nearestShift.EndHour);
                    var timeDateWithBreaks = currentShift != nearestShift ? endShift.AddSeconds(-duration) : timeDate;

                    // Adding breaks duration
                    double availableWorkersTime = 0;
                    double remainingWorkTime = duration;
                    var lastStep = timeDateWithBreaks.Value.TimeOfDay.TotalHours;
                    var breaksByInitHour = customTimeTableWithBreaks[nearestShift].OrderByDescending(b => b.InitHour);

                    if (breaksByInitHour.Any())
                    {
                        // Iterate on each break to determine the time stimation
                        foreach (var brk in breaksByInitHour)
                        {
                            // Check if the break is in the future of the current date
                            if (brk.InitHour < lastStep && brk.EndHour < lastStep)
                            {
                                availableWorkersTime += (lastStep - brk.EndHour) * 3600;
                                // Check if the process can be executed after the next break
                                if (availableWorkersTime >= duration)
                                    return timeDate.Value.Date.AddSeconds(lastStep * 3600 - remainingWorkTime);
                                else
                                {
                                    // Store the already processed time to complete the process before the last break
                                    remainingWorkTime = duration - availableWorkersTime;
                                    lastStep = brk.InitHour;
                                }
                            }
                            else lastStep = Math.Min(lastStep, brk.InitHour);
                            // Check if there are not more breaks (Finish the process at the beg. of the shift or move to the previous shift)
                            if (breaksByInitHour.Last().InitHour == brk.InitHour && breaksByInitHour.Last().EndHour == brk.EndHour)
                            {
                                availableWorkersTime += (lastStep - nearestShift.InitHour) * 3600;
                                if (availableWorkersTime >= duration)
                                    return timeDate.Value.Date.AddSeconds(lastStep * 3600 - duration);
                            }
                        }
                    }
                    return timeDateWithBreaks;
                }
                return timeDate;
            }
            else return timeDate;
        }
        private class TimeTable
        {
            public double InitHour { get;set; }
            public double EndHour { get; set; }
        }
        #endregion

        #endregion
    }
}
