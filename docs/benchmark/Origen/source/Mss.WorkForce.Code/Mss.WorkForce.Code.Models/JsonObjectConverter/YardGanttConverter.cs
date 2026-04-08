using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.JsonObjectConverter;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.ModelGantt.DataGanttPlanning;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models
{
    public static class YardGanttConverter
    {

        public static GanttDataConvertDto<YardTaskGantt> YardTaskGanttConvert(IEnumerable<YardMetricsAppointmentsPerDock> yardAppointmentsCollections, Dictionary<string, string?> trailers, UserFormatOptions userFormat)
        {
            Dictionary<Guid, YardTaskGantt> taskForYards = new();

            foreach (var yardAppointment in yardAppointmentsCollections)
            {

                var yardDock = yardAppointment.YardMetricsPerDock;

                YardTaskGantt yardDockTask;

                if (!taskForYards.TryGetValue(yardDock.Id, out yardDockTask))
                {
                    yardDockTask = new YardTaskGantt(userFormat)
                    {
                        id = yardDock.Id,
                        ProcessType = yardDock.ProcessType,
                        DockName = yardDock.Dock.Zone?.Name ?? string.Empty,
                        AttendedAppointments = yardDock.AttendedAppointments,
                        TotalAppointments = yardDock.TotalAppointments,
                        Saturation = yardDock.Saturation,
                        TooltipType = GanttTooltipType.YardGeneral,
                        segments = new List<SegmentDto>(),
                        levelTask = 1,
                    };

                    taskForYards.Add(yardDockTask.id, yardDockTask);
                }

                YardTaskGantt appointmentTask = new(userFormat);

                appointmentTask.id = yardAppointment.Id;
                appointmentTask.parentId = yardDockTask.id;
                appointmentTask.progress = (int)yardAppointment.Progress;
                appointmentTask.Customer = yardAppointment.Customer;
                appointmentTask.YardCode = yardAppointment.YardCode;
                appointmentTask.AppointmentCode = yardAppointment.AppointmentCode;
                appointmentTask.VehicleType = yardAppointment.VehicleType;
                appointmentTask.License = yardAppointment.License;
                appointmentTask.TotalOrders = yardAppointment.TotalOrders;
                appointmentTask.AppointmentDate = yardDockTask.UtcToTimeZoneOffSet(yardAppointment.AppointmentDate).ToUserTime(userFormat.FullDate);
                appointmentTask.CompletedOrders = yardAppointment.CompletedOrders;
                appointmentTask.ProcessType = yardAppointment.ProcessType;
                appointmentTask.IsOutbound = yardAppointment.ProcessType == "Loading";
                appointmentTask.TooltipType = GanttTooltipType.YardOrder;
                appointmentTask.VehicleCode = trailers.GetValueOrDefault(yardAppointment.License) ?? string.Empty;
                appointmentTask.BackgroundColorRow = GetBackgroundColor(yardAppointment.ProcessType);
                appointmentTask.color = GetBackgroundProgressBarColor(yardAppointment.ProcessType);
                //Datos para calcular si esta o no citas sobre puestas 
                appointmentTask.StartDate = yardAppointment.StartDate;
                appointmentTask.EndDate = yardAppointment.EndDate;
                appointmentTask.CommintedDate = yardDockTask.UtcToTimeZoneOffSet(yardAppointment.AppointmentDate);
                appointmentTask.IsChildTask = true;
                appointmentTask.levelTask = 2;

                taskForYards.Add(appointmentTask.id, appointmentTask);

                yardDockTask.segments.Add(new SegmentDto
                {
                    id = appointmentTask.id,
                    end = appointmentTask.end,
                    start = appointmentTask.start,
                    progress = appointmentTask.progress,
                    StartDate = appointmentTask.StartDate,
                    EndtDate = appointmentTask.EndDate,
                    isOnTime = appointmentTask.isOnTime,
                });

            }

            SetIsOnTimeTask(ref taskForYards);

            var taskYardConverter = TasksIndexConverter.AssignRootIndex((taskForYards.Values.ToList().Cast<YardTaskGantt>().ToList()));

            return new GanttDataConvertDto<YardTaskGantt>
            {
                TaskGantt = taskYardConverter,
                DependenciesGantt = new List<DependenciesGantt>(),
            };

        }

        private static string GetBackgroundColor(string activityType)
        {
            switch (activityType)
            {
                case "Loading":
                    return GanttColors.BlueColor;

                case "Inbound":
                    return GanttColors.Green100Color;

                default:
                    return GanttColors.DefaultColor;
            }
        }

        private static string GetBackgroundProgressBarColor(string activityType)
        {
            switch (activityType)
            {
                case "Loading":
                    return GanttColors.Blue600Color;

                case "Inbound":
                    return GanttColors.Green800Color;

                default:
                    return GanttColors.Smoke800Color;
            }

        }

        private static void SetIsOnTimeTask(ref Dictionary<Guid, YardTaskGantt> taskForYards)
        {
            foreach (var kvp in taskForYards.Where(x => x.Value.TooltipType == GanttTooltipType.YardGeneral))
            {
                if (kvp.Value.segments is List<SegmentDto> segmentDtos)
                {
                    kvp.Value.isOnTime = !segmentDtos.Any(x => !x.isOnTime);
                    kvp.Value.isOffOverlay = HasOverlappingSegments(segmentDtos);
                }
            }
        }

        private static bool HasOverlappingSegments(List<SegmentDto> segments)
        {
            try
            {
                // Ignorar segmentos inválidos (con fechas nulas)
                var validSegments = segments
                    .Where(s => s.StartDate != null && s.EndtDate != null)
                    .OrderBy(s => s.StartDate)
                    .ToList();

                for (int i = 0; i < validSegments.Count - 1; i++)
                {
                    var current = validSegments[i];
                    var next = validSegments[i + 1];

                    if (next.StartDate < current.EndtDate)
                        return false;
                }
            }
            catch
            {
                throw;
            }
            return true;
        }

    }
}
