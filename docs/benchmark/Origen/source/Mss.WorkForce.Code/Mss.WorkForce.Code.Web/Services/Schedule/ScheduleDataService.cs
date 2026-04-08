using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web.Services
{
    public class ScheduleDataService : IScheduleDataService
    {
        private readonly DataAccess _dataAccess;

        public ScheduleDataService(DataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public IEnumerable<YardResourceBase> GetSaturations(Guid planningId, YardResourceType resourceGroupType, double offset)
        {
            List<YardResourceBase> list = new();
            int counter = 1;

            var dockSaturations = _dataAccess.GetDockSaturations(planningId).GroupBy(s => s.DockId).Select(g => g.First()).OrderBy(s => s.Dock.Zone.Name).ToList();
            var stageSaturations = _dataAccess.GetStageSaturations(planningId).GroupBy(s => s.StageId).Select(g => g.First()).OrderBy(s => s.Stage.Zone.Name).ToList();
            var relations = _dataAccess.GetAvailableDockStageRelations();

            if (resourceGroupType == YardResourceType.Docks)
            {
				foreach (var sat in dockSaturations)
                {
                    // Crear Dock principal
                    var dock = list.FirstOrDefault(x => x.EntityId == sat.DockId) as DockResource;
                    if (dock == null)
                    {
                        eRosourceType resourceType;
                        
                        if (sat.Dock.AllowInbound && sat.Dock.AllowOutbound)
                            resourceType = eRosourceType.ResourceBoth;
                        else if (sat.Dock.AllowInbound && !sat.Dock.AllowOutbound)
                            resourceType = eRosourceType.ResourceIn;
                        else if (!sat.Dock.AllowInbound && sat.Dock.AllowOutbound)
                            resourceType = eRosourceType.ResourceOut;
                        else
                            resourceType = eRosourceType.None;

                        dock = new DockResource
                        {
                            Id = counter++,
                            EntityId = sat.DockId,
                            DockCode = sat.Dock.Zone.Name,
                            DockName = sat.Dock.Zone.Name,
                            Name = sat.Dock.Zone.Name,
                            ParentId = 0,
                            BackgroundCss = "mlx-scheduler-bg-white-color",
                            TextCss = "dxbl-black-font-color",
                            ResourceType = resourceType,
                            Appointments = new()
                        };

                        list.Add(dock);
                        
                        var relatedStageIds = relations
                            .Where(r => r.DockId == sat.DockId)
                            .Select(r => r.StageId)
                            .Distinct()
                            .ToList();

                        var relatedStages = stageSaturations
                            .Where(s => relatedStageIds.Contains(s.StageId))
                            .ToList();

                        foreach (var stageSat in relatedStages)
                        {
                            var stage = new StageResource
                            {
                                Id = counter++,
                                EntityId = stageSat.StageId,
                                Name = stageSat.Stage.Zone.Name,
                                ParentId = dock.Id,
                                BackgroundCss = "mlx-scheduler-bg-lightgray-color",
                                TextCss = "dxbl-black-font-color",
                                ResourceType = eRosourceType.ResourceIn,
                                Appointments = new(),
								IsGroup = false
							};

                            stage.Appointments.Add(new AppointmentMetrics
                            {
                                Accepted = true,
                                StartDate = stageSat.InitHour,
                                EndDate = stageSat.EndHour,
                                ResourceId = stage.Id,
                                TotalSaturation = stageSat.Saturation,
                                ActualUtilization = stageSat.RealUsage,
                                PlannedUtilization = stageSat.PlannedUsage,
                                TotalCapacity = stageSat.TotalCapacity
                            });

                            list.Add(stage);
                            dock.HasAnyChild = true;
                        }
                    }

                    // Agregar metrica al dock
                    dock.Appointments.Add(new AppointmentMetrics
                    {
                        Accepted = true,
                        StartDate = sat.InitHour,
                        EndDate = sat.EndHour,
                        ResourceId = dock.Id,
                        TotalSaturation = sat.Saturation,
                        ActualUtilization = sat.RealUsage,
                        PlannedUtilization = sat.PlannedUsage,
                        TotalCapacity = sat.TotalCapacity
                    });
                }
            }

            else if (resourceGroupType == YardResourceType.Stages)
            {
				foreach (var sat in stageSaturations)
                {
                    // Crear Stage principal
                    var stage = list.FirstOrDefault(x => x.EntityId == sat.StageId) as StageResource;
                    if (stage == null)
                    {
                        stage = new StageResource
                        {
                            Id = counter++,
                            EntityId = sat.StageId,
                            Name = sat.Stage.Zone.Name,
                            ParentId = 0,
                            BackgroundCss = "mlx-scheduler-bg-lightgray-color",
                            TextCss = "dxbl-black-font-color",
                            ResourceType = eRosourceType.ResourceIn,
                            Appointments = new()
                        };

                        list.Add(stage);
                        
                        var relatedDockIds = relations
                            .Where(r => r.StageId == sat.StageId)
                            .Select(r => r.DockId)
                            .Distinct()
                            .ToList();

                        var relatedDocks = dockSaturations
                            .Where(d => relatedDockIds.Contains(d.DockId))
                            .ToList();

                        foreach (var dockSat in relatedDocks)
                        {
                            eRosourceType resourceType;

                            if (dockSat.Dock.AllowInbound && dockSat.Dock.AllowOutbound)
                                resourceType = eRosourceType.ResourceBoth;
                            else if (dockSat.Dock.AllowInbound && !dockSat.Dock.AllowOutbound)
                                resourceType = eRosourceType.ResourceIn;
                            else if (!dockSat.Dock.AllowInbound && dockSat.Dock.AllowOutbound)
                                resourceType = eRosourceType.ResourceOut;
                            else
                                resourceType = eRosourceType.None;

                            var dock = new DockResource
                            {
                                Id = counter++,
                                EntityId = dockSat.DockId,
                                Name = dockSat.Dock.Zone.Name,
                                ParentId = stage.Id,
                                BackgroundCss = "mlx-scheduler-bg-white-color",
                                TextCss = "dxbl-black-font-color",
                                ResourceType = resourceType,
                                Appointments = new(),
                                IsGroup = false
                            };

                            dock.Appointments.Add(new AppointmentMetrics
                            {
                                Accepted = true,
                                StartDate = dockSat.InitHour,
                                EndDate = dockSat.EndHour,
                                ResourceId = dock.Id,
                                TotalSaturation = dockSat.Saturation,
                                ActualUtilization = dockSat.RealUsage,
                                PlannedUtilization = dockSat.PlannedUsage,
                                TotalCapacity = dockSat.TotalCapacity
                            });

                            list.Add(dock);
                            stage.HasAnyChild = true;
                        }
                    }

                    // Agregar metrica al Stage
                    stage.Appointments.Add(new AppointmentMetrics
                    {
                        Accepted = true,
                        StartDate = sat.InitHour,
                        EndDate = sat.EndHour,
                        ResourceId = stage.Id,
                        TotalSaturation = sat.Saturation,
                        ActualUtilization = sat.RealUsage,
                        PlannedUtilization = sat.PlannedUsage,
                        TotalCapacity = sat.TotalCapacity
                    });
                }
            }

            return list;
        }
    }
}
