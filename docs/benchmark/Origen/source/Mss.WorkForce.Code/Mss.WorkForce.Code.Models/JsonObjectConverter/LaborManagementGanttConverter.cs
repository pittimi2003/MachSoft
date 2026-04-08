using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.JsonObjectConverter;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.ModelGantt.DataGanttPlanning;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models
{
    public class LaborManagementGanttConverter
    {
        #region Methods

        public static GanttDataConvertDto<LaborEquipmentGantt> LaborEquipmentsTaskConverter(IEnumerable<WFMLaborEquipment> laborEquipmentsCollections, IEnumerable<WFMLaborItemPlanning> laborItemPlanningCollections, UserFormatOptions userFormat)
        {
            Dictionary<Guid, LaborEquipmentGantt> laborTaskDictionary = new Dictionary<Guid, LaborEquipmentGantt>();
            List<LaborEquipmentGantt> taskEquipments = new();
            try
            {
                // Creamos los padres
                foreach (var laborEquipment in laborEquipmentsCollections)
                {
                    LaborEquipmentGantt laborTask = new(userFormat)
                    {
                        id = laborEquipment.Id,
                        TypeEquipmentName = laborEquipment.TypeEquipment.Name,
                        EquipmentGroupName = laborEquipment.EquipmentGroup.Name,
                        Equipments = laborEquipment.Equipments,
                        WorkTime = TimeFormatter.GetFormattedWorkTime(laborEquipment.WorkTime),
                        Efficiency = Math.Round(laborEquipment.Efficiency, 2),
                        Productivity = Math.Round(laborEquipment.Productivity, 2),
                        TotalProductivity = Math.Round(laborEquipment.TotalProductivity, 2),
                        Utility = Math.Round(laborEquipment.Utility, 2),
                        TotalUtility = Math.Round(laborEquipment.TotalUtility, 2),
                        TotalOrders = laborEquipment.TotalOrders,
                        ClosedOrders = laborEquipment.ClosedOrders,
                        TooltipType = GanttTooltipType.LaborEquipmentGeneral,
                        progress = Convert.ToInt32(laborEquipment.Progress),
                        FillProgress = true,
                        levelTask = 1,
                    };
                    laborTaskDictionary.Add(laborTask.id, laborTask);

                    var itemPlanningsForEquipments = laborItemPlanningCollections.Where(x => x.EquipmentGroupId == laborEquipment.EquipmentGroupId);
                    foreach (var itemPlanning in itemPlanningsForEquipments)
                    {
                        LaborEquipmentGantt orderPlanningTaskLevel2;

                        var orderWorkerPerFlow = itemPlanning.WFMLaborEquipmentPerProcessType?.WFMLaborEquipmentPerFlow;
                        var fechainicio = itemPlanningsForEquipments.Where(x => x.InputOrder.IsOutbound == orderWorkerPerFlow.IsOutbound).Min(x => x.InitDate);
                        var fechaFin = itemPlanningsForEquipments.Where(x => x.InputOrder.IsOutbound == orderWorkerPerFlow.IsOutbound).Max(x => x.EndDate);
                        if (!laborTaskDictionary.TryGetValue(orderWorkerPerFlow.Id, out orderPlanningTaskLevel2))
                        {
                            orderPlanningTaskLevel2 = new LaborEquipmentGantt(userFormat)
                            {
                                id = orderWorkerPerFlow.Id,
                                parentId = laborTask.id,
                                IsOutbound = orderWorkerPerFlow.IsOutbound,
                                StartDate = fechainicio,
                                EndDate = fechaFin,
                                TooltipType = GanttTooltipType.LaborEquipmentOrder,
                                Equipments = orderWorkerPerFlow.Equipments,
                                WorkTime = TimeFormatter.GetFormattedWorkTime(orderWorkerPerFlow.WorkTime),
                                Efficiency = orderWorkerPerFlow.Efficiency,
                                Productivity = orderWorkerPerFlow.Productivity,
                                TotalProductivity = orderWorkerPerFlow.TotalProductivity,
                                Utility = orderWorkerPerFlow.Utility,
                                TotalUtility = orderWorkerPerFlow.TotalUtility,
                                TotalOrders = orderWorkerPerFlow.TotalOrders,
                                ClosedOrders = orderWorkerPerFlow.ClosedOrders,
                                CommintedDate = itemPlanning.InputOrder.AppointmentDate,
                                levelTask = 2,
                            };

                            // Añadimos tarea del gannt Nivel 2
                            laborTaskDictionary.Add(orderWorkerPerFlow.Id, orderPlanningTaskLevel2);
                        }

                        LaborEquipmentGantt processTypeTask;

                        var processOrder = itemPlanning.WFMLaborEquipmentPerProcessType;
                        if (!laborTaskDictionary.TryGetValue(processOrder.Id, out processTypeTask))
                        {
                            var fechainicio_ = itemPlanningsForEquipments.Where(x => x.WFMLaborEquipmentPerProcessType.ProcessType == processOrder.ProcessType).Min(x => x.InitDate);
                            var fechaFin_ = itemPlanningsForEquipments.Where(x => x.WFMLaborEquipmentPerProcessType.ProcessType == processOrder.ProcessType).Max(x => x.EndDate);
                            processTypeTask = new LaborEquipmentGantt(userFormat)
                            {
                                id = processOrder.Id,
                                parentId = orderWorkerPerFlow.Id,
                                progress = Convert.ToInt32(itemPlanning.InputOrder.Progress),
                                IsOutbound = itemPlanning.InputOrder.IsOutbound,
                                ActivityTitle = processOrder.ProcessType,
                                StartDate = fechainicio_,
                                EndDate = fechaFin_,
                                TooltipType = GanttTooltipType.LaborEquipmentActivity,
                                Efficiency = processOrder.Efficiency,
                                Productivity = processOrder.Productivity,
                                TotalProductivity = processOrder.TotalProductivity,
                                Utility = processOrder.Utility,
                                TotalUtility = processOrder.TotalUtility,
                                TotalActivities = processOrder.TotalProcesses,
                                ClosedActivities = processOrder.ClosedProcesses,
                                Equipments = processOrder.Equipments,
                                WorkTime = TimeFormatter.GetFormattedWorkTime(processOrder.WorkTime),
                                levelTask = 3,
                            };

                            laborTaskDictionary.Add(processTypeTask.id, processTypeTask);
                            laborTaskDictionary[processOrder.Id].segments = AddSegmentsForActivity(laborTaskDictionary[processOrder.Id], itemPlanning.Id, orderPlanningTaskLevel2.UtcToTimeZoneOffSet(itemPlanning.InitDate).DateTime, orderPlanningTaskLevel2.UtcToTimeZoneOffSet(itemPlanning.EndDate).DateTime, itemPlanning.InputOrder.Progress ?? 0, itemPlanning);
                        }
                        else
                        {
                            laborTaskDictionary[processOrder.Id].segments  = AddSegmentsForActivity(laborTaskDictionary[processOrder.Id], itemPlanning.Id, orderPlanningTaskLevel2.UtcToTimeZoneOffSet(itemPlanning.InitDate).DateTime, orderPlanningTaskLevel2.UtcToTimeZoneOffSet(itemPlanning.EndDate).DateTime, itemPlanning.InputOrder.Progress ?? 0, itemPlanning);
                        }

                        laborTaskDictionary[orderPlanningTaskLevel2.id].progress = getProgress(laborTaskDictionary[processOrder.Id].segments);
                    }

                }

                taskEquipments = TasksIndexConverter.AssignRootIndex((laborTaskDictionary.Values.ToList().Cast<LaborEquipmentGantt>().ToList()));

            }
            catch (Exception)
            {
                throw;
            }
             return new GanttDataConvertDto<LaborEquipmentGantt>
            {
                TaskGantt = taskEquipments,
                DependenciesGantt = new List<DependenciesGantt>(),
            };
        }

        public static GanttDataConvertDto<LaborTaskGantt> LaborWorkersTaskConverter(IEnumerable<WFMLaborWorker> laborWorkerCollections, IEnumerable<WFMLaborItemPlanning> laborItemPlanningCollections, IEnumerable<Break> breaksCollections, UserFormatOptions userFormat)
        {
            Dictionary<Guid, LaborTaskGantt> laborTaskDictionary = new Dictionary<Guid, LaborTaskGantt>();
            List<LaborTaskGantt> taskWorkers = new();
            try
            {
                laborWorkerCollections = laborWorkerCollections.OrderBy(x => x.Schedule.Shift.Name);
                // Creamos los padres
                foreach (var laborWorker in laborWorkerCollections)
                {
                    LaborTaskGantt workerTaskLevel1 = new(userFormat)
                    {
                        id = laborWorker.Id,
                        WorkerId = laborWorker.Worker.Id,
                        WorkerName = laborWorker.Worker.Name,
                        ShiftName = laborWorker.Schedule?.Shift?.Name ?? string.Empty,
                        RolName = laborWorker.Worker.Rol.Name,
                        ShiftInitHour = FormatHourToAmPm(laborWorker.Schedule?.Shift?.InitHour + userFormat.TimeZoneOffSet ?? -1, userFormat),
                        ShiftEndHour = FormatHourToAmPm(laborWorker.Schedule?.Shift?.EndHour + userFormat.TimeZoneOffSet ?? -1, userFormat),
                        WorkTime = TimeFormatter.GetFormattedWorkTime(laborWorker.WorkTime),
                        Efficiency = Math.Round(laborWorker.Efficiency, 2),
                        Productivity = Math.Round(laborWorker.Productivity, 2),
                        Utility = Math.Round(laborWorker.Utility, 2),
                        Breaks = laborWorker.Breaks == 0 ? "" : laborWorker.Breaks.ToString(),
                        TotalOrders = laborWorker.TotalOrders,
                        ClosedOrders = laborWorker.ClosedOrders,
                        TeamName = laborWorker.Worker.Team.Name,
                        TooltipType = GanttTooltipType.LaborWorkerGeneral,
                        progress = Convert.ToInt32(laborWorker.Progress),
                        Resource = laborWorker.Worker.Name,
                        TotalProductivity = laborWorker.TotalProductivity,
                        FillProgress = true,
                        levelTask = 1,
                    };

                    if (breaksCollections.Any())
                    {
                        workerTaskLevel1.breaksGantts = new List<BreaksGantt>();
                        foreach ( var currentbreak in breaksCollections.Where(x => x.BreakProfileId == laborWorker.Schedule.BreakProfileId))
                        {
                            var initTime = TimeSpan.FromHours(currentbreak.InitBreak);
                            var endTime = TimeSpan.FromHours(currentbreak.EndBreak);
                            if (currentbreak.InitBreak != null && currentbreak.EndBreak != null)
                            {
                                workerTaskLevel1.breaksGantts.Add(new BreaksGantt
                                {
                                    BreakId = currentbreak.BreakProfileId,
                                    InitBreak = workerTaskLevel1.GetFormatDateForGantt(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, initTime.Hours, initTime.Minutes, 0)),
                                    EndBreak = workerTaskLevel1.GetFormatDateForGantt(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, endTime.Hours, endTime.Minutes, 0)),
                                });
                            }
                        }
                    }

                    // Añadimos la tarea Nivel 1
                    laborTaskDictionary.Add(workerTaskLevel1.id, workerTaskLevel1);


                    var itemPlanningsForWorker = laborItemPlanningCollections.Where(x => x.WorkerId == laborWorker.WorkerId);

                    Dictionary<string, LaborTaskGantt> laborProcessType = new Dictionary<string, LaborTaskGantt>();

                    foreach (var itemPlanning in itemPlanningsForWorker)
                    {
                        LaborTaskGantt orderPlanningTaskLevel2;

                        var orderWorkerPerFlow = itemPlanning.WFMLaborPerProcessType?.WFMLaborPerFlow;
                        var fechainicio = itemPlanningsForWorker.Where(x => x.InputOrder.IsOutbound == orderWorkerPerFlow.IsOutbound).Min(x => x.InitDate);
                        var fechaFin = itemPlanningsForWorker.Where(x => x.InputOrder.IsOutbound == orderWorkerPerFlow.IsOutbound).Max(x => x.EndDate);
                        if (!laborTaskDictionary.TryGetValue(orderWorkerPerFlow.Id, out orderPlanningTaskLevel2))
                        {
                            orderPlanningTaskLevel2 = new LaborTaskGantt(userFormat)
                            {
                                id = orderWorkerPerFlow.Id,
                                parentId = workerTaskLevel1.id,
                                IsOutbound = orderWorkerPerFlow.IsOutbound,
                                WorkerName = string.Empty,
                                ShiftName = string.Empty,
                                StartDate = fechainicio,
                                EndDate = fechaFin,
                                RolName = string.Empty,
                                TooltipType = GanttTooltipType.LaborWorkerOrder,
                                ShiftInitHour = FormatHourToAmPm(orderWorkerPerFlow.Schedule?.Shift?.InitHour + userFormat.TimeZoneOffSet ?? -1, userFormat),
                                ShiftEndHour = FormatHourToAmPm(orderWorkerPerFlow.Schedule?.Shift?.EndHour + userFormat.TimeZoneOffSet ?? -1, userFormat),
                                WorkTime = TimeFormatter.GetFormattedWorkTime(orderWorkerPerFlow.WorkTime),
                                Efficiency = Math.Round(orderWorkerPerFlow.Efficiency, 2),
                                Productivity = Math.Round(orderWorkerPerFlow.Productivity, 2),
                                Utility = Math.Round(orderWorkerPerFlow.Utility, 2),
                                TotalOrders = orderWorkerPerFlow.TotalOrders,
                                ClosedOrders = orderWorkerPerFlow.ClosedOrders,
                                TeamName = orderWorkerPerFlow.Worker?.Team?.Name ?? string.Empty,
                                CommintedDate = itemPlanning.InputOrder.AppointmentDate,
                                TotalProductivity = orderWorkerPerFlow.TotalProductivity,
                                levelTask = 2,
                            };

                            // Añadimos tarea del gannt Nivel 2
                            laborTaskDictionary.Add(orderWorkerPerFlow.Id, orderPlanningTaskLevel2);
                        }

                        LaborTaskGantt processTypeTask;

                        var processOrder = itemPlanning.WFMLaborPerProcessType;
                        if (!laborTaskDictionary.TryGetValue(processOrder.Id, out processTypeTask))
                        {
                            var fechainicio_ = itemPlanningsForWorker.Where(x => x.WFMLaborPerProcessType.ProcessType == processOrder.ProcessType).Min(x => x.InitDate);
                            var fechaFin_ = itemPlanningsForWorker.Where(x => x.WFMLaborPerProcessType.ProcessType == processOrder.ProcessType).Max(x => x.EndDate);
                            processTypeTask = new LaborTaskGantt(userFormat)
                            {
                                id = processOrder.Id,
                                parentId = orderWorkerPerFlow.Id,
                                IsOutbound = itemPlanning.InputOrder.IsOutbound,
                                ActivityTitle = processOrder.ProcessType,
                                WorkerName = string.Empty,
                                StartDate = fechainicio_,
                                EndDate = fechaFin_,
                                ShiftName = string.Empty,
                                RolName = string.Empty,
                                TooltipType = GanttTooltipType.LaborWorkerActivity,
                                ShiftInitHour = FormatHourToAmPm(processOrder.Schedule?.Shift?.InitHour ?? -1, userFormat),
                                ShiftEndHour = FormatHourToAmPm(processOrder.Schedule?.Shift?.EndHour ?? -1, userFormat),
                                WorkTime = TimeFormatter.GetFormattedWorkTime(processOrder.WorkTime),
                                Efficiency = Math.Round(processOrder.Efficiency, 2),
                                Productivity = Math.Round(processOrder.Productivity, 2),
                                Utility = Math.Round(processOrder.Utility, 2),
                                TotalActivities = processOrder.TotalProcesses,
                                ClosedActivities = processOrder.ClosedProcesses,
                                TeamName = processOrder.Worker?.Team?.Name ?? string.Empty,
                                progress = Convert.ToInt32(itemPlanning.InputOrder.Progress),
                                levelTask = 3,
                            };

                            laborTaskDictionary.Add(processTypeTask.id, processTypeTask);
                            laborTaskDictionary[processOrder.Id].segments = AddSegmentsForActivity(laborTaskDictionary[processOrder.Id], itemPlanning.Id, processTypeTask.UtcToTimeZoneOffSet(itemPlanning.InitDate).DateTime, processTypeTask.UtcToTimeZoneOffSet(itemPlanning.EndDate).DateTime, itemPlanning.InputOrder.Progress ?? 0, itemPlanning);                            
                        }
                        else
                        {
                            laborTaskDictionary[processOrder.Id].segments = AddSegmentsForActivity(laborTaskDictionary[processOrder.Id], itemPlanning.Id, processTypeTask.UtcToTimeZoneOffSet(itemPlanning.InitDate).DateTime, processTypeTask.UtcToTimeZoneOffSet(itemPlanning.EndDate).DateTime, itemPlanning.InputOrder.Progress ?? 0, itemPlanning);                            
                        }

                        laborTaskDictionary[orderPlanningTaskLevel2.id].progress = getProgress(laborTaskDictionary[processOrder.Id].segments);

                        if (processTypeTask.parentId != null)
                        {
                            Guid itemParentId = processTypeTask.parentId ?? Guid.Empty;
                            if (laborTaskDictionary[itemParentId].start == null)
                                laborTaskDictionary[itemParentId].StartDate = processTypeTask.StartDate;
                        }
                        if (orderPlanningTaskLevel2.parentId != null)
                        {
                            Guid itemId = orderPlanningTaskLevel2.parentId ?? Guid.Empty;
                            if (laborTaskDictionary[itemId].start == null)
                                laborTaskDictionary[itemId].StartDate = orderPlanningTaskLevel2.StartDate;
                        }
                    }
                }

                taskWorkers = TasksIndexConverter.AssignRootIndex((laborTaskDictionary.Values.ToList().Cast<LaborTaskGantt>().ToList()));
            }
            catch (Exception)
            {
                throw;
            }

            return  new GanttDataConvertDto<LaborTaskGantt>
            {
                TaskGantt = taskWorkers,
                DependenciesGantt = new List<DependenciesGantt>(),
            };

        }

        private static int getProgress(List<SegmentDto> segments)
        {
            if(segments.Any())
                return segments.Sum(i => i.progress) / segments.Count;
            else
                return 0;
        }

        public static string FormatHourToAmPm(double hourValue, UserFormatOptions userFormat)
        {
            if (hourValue < 0 || hourValue >= 24)
                return "";

            int hour = (int)Math.Floor(hourValue);
            double minutesDecimal = (hourValue - hour) * 60;
            int minutes = (int)Math.Round(minutesDecimal);

            var date = new DateTime(1, 1, 1, hour, minutes, 0);

            return date.ToUserTime(userFormat.HourFormat);
        }

        private static List<SegmentDto> AddSegmentsForActivity<T>(T ganttBase , Guid itemPlanningId, DateTime start, DateTime end, double progress, WFMLaborItemPlanning itemplanning) where T : GanttTaskBase
        {
            if (ganttBase.segments == null)
                ganttBase.segments = new List<SegmentDto>();

            double durationInSeconds = itemplanning.WorkTime;
            string formattedWorkTime = TimeFormatter.GetFormattedWorkTime(durationInSeconds);


            ganttBase.segments.Add(new SegmentDto
            {
                id = itemPlanningId,
                start = ganttBase.GetFormatDateForGantt(start),
                end = ganttBase.GetFormatDateForGantt(end),
                progress = Convert.ToInt32(progress),
                worktime = formattedWorkTime
            });

            return ganttBase.segments;
        }

        #endregion
    }
}
